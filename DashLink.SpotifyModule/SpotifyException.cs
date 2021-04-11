using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Spotify
{
    public class SpotifyException : Exception
    {
        public SpotifyException() : base()
        { }

        public SpotifyException(string message) : base(message)
        { }
    }
}
