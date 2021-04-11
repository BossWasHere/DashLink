using DashLink.Spotify.Net.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net
{
    public class OAuthClient : BaseOAuthClient
    {
        public ClientInformation AuthData;

        public override bool ValidToken => !string.IsNullOrEmpty(AuthData.AccessToken);
        public override bool CanRefresh => !(string.IsNullOrEmpty(AuthData.RefreshUri) || string.IsNullOrEmpty(AuthData.RefreshToken)
                || string.IsNullOrEmpty(AuthData.ClientId) || string.IsNullOrEmpty(AuthData.ClientSecret));

        public OAuthClient(ClientInformation authData)
        {
            AuthData = authData;
        }

        protected override async Task<HttpResponseMessage> SendRequest(string request, string jsonBody, bool requireAuth, HttpMethod method, bool allowRefreshing = true)
        {
            if (!ValidToken && CanRefresh) await Refresh();
            if (!ValidToken) return null;

            using (var client = new HttpClient())
            {
                var requestUri = new Uri(request);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));
                if (requireAuth) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Bearer, AuthData.AccessToken);

                HttpRequestMessage requestMsg = new HttpRequestMessage(method, requestUri);
                if (jsonBody != null)
                {
                    requestMsg.Content = new StringContent(jsonBody, Encoding.UTF8, ApplicationJson);
                }

                var response = await client.SendAsync(requestMsg);

                if (response.StatusCode == HttpStatusCode.Unauthorized) AuthData.AccessToken = null;

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

        public async Task<OAuthToken> Refresh(string refreshUri, string refreshToken, string applicationAuthentication)
        {
            using (var client = new HttpClient())
            {
                var requestUri = new Uri(refreshUri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Basic, applicationAuthentication);

                HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>{
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken }
                    })
                };

                var response = await client.SendAsync(requestMsg);

                if (response.IsSuccessStatusCode)
                {
                    OAuthToken oar = JsonSerializer.Deserialize<OAuthToken>(await response.Content.ReadAsStringAsync());
                    AuthData.AccessToken = oar.AccessToken;

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
            return await Refresh(AuthData.RefreshUri, AuthData.RefreshToken, AuthData.ClientId + ':' + AuthData.ClientSecret);
        }
    }
}
