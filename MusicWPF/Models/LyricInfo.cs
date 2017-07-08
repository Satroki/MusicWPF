using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MusicWPF.Models
{
    public class LyricInfo
    {
        public LyricInfo(string lrcString)
        {
            LrcLines = InitLines(lrcString);
        }

        public static string LrcFormat(string oriLrc)
        {
            if (string.IsNullOrWhiteSpace(oriLrc))
                return string.Empty;
            var lines = oriLrc.Split('\n').Where(line => line.StartsWith("["));
            var offset = new TimeSpan();
            var os = lines.FirstOrDefault(o => Regex.IsMatch(o, @"\[offset:[-\+]?\d+\]"));
            if (os != null)
                offset = TimeSpan.FromMilliseconds(int.Parse(Regex.Match(os, @"-?\d+").Value));

            var list = new List<LrcLine>();
            foreach (var item in lines)
            {
                var matches = Regex.Matches(item, @"\[(\d{1,2}):(\d{1,2})([\.:](\d{1,3}))?\]").Cast<Match>();
                if (matches.Count() == 0)
                    continue;
                var content = item.Substring(matches.Sum(m => m.Value.Length)).Trim();
                foreach (Match m in matches)
                {
                    var s = $"0:{m.Groups[1].Value}:{m.Groups[2].Value}";
                    if (m.Groups[4].Success)
                        s = $"{s}.{m.Groups[4].Value}";
                    list.Add(new LrcLine()
                    {
                        Content = content,
                        TimePoint = TimeSpan.Parse(s).Add(offset),
                    });
                }
            }
            return string.Join("\n", list.OrderBy(lrc => lrc.TimePoint));
        }

        public LrcLine[] LrcLines { get; set; }

        public string Lyric => string.Join("\n", LrcLines.AsEnumerable());

        private LrcLine[] InitLines(string lrcString)
        {
            if (string.IsNullOrWhiteSpace(lrcString))
                return null;
            var lines = lrcString.Split('\n');
            var list = new List<LrcLine>();
            foreach (var item in lines)
            {
                list.Add(new LrcLine()
                {
                    Content = item.Substring(10),
                    TimePoint = TimeSpan.Parse("00:" + item.Substring(1, 8)),
                });
            }
            return list.ToArray();
        }

        public string Seek(TimeSpan ts) => LrcLines.LastOrDefault(ll => ll.TimePoint < ts)?.Content;

        private int index = 0;
        public string Sequence(TimeSpan ts)
        {
            if (index >= LrcLines.Length)
                return string.Empty;
            while (index < LrcLines.Length - 1)
            {
                if (ts >= LrcLines[index].TimePoint && ts < LrcLines[index + 1].TimePoint)
                    return LrcLines[index].Content;
                index++;
            }
            return string.Empty;
        }
    }

    public class LrcLine
    {
        public TimeSpan TimePoint { get; set; }
        public string Content { get; set; }
        public override string ToString() => TimePoint.ToString(@"\[mm\:ss\.ff\]") + Content;
    }
}
