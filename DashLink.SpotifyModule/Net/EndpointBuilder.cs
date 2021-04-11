using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DashLink.Spotify.Net
{
    public static class EndpointBuilder
    {
        private const string BaseURL = "https://api.spotify.com/v1";
        private const string Albums = BaseURL + "/albums";
        private const string Artists = BaseURL + "/artists";
        private const string Player = BaseURL + "/me/player";

        public static string PKCEAuthorizeEndpoint(string clientId, string redirectUri, string codeChallenge, string scope, string state, bool urlEncode)
        {
            return NewBuilder("https://accounts.spotify.com/authorize?response_type=code&client_id=").Append(clientId).Append("&redirect_uri=")
                .Append(urlEncode ? HttpUtility.UrlEncode(redirectUri) : redirectUri).Append("&code_challenge=").Append(codeChallenge).Append("&code_challenge_method=S256&scope=")
                .Append(urlEncode ? HttpUtility.UrlEncode(scope) : scope).Append("&state=").Append(state).ToString();
        }

        public static string TokenEndpoint() => "https://accounts.spotify.com/api/token";

        public static string GetAlbums() => Albums;

        public static string GetAlbumByID(string id)
        {
            return NewBuilder(Albums).Append('/').Append(id.ValidateSpotifyID()).ToString();
        }

        public static string GetTracksFromAlbumByID(string id, int limit = 0, int offset = 0, string market = null)
        {
            var b = NewBuilder(Albums).Append('/').Append(id.ValidateSpotifyID()).Append("/tracks");
            var c = false;
            if (limit.Wrap(0, 50) > 0)
            {
                b.Append("?limit=").Append(limit);
                c = true;
            }
            if (offset.Wrap(0, 99) > 0)
            {
                b.Append(c ? '&' : '?').Append("offset=").Append(offset);
                c = true;
            }
            if (!string.IsNullOrWhiteSpace(market))
            {
                b.Append(c ? '&' : '?').Append("market=").Append(market.ValidateISOAlpha2());
            }
            return b.ToString();
        }

        public static string GetArtists()
            => Artists;

        public static string GetArtistByID(string id)
            => NewBuilder(Artists).Append('/').Append(id.ValidateSpotifyID()).ToString();

        public static string GetTracksFromArtistByID(string id, int limit = 0, int offset = 0, string country = null)
        {
            var b = NewBuilder(Albums).Append('/').Append(id.ValidateSpotifyID()).Append("/albums");
            var c = false;
            if (limit.Wrap(0, 50) > 0)
            {
                b.Append("?limit=").Append(limit);
                c = true;
            }
            if (offset.Wrap(0, 99) > 0)
            {
                b.Append(c ? '&' : '?').Append("offset=").Append(offset);
                c = true;
            }
            if (!string.IsNullOrWhiteSpace(country))
            {
                b.Append(c ? '&' : '?').Append("country=").Append(country.ValidateISOAlpha2());
            }
            return b.ToString();
        }

        //	/v1/artists/{id}/top-tracks
        // 	/v1/artists/{id}/related-artists

        public static string GetCurrentPlaybackInformation(string market = null, bool includeEpisodeType = false)
        {
            var b = NewBuilder(Player);
            bool c = false;
            if (!string.IsNullOrWhiteSpace(market))
            {
                b.Append('?').Append("market=").Append(market.ToLower().Equals("from_token") ? "from_token" : market.ValidateISOAlpha2());
                c = true;
            }
            if (includeEpisodeType)
            {
                b.Append(c ? '&' : '?').Append("additional_types=episode");
            }
            return b.ToString();
        }

        public static string GetCurrentPlayingTrack(string market = null)
        {
            return NewBuilder(Player).Append("/currently-playing?market=").Append(market == null || market.ToLower().Equals("from_token") ? "from_token" : market.ValidateISOAlpha2()).ToString();
        }

        public static string StartOrResumePlayback()
        {
            return NewBuilder(Player).Append("/play").ToString();
        }

        public static string PausePlayback()
        {
            return NewBuilder(Player).Append("/pause").ToString();
        }

        public static string SkipToNextTrack()
        {
            return NewBuilder(Player).Append("/next").ToString();
        }

        public static string SkipToLastTrack()
        {
            return NewBuilder(Player).Append("/previous").ToString();
        }

        public static string SeekToPositionInTrack(int positionMillis)
        {
            return NewBuilder(Player).Append("/seek?position_ms=").Append(positionMillis.Wrap(0, int.MaxValue)).ToString();
        }

        public static string SetRepeatMode(RepeatMode mode)
        {
            return NewBuilder(Player).Append("/repeat?state=").Append(mode == RepeatMode.Track ? "track" : (mode == RepeatMode.Context ? "context" : "off")).ToString();
        }

        public static string SetVolume(int volumePercent)
        {
            return NewBuilder(Player).Append("/volume?volume_percent=").Append(volumePercent.Wrap(0, 100)).ToString();
        }

        public static string SetShuffleState(bool enableShuffle)
        {
            return NewBuilder(Player).Append("/shuffle?state=").Append(enableShuffle ? "true" : "false").ToString();
        }

        private static StringBuilder NewBuilder(string baseStr) => new StringBuilder(baseStr);
    }
}
