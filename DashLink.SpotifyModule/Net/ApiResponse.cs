using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net
{

    public class ApiResponse<T>
    {
        public bool Successful => Response.IsSuccessStatusCode;
        public HttpResponseMessage Response { get; }
        public T Object { get; }

        public ApiResponse(HttpResponseMessage response, T obj)
        {
            Response = response;
            Object = obj;
        }
    }
}
