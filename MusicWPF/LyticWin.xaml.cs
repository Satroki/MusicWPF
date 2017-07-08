using MusicWPF.Models;
using MusicWPF.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace MusicWPF
{
    /// <summary>
    /// LyticWin.xaml 的交互逻辑
    /// </summary>
    public partial class LyricWin : Window
    {
        public LyricWin(MusicFile mf)
        {
            InitializeComponent();
            lrcTemp = Settings.Default.TempPath;
            fsw.Path = lrcTemp;
            fsw.Created += Fsw_Created;
            loadMF(mf);

            checkBox.Checked += checkedChanged;
            checkBox.Unchecked += checkedChanged;
            checkedChanged(null, null);
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(1000);
            var fi = new FileInfo(e.FullPath);
            if (fi.Length < 20 * 1024)
            {
                var lrc = Methods.TxtToLrc(fi.FullName);
                var str = LyricInfo.LrcFormat(lrc);
                if (string.IsNullOrEmpty(str))
                    return;
                Dispatcher.Invoke(() =>
                {
                    tbLrc.Text = str;
                    SaveLrc();
                });
            }
        }

        private void loadMF(MusicFile mf)
        {
            this.mf = mf;
            Title = mf.DisplayName;
            tbLrc.Text = mf.Lyric;
        }

        private MusicFile mf;
        private FileSystemWatcher fsw = new FileSystemWatcher();
        private string lrcTemp;

        private void import_Click(object sender, RoutedEventArgs e)
        {
            tbLrc.Text = LyricInfo.LrcFormat(tbLrc.Text);
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            SaveLrc();
            Close();
        }

        private void SaveLrc()
        {
            mf.Lyric = LyricInfo.LrcFormat(tbLrc.Text);
            MDModel.Model.SaveChanges();
            mf.ReloadLrc();
        }

        private void offset_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbOffset.Text))
                return;
            tbLrc.Text = $"[offset:{tbOffset.Text}]\n" + tbLrc.Text;
            import_Click(null, null);
            SaveLrc();
        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {
            SaveLrc();
        }

        private void tbLrc_Drop(object sender, DragEventArgs e)
        {
            var file = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            var lrc = Methods.TxtToLrc(file);
            tbLrc.Text = LyricInfo.LrcFormat(lrc);
        }

        private void tbLrc_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void openTemp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(lrcTemp);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            fsw.EnableRaisingEvents = false;
        }

        private void checkedChanged(object sender, RoutedEventArgs e)
        {
            fsw.EnableRaisingEvents = checkBox.IsChecked ?? false;
        }

        private void regexDel_Click(object sender, RoutedEventArgs e)
        {
            tbLrc.Text = Regex.Replace(tbLrc.Text, tbPatt.Text, string.Empty);
        }

        private static LyricWin win;
        public static void ShowWindow(MusicFile mf)
        {
            if (mf == null)
                return;
            if (win == null)
                win = new LyricWin(mf);
            else
                win.loadMF(mf);
            win.Show();
            win.checkedChanged(null, null);
        }

        private void openLrc_Clcik(object sender, RoutedEventArgs e)
        {
            var name = mf.FullName.Substring(0, mf.FullName.LastIndexOf('.')) + ".lrc";
            if (File.Exists(name))
                tbLrc.Text = LyricInfo.LrcFormat(File.ReadAllText(name, Encoding.UTF8));
        }
    }
}
