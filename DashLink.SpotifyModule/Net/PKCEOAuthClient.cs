using DashLink.Spotify.Net.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace DashLink.Spotify.Net
{
    public class PKCEOAuthClient : BaseOAuthClient
    {
        public ClientInformation AuthData;

        private string codeVerifier;
        private string codeChallenge;

        private string state;

        public override bool ValidToken => !string.IsNullOrEmpty(AuthData.AccessToken);
        public override bool CanRefresh => !(string.IsNullOrEmpty(AuthData.RefreshUri) || string.IsNullOrEmpty(AuthData.RefreshToken) || string.IsNullOrEmpty(AuthData.ClientId));

        public PKCEOAuthClient()
        {
            codeVerifier = CreateCodeVerifier();
            codeChallenge = CreateSHA256CodeChallenge(codeVerifier);
        }

        public PKCEOAuthClient(string codeVerifier)
        {
            this.codeVerifier = codeVerifier ?? throw new ArgumentNullException(nameof(codeVerifier));
            codeChallenge = CreateSHA256CodeChallenge(codeVerifier);
        }

        public PKCEOAuthClient(ClientInformation authData)
        {
            AuthData = authData;
        }

        public string GetCodeChallenge()
        {
            return codeChallenge;
        }

        public string NewState()
        {
            state = CreateCodeVerifier();
            return state;
        }

        public bool CompareState(string state)
        {
            return this.state.Equals(state);
        }

        public async Task<OAuthToken> ExchangeCode(string endpoint, string code, string clientId, string redirectUri)
        {
            if (string.IsNullOrEmpty(codeVerifier)) throw new Exception("Code exchange cannot occur as code was not generated");
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri)) return null;

            using (var client = new HttpClient())
            {
                var requestUri = new Uri(endpoint);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));

                HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                        { "client_id", clientId },
                        { "grant_type", "authorization_code" },
                        { "code", code },
                        { "redirect_uri", redirectUri },
                        { "code_verifier", codeVerifier }
                    })
                };

                var response = await client.SendAsync(requestMsg);

                if (response.IsSuccessStatusCode)
                {
                    OAuthToken oar = JsonSerializer.Deserialize<OAuthToken>(await response.Content.ReadAsStringAsync());

                    AuthData.AccessToken = oar.AccessToken;
                    AuthData.RefreshToken = oar.RefreshToken;

                    return oar;
                }
            }
            return null;
        }

        public async Task<OAuthToken> ExchangeCode(ClientInformation authData, string code)
        {
            return await ExchangeCode(authData.RefreshUri, code, authData.ClientId, authData.RequestUri);
        }

        public async Task<OAuthToken> Refresh(string refreshUrl, string refreshToken, string clientId)
        {
            using (var client = new HttpClient())
            {
                var requestUri = new Uri(refreshUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));

                HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>{
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken },
                        { "client_id", clientId }
                    })
                };

                var response = await client.SendAsync(requestMsg);

                if (response.IsSuccessStatusCode)
                {
                    OAuthToken oar = JsonSerializer.Deserialize<OAuthToken>(await response.Content.ReadAsStringAsync());

                    AuthData.AccessToken = oar.AccessToken;
                    AuthData.RefreshToken = oar.RefreshToken;

                    RaiseRefreshEvent(new AuthenticationRefreshEventArgs(true, response.StatusCode, oar));
                    return oar;
                }

                RaiseRefreshEvent(new AuthenticationRefreshEventArgs(false, response.StatusCode, null));
            }
            return null;
        }

        public override async Task<OAuthToken> Refresh()
        {
            if (!CanRefresh) return null;
            return await Refresh(AuthData.RefreshUri, AuthData.RefreshToken, AuthData.ClientId);
        }

        public static string CreateCodeVerifier(int length = 64)
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[length];
                rng.GetBytes(tokenData);

                string codeVerifier = HttpServerUtility.UrlTokenEncode(tokenData);
                codeVerifier = codeVerifier.Substring(0, codeVerifier.Length - 1);

                return codeVerifier;
            }
        }

        public static string CreateSHA256CodeChallenge(string codeVerifier)
        {
            codeVerifier = codeVerifier ?? throw new ArgumentNullException(nameof(codeVerifier));
            using (var sha256 = SHA256.Create())
            {
                var challengeData = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

                var codeChallenge = HttpServerUtility.UrlTokenEncode(challengeData);
                codeChallenge = codeChallenge.Substring(0, codeChallenge.Length - 1);

                return codeChallenge;
            }
        }

        protected override async Task<HttpResponseMessage> SendRequest(string request, string jsonBody, bool requireAuth, HttpMethod method, bool allowRefreshing = true)
        {
            if (!ValidToken && CanRefresh) await Refresh();
            if (!ValidToken) return null;

            using (var client = new HttpClient())
            {
                var requestUri = new Uri(request);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));
                if (requireAuth) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Bearer, AuthData.AccessToken ?? throw new NullReferenceException("The token for this client does not exist yet"));

                HttpRequestMessage requestMsg = new HttpRequestMessage(method, requestUri);
                if (jsonBody != null)
                {
                    requestMsg.Content = new StringContent(jsonBody, Encoding.UTF8, ApplicationJson);
                }

                var response = await client.SendAsync(requestMsg);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AuthData.AccessToken = null;
                    if (CanRefresh && allowRefreshing)
                    {
                        return await SendRequest(request, jsonBody, requireAuth, method, false);
                    }
                }

                return response;
            }
        }
    }
}
