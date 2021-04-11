using DashLink.Core.Data;
using System;

namespace DashLink.Core.IO
{
    public class LcdCache
    {
        private class Line
        {
            internal string text;
            internal bool changed;
            internal int scrollPos;
        }

        public int LineCount { get; }
        public int ScrollCount { get; set; }
        private readonly Line[] lines;

        public LcdCache() : this(2)
        { }

        public LcdCache(int lineCount)
        {
            LineCount = lineCount > 0 ? lineCount : throw new ArgumentOutOfRangeException(nameof(lineCount), "Line count must be greater than or equal to 1");
            ScrollCount = 1;
            lines = new Line[LineCount];
        }

        public void SetLine(int line, string text)
        {
            var l = lines[line >= 0 && line < LineCount ? line : throw new IndexOutOfRangeException("Line index out of range")];
            text = text ?? string.Empty;
            l.changed = !text.Equals(l.text);
            l.text = text;
            l.scrollPos = 0;
        }

        public void ResetScroll(int line, bool setFlag)
        {
            var l = lines[line >= 0 && line < LineCount ? line : throw new IndexOutOfRangeException("Line index out of range")];
            l.changed = l.changed || setFlag;
            l.scrollPos = 0;
        }

        public string GetLine(int line, bool resetFlag)
        {
            var l = lines[line >= 0 && line < LineCount ? line : throw new IndexOutOfRangeException("Line index out of range")];
            l.changed = !resetFlag && l.changed;
            return l.text ?? string.Empty;
        }

        public bool HasChanged(int line)
        {
            return lines[line >= 0 && line < LineCount ? line : throw new IndexOutOfRangeException("Line index out of range")].changed;
        }

        public string GetLineFormatted(int line, bool resetFlag, int lineLength, LcdLineOverflow overflow)
        {
            var str = GetLine(line, resetFlag);
            int len = str.Length;
            lineLength = lineLength > 0 ? lineLength : throw new ArgumentOutOfRangeException(nameof(lineLength), "Line length must be greater than or equal to 1");

            if (len <= lineLength)
            {
                return str;
            }
            int trimLen = Math.Min(len, lineLength);

            switch (overflow)
            {
                case LcdLineOverflow.OneDot:
                    return trimLen > 2 ? str.Substring(0, trimLen - 1) + '.' : str.Substring(0, trimLen);
                case LcdLineOverflow.TwoDots:
                    return trimLen > 3 ? str.Substring(0, trimLen - 2) + ".." : str.Substring(0, trimLen);
                case LcdLineOverflow.ThreeDots:
                    return trimLen > 4 ? str.Substring(0, trimLen - 3) + "..." : str.Substring(0, trimLen);
                case LcdLineOverflow.Scroll:
                    var l = lines[line];
                    int start = l.scrollPos;
                    l.scrollPos = (start + 1) % len;
                    var longStr = l.text + l.text;
                    return longStr.Substring(start, trimLen);
                default:
                    return str.Substring(0, trimLen);
            }
        }
    }
}
