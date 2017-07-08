using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MusicWPF.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace MusicWPF
{
    internal static class Methods
    {
        public static async Task<int> AddMusicFile(string directoryName)
        {
            return await Task.Run(() =>
            {
                var files = new DirectoryInfo(directoryName).GetFiles("*.flac", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    using (var tag = TagLib.File.Create(f.FullName))
                    {
                        var mf = new MusicFile()
                        {
                            Album = tag.Tag.Album,
                            Artist = string.Join(",", tag.Tag.Performers),
                            FileSize_MB = (double)f.Length / 1048576,
                            FullName = f.FullName,
                            Title = tag.Tag.Title,
                            Name = f.Name,
                        };
                        MDModel.Model.MusicFile.Add(mf);
                    }
                }
                return MDModel.Model.SaveChanges();
            });
        }

        public static List<string> ImportListFile(string fileName, ListType type)
        {
            using (var sr = File.OpenText(fileName))
            {
                switch (type)
                {
                    case ListType.M3U8:
                        {
                            var line = sr.ReadLine().ToUpper();
                            if (!line.Contains("#EXTM3U"))
                                return null;
                            line = sr.ReadLine();
                            var temp = new List<string>();
                            while (!string.IsNullOrWhiteSpace(line))
                            {
                                if (line[0] == '#')
                                {
                                    line = sr.ReadLine();
                                    continue;
                                }
                                temp.Add(line);
                                line = sr.ReadLine();
                            }
                            return temp;
                        }
                    default: return null;
                }
            }
        }

        public static int AddDirectory(IEnumerable<DirectoryInfo> diList, string className)
        {
            var dllist = diList.Select(di =>
                new DirectoryList()
                {
                    Name = di.Name,
                    FullName = di.FullName,
                    ClassName = className,
                }
            );
            MDModel.Model.DirectoryList.AddRange(dllist);
            return MDModel.Model.SaveChanges();
        }

        public static int AddDirectory(DirectoryInfo di, string className)
        {
            var dl = new DirectoryList()
            {
                Name = di.Name,
                FullName = di.FullName,
                ClassName = className,
            };
            MDModel.Model.DirectoryList.Add(dl);
            return MDModel.Model.SaveChanges();
        }

        public static async Task<int> ReloadPlayList(PlayList pl)
        {
            return await Task.Run(() =>
            {
                int count = 0;
                var newLines = ImportListFile(pl.Source, ListType.M3U8);
                if (newLines == null)
                    return 0;
                var dict = pl.ListFiles.ToDictionary(lf => lf.MusicFile.FullName);
                var oldLines = dict.Keys.AsEnumerable();
                var del = oldLines.Except(newLines).Select(ol => dict[ol]);
                var add = newLines.Except(oldLines).Select(nl => MDModel.Model.MusicFile.SingleOrDefault(m => m.FullName == nl));

                MDModel.Model.ListFiles.RemoveRange(del);
                //foreach (var di in del)
                //    pl.ListFiles.Remove(di);
                count += MDModel.Model.SaveChanges();

                foreach (var ai in add)
                {
                    if (ai == null)
                        continue;
                    var lf = new ListFiles()
                    {
                        FileId = ai.FileId,
                        IsChecked = true,
                        ListId = pl.ListId,
                    };
                    pl.ListFiles.Add(lf);
                }
                count += MDModel.Model.SaveChanges();
                return count;
            });
        }

        public static bool ExportLrc(MusicFile mf, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(mf.Lyric))
                return false;
            var name = mf.FullName.Substring(0, mf.FullName.LastIndexOf('.')) + ".lrc";
            if ((!overwrite) && File.Exists(name))
                return false;
            File.WriteAllText(name, mf.Lyric, Encoding.UTF8);
            return true;
        }

        public static string TxtToLrc(string fileName)
        {
            try
            {
                var str = File.ReadAllText(fileName, Encoding.UTF8);
                if (fileName.ToUpper().EndsWith(".LRC"))
                    return str;
                else
                {
                    dynamic json = JsonConvert.DeserializeObject(str);
                    return json.lrc.lyric.Value;
                }
            }
            catch
            { return string.Empty; }
        }

        public const string ListData = "list.dat";
        public static void SaveListOrder(this IEnumerable<ListFiles> list)
        {
            using (var sw = File.CreateText(ListData))
            {
                foreach (var lf in list)
                    sw.WriteLine(lf.Id);
            }
        }

        public static ObservableCollection<ListFiles> LoadListOrder(ObservableCollection<ListFiles> list)
        {
            if (!File.Exists(ListData))
                return list;
            var ids = File.ReadAllLines(ListData);
            if (ids.Length == 0)
                return list;
            var query = from id in ids
                        from lf in list
                        where lf.Id.ToString() == id
                        select lf;
            return new ObservableCollection<ListFiles>(query);
        }
        public enum ListType { M3U8 };
    }
}
