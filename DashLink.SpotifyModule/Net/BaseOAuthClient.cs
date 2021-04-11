using DashLink.Spotify.Net;
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
    public abstract class BaseOAuthClient
    {
        public const string ApplicationJson = "application/json";
        public const string ApplicationUrlEncoded = "application/x-www-form-urlencoded";
        public const string Basic = "Basic";
        public const string Bearer = "Bearer";

        public abstract bool ValidToken { get; }
        public abstract bool CanRefresh { get; }
        public delegate void AuthenticationRefreshHandler(object sender, AuthenticationRefreshEventArgs args);
        public event AuthenticationRefreshHandler OnAuthenticationRefreshEvent;

        public abstract Task<OAuthToken> Refresh();

        public async Task<HttpResponseMessage> Get(string request, bool requireAuth)
        {
            return await SendRequest(request, null, requireAuth, HttpMethod.Get);
        }

        public async Task<ApiResponse<T>> Get<T>(string request, bool requireAuth)
        {
            return await SendRequest<T>(request, null, requireAuth, HttpMethod.Get);
        }

        public async Task<HttpResponseMessage> Post(string request, string jsonBody, bool requireAuth)
        {
            return await SendRequest(request, jsonBody, requireAuth, HttpMethod.Post);
        }

        public async Task<ApiResponse<T>> Post<T>(string request, string jsonBody, bool requireAuth)
        {
            return await SendRequest<T>(request, jsonBody, requireAuth, HttpMethod.Post);
        }

        public async Task<HttpResponseMessage> Put(string request, string jsonBody, bool requireAuth)
        {
            return await SendRequest(request, jsonBody, requireAuth, HttpMethod.Put);
        }

        public async Task<ApiResponse<T>> Put<T>(string request, string jsonBody, bool requireAuth)
        {
            return await SendRequest<T>(request, jsonBody, requireAuth, HttpMethod.Put);
        }

        public async Task<HttpResponseMessage> Delete(string request, bool requireAuth)
        {
            return await SendRequest(request, null, requireAuth, HttpMethod.Delete);
        }

        public async Task<ApiResponse<T>> Delete<T>(string request, bool requireAuth)
        {
            return await SendRequest<T>(request, null, requireAuth, HttpMethod.Delete);
        }

        protected abstract Task<HttpResponseMessage> SendRequest(string request, string jsonBody, bool requireAuth, HttpMethod method, bool allowRefreshing = true);

        protected virtual async Task<ApiResponse<T>> SendRequest<T>(string request, string jsonBody, bool requireAuth, HttpMethod method, bool allowRefreshing = true)
        {
            var response = await SendRequest(request, jsonBody, requireAuth, method, allowRefreshing);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new ApiResponse<T>(response, JsonSerializer.Deserialize<T>(await response.Content.ReadAsByteArrayAsync()));
            }
            else
            {
                return new ApiResponse<T>(response, default);
            }
        }

        protected void RaiseRefreshEvent(AuthenticationRefreshEventArgs args)
        {
            OnAuthenticationRefreshEvent?.Invoke(this, args);
        }
    }
}
