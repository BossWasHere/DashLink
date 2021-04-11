using DashLink.Spotify.Net.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net
{
    public class AuthenticationRefreshEventArgs : EventArgs
    {
        public bool Successful { get; set; }
        public HttpStatusCode Code { get; set; }
        public OAuthToken NewToken { get; set; }

        public AuthenticationRefreshEventArgs(bool success, HttpStatusCode code, OAuthToken newToken)
        {
            Successful = success;
            Code = code;
            NewToken = newToken;
        }
    }
}
