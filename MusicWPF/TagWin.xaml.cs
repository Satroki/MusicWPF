using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MusicWPF.Models;
using TagLib;

namespace MusicWPF
{
    /// <summary>
    /// TagWin.xaml 的交互逻辑
    /// </summary>
    public partial class TagWin : Window
    {
        private MusicFile mf;

        public TagWin(ListFiles lf)
        {
            InitializeComponent();
            mf = lf.MusicFile;
            DataContext = mf;
        }

        public static void ShowTagWin(Window win, ListFiles lf)
        {
            if (lf == null)
                return;
            var tw = new TagWin(lf) { Owner = win };
            tw.Show();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            var file = File.Create(mf.FullName);
            var t = file.Tag;
            t.Title = mf.Title;
            t.Album = mf.Album;
            t.Performers = mf.Artist.Split(',');
            file.Save();
            file.Dispose();
            MDModel.Model.SaveChanges();
            Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
