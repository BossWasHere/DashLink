using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net
{
    public static class DataValidation
    {
        public static string ValidateSpotifyID(this string s)
        {
            if (s == null || s.Length != 22) throw new ArgumentException("Invalid Spotify ID");
            return s;
        }

        public static string ValidateISOAlpha2(this string s)
        {
            if (s == null || s.Length != 2) throw new ArgumentException("Invalid ISO 3166-1 alpha-2 Country Code");
            return s;
        }

        public static int AssertLessOrEqual(this int i, int larger)
        {
            if (i > larger) throw new ArgumentException("Value is larger than maximum");
            return i;
        }

        public static int AssertGreaterOrEqual(this int i, int smaller)
        {
            if (i < smaller) throw new ArgumentException("Value is smaller than minimum");
            return i;
        }

        public static int Wrap(this ref int i, int min, int max)
        {
            return i = Math.Min(Math.Max(i, min), max);
        }
    }
}
