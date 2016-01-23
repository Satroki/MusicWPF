using MusicWPF.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SatrokiLibrary.Extend;

namespace MusicWPF
{
    /// <summary>
    /// ManageCenter.xaml 的交互逻辑
    /// </summary>
    public partial class ManageCenter : Window
    {
        public ManageCenter()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lbPlayList.ItemsSource = await Task.Run(() => MDModel.Model.PlayList.ToList());
            var dirs = await Task.Run(() => MDModel.Model.DirectoryList.OrderBy(dl => dl.FullName).ToList());
            cbClass.ItemsSource = dirs.Select(d => d.ClassName).Distinct();
            cbClass.SelectedItem = "动画";
            cbDirs.DisplayMemberPath = nameof(DirectoryList.FullName);
            cbDirs.SelectedValuePath = nameof(DirectoryList.FullName);
            cbDirs.ItemsSource = dirs;
        }

        private void bSavePlayList_Click(object sender, RoutedEventArgs e)
        {
            MDModel.Model.SaveChanges();
        }

        private async void bReloadPlayList_Click(object sender, RoutedEventArgs e)
        {
            var pl = lbPlayList.SelectedItem as PlayList;
            if (pl == null)
                return;
            var count = await Methods.ReloadPlayList(pl);
            MessageBox.Show($"共导入 {count} 个文件");
        }


        private DirectoryInfo directoryInfo;
        private void cbClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbClass.SelectedItem == null)
                cbDirs.Items.Filter = null;
            cbDirs.Items.Filter = (o) =>
                ((DirectoryList)o).ClassName == (string)cbClass.SelectedItem;
        }

        private void tbLog_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private async void tbLog_Drop(object sender, DragEventArgs e)
        {
            var str = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            directoryInfo = new DirectoryInfo(str);
            await ShowOrExcCmd();
        }

        private async Task ShowOrExcCmd(bool exc = false)
        {
            tbLog.Text = string.Empty;
            if (directoryInfo == null)
                return;
            var dest = cbDirs.Text;
            var className = (string)cbClass.SelectedItem;
            var newPath = Path.Combine(dest, directoryInfo.Name);

            AppendLog($"目标：{directoryInfo.FullName}\n");

            if (!Directory.Exists(dest))
            {
                AppendLog($"创建文件夹：{dest}\n");
                if (exc)
                {
                    Directory.CreateDirectory(dest);
                    AppendLog("——完成。\n");
                }

                AppendLog($"添加路径到数据库：{dest}\n");
                if (exc)
                {
                    var count = Methods.AddDirectory(new DirectoryInfo(dest), className);
                    AppendLog($"——完成：{count} 条记录\n");
                }
            }

            tbLog.Text += $"移动文件夹：{directoryInfo.FullName}\n    到：{newPath}\n";
            if (exc)
            {
                await Task.Run(() =>
                {
                    if (directoryInfo.FullName.ToUpper()[0] == dest.ToUpper()[0])
                        directoryInfo.MoveTo(newPath);
                    else
                    {
                        directoryInfo.CopyTo(dest);
                        directoryInfo.Delete(true);
                    }
                });
                AppendLog("——完成。\n");
            }

            tbLog.Text += "添加音乐文件\n";
            if (exc)
            {
                var count = await Methods.AddMusicFile(newPath);
                AppendLog($"——完成：{count} 个文件。\n");
            }
        }

        private void AppendLog(string line) => tbLog.Text += line;

        private async void ShowCmd_Click(object sender, RoutedEventArgs e)
        {
            await ShowOrExcCmd();
        }

        private async void ExcCmd_Click(object sender, RoutedEventArgs e)
        {
            await ShowOrExcCmd(true);
        }

        private void ExportLrc_Click(object sender, RoutedEventArgs e)
        {
            tbLog.Text = string.Empty;
            var mfs = MDModel.Model.MusicFile.Where(mf => mf.Lyric != null).ToList();
            var count = 0;
            foreach (var mf in mfs)
            {
                if (Methods.ExportLrc(mf))
                    count++;
            }
            AppendLog($"导出完成：{count} / {mfs.Count}");
        }
    }
}
