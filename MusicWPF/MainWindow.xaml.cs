using MusicWPF.Models;
using SatrokiLibrary.Extend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using static MusicWPF.Properties.Settings;

namespace MusicWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            EventsInit();
            DataContext = this;
        }

        #region 字段
        private IMediaPlay mediaPlayer = new WindowsMediaPlayer();
        private TimeSpan zero = new TimeSpan();
        private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };
        private ObservableCollection<ListFiles> fileList;
        private ListFiles currentLF;
        private string lrcContent;
        private bool paused = true;
        private bool listLoaded = false;
        private int index = 0;
        #endregion

        #region 属性
        public List<PlayList> PlayLists { get; set; }
        public PlayList CurrentList { get; set; }
        public TimeSpan Position
        {
            get { return mediaPlayer.Position; }
            set { mediaPlayer.Position = value; OnPropertyChanged(nameof(Position)); }
        }
        public double Volume
        {
            get { return mediaPlayer.Volume; }
            set { mediaPlayer.Volume = value; OnPropertyChanged(nameof(Volume)); }
        }
        public ObservableCollection<ListFiles> FileList
        {
            get { return fileList; }
            set { fileList = value; OnPropertyChanged(nameof(FileList)); }
        }
        public int Index
        {
            get { return index; }
            set
            {
                index = value;
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(NextName));
            }
        }
        public MusicFile Current => CurrentLF?.MusicFile;
        public ListFiles CurrentLF
        {
            get { return currentLF; }
            set
            {
                if (currentLF != value)
                { currentLF = value; MusicChanged(); }
            }
        }
        public string LrcContent
        {
            get { return lrcContent; }
            set
            {
                if (lrcContent != value)
                { lrcContent = value; OnPropertyChanged(nameof(LrcContent)); }
            }
        }
        public Tuple<int, double> LastFile { get; set; }
        public bool ListLoaded
        {
            get { return listLoaded; }
            set { listLoaded = value; OnPropertyChanged(nameof(ListLoaded)); }
        }
        public ListFiles SelectedFile { get; set; }
        public string NextName => FileList[(Index + 1) % FileList.Count].MusicFile.Title;
        public string LastName => FileList[(FileList.Count + Index - 1) % FileList.Count].MusicFile.Title;
        #endregion

        #region 初始化
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetState();

            PlayLists = await Task.Run(() => MDModel.Model.PlayList.Where(pl => pl.Enabled).ToList());

            int index = ContextMenu.Items.IndexOf(spIndex) + 1;
            foreach (var item in PlayLists)
            {
                var mi = new MenuItem() { Header = item.Tag ?? item.Name, Tag = item };
                mi.Click += GetList_Click;
                ContextMenu.Items.Insert(index, mi);
                index++;
            }

            var cl = PlayLists.SingleOrDefault(pl => pl.ListId == Default.LastListId);
            if (cl != null)
            {
                await SetList(cl, false);
                FileList = Methods.LoadListOrder(FileList);
            }
            ListLoaded = true;
        }
        private async void GetList_Click(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            var pl = (PlayList)mi.Tag;
            await SetList(pl);
        }
        private async Task SetList(PlayList pl, bool random = true)
        {
            CurrentList = pl;
            FileList = await Task.Run(() =>
            new ObservableCollection<ListFiles>(pl.ListFiles.Where(lf => lf.IsChecked)));
            if (random)
                miRandom_Click(null, null);
        }
        private void EventsInit()
        {
            InitializeComponent();

            timer.Tick += (s, e) => PositionChanged();

            mediaPlayer.MediaOpened = () =>
             {
                 if (Current.TotalSeconds == null)
                 {
                     Current.Duration = mediaPlayer.Duration;
                     MDModel.Model.SaveChanges();
                 }
                 OnPropertyChanged(nameof(Current));
             };

            mediaPlayer.MediaEnded = () =>
              {
                  timer.Stop();
                  if (tbnLoop.IsChecked == true)
                  {
                      mediaPlayer.Position = zero;
                      mediaPlayer.Play();
                      timer.Start();
                  }
                  else
                      Play(1);
              };
        }
        #endregion

        #region 播放控制
        private void Play_Click(object sender, RoutedEventArgs e) => Play();
        private void Pause_Click(object sender, RoutedEventArgs e) => Pause();
        private void Stop_Click(object sender, RoutedEventArgs e) => Stop();
        private void Last_Click(object sender, RoutedEventArgs e) => Play(-1);
        private void Next_Click(object sender, RoutedEventArgs e) => Play(1);

        private void PrePlay(int delta)
        {
            if (FileList == null || FileList.Count == 0)
                return;
            if (LastFile != null)
                Index = FileList.FindIndex(mf => mf.FileId == LastFile.Item1);
            if (Index < 0)
                Index = 0;
            Index += delta;
            Index = Index % FileList.Count;
            CurrentLF = FileList[Index];
            mediaPlayer.Open(Current.FullName);
            mediaPlayer.Position = TimeSpan.FromSeconds(LastFile?.Item2 ?? 0);
        }
        private void Play(int delta = 0)
        {
            PrePlay(delta);
            mediaPlayer.Play();
            paused = false;
            timer.Start();
            LastFile = null;
        }
        private void Pause()
        {
            if (paused)
            {
                if (mediaPlayer.Source != null)
                { mediaPlayer.Play(); timer.Start(); }
                else Play();
                paused = false;
            }
            else
            {
                mediaPlayer.Pause();
                timer.Stop();
                paused = true;
            }
        }
        private void Stop()
        {
            mediaPlayer.Stop();
            timer.Stop();
            PositionChanged();
        }
        private void MusicChanged()
        {
            OnPropertyChanged(nameof(CurrentLF));
            OnPropertyChanged(nameof(Current));
            lbList.SelectedItem = CurrentLF;
            lbList.ScrollIntoView(CurrentLF);
        }
        private void PositionChanged()
        {
            OnPropertyChanged(nameof(Position));
            LrcContent = Current.LyricInfo?.Seek(Position);
        }
        private void miRandom_Click(object sender, RoutedEventArgs e)
        {
            if (FileList != null)
            {
                var temp = tbnRandom.IsChecked == true
                    ? FileList.OrderByRandom()
                    : FileList.OrderBy(fl => fl.Id);
                FileList = new ObservableCollection<ListFiles>(temp);
            }
        }
        #endregion

        #region 右键菜单
        private void miLrc_Click(object sender, RoutedEventArgs e) => LyricWin.ShowWindow(Current);
        private void miClose_Click(object sender, RoutedEventArgs e) => Close();
        private void miManage_Click(object sender, RoutedEventArgs e) => new ManageCenter().Show();
        private void miCopyLrc_Click(object sender, RoutedEventArgs e) => Clipboard.SetText((string)lLrc.Content);
        private void miScroll_Click(object sender, RoutedEventArgs e) => lbList.ScrollIntoView(CurrentLF);
        private void miTag_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            TagWin.ShowTagWin(this, CurrentLF);
        }
        #endregion

        #region 列表事件
        private void lbList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedFile == null)
                return;
            Index = FileList.IndexOf(SelectedFile);
            LastFile = null;
            Play();
        }
        private void tbFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var key = tbFilter.Text;
            if (!string.IsNullOrWhiteSpace(key))
                lbList.Items.Filter = (o) =>
                {
                    var mf = ((ListFiles)o).MusicFile;
                    return mf.FullName.Contains(key) || mf.Artist.Contains(key) || mf.Album.Contains(key);
                };
            else
                lbList.Items.Filter = null;
        }
        private void lbList_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(tbFilter);
            if (p.Y > 0 && p.Y < 20)
                tbFilter.Visibility = Visibility.Visible;
            else
                tbFilter.Visibility = Visibility.Hidden;
        }
        private void tbnList_Click(object sender, RoutedEventArgs e)
        {
            var maxHeight = SystemParameters.WorkArea.Height;
            if (gdList.Visibility == Visibility.Visible)
            {
                var h = gdList.ActualHeight;
                Top = Math.Min(maxHeight - 65, Top + h);
                gdList.Visibility = Visibility.Collapsed;
            }
            else
            {
                gdList.Visibility = Visibility.Visible;
                var h = gdList.ActualHeight;
                Top = Math.Max(0, Top - h);
            }
        }
        #endregion

        #region 列表-右键菜单
        private void miDelFromList_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null)
            {
                SelectedFile.IsChecked = false;
                MDModel.Model.SaveChanges();
                FileList.Remove(SelectedFile);
            }
        }
        private void miCopyName_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(Current?.DisplayName);
        private void miOpenDir_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null)
            {
                var di = new System.IO.FileInfo(SelectedFile.MusicFile.FullName).DirectoryName;
                Process.Start(di);
            }
        }
        private void miEditSelectedLrc_Click(object sender, RoutedEventArgs e) => LyricWin.ShowWindow(SelectedFile?.MusicFile);
        private void miCopySelectedTitle_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(SelectedFile?.MusicFile.DisplayName);
        private void miSelectedTag_Click(object sender, RoutedEventArgs e) => TagWin.ShowTagWin(this, SelectedFile);
        #endregion

        #region 其他
        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region 热键
        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1: miManage_Click(null, null); break;
                case Key.F2: miCopyName_Click(null, null); break;
                case Key.F3: miScroll_Click(null, null); break;
                case Key.F4: miLrc_Click(null, null); break;
                case Key.F5: miTag_Click(null, null); break;
                case Key.Escape: miClose_Click(null, null); break;
            }
        }
        #endregion

        #region 配置
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveState();
            FileList.SaveListOrder();
            Application.Current.Shutdown();
        }
        public void SetState()
        {
            var w = SystemParameters.WorkArea.Width;
            var h = SystemParameters.WorkArea.Height;
            Top = Math.Min(h - 65, Default.WinTop);
            Left = Math.Min(w - 400, Default.WinLeft);

            tbnLoop.IsChecked = Default.LoopChecked;
            tbnRandom.IsChecked = Default.RandomChecked;
            LastFile = new Tuple<int, double>(Default.FileId, Default.Position);
            gdList.Visibility = (Visibility)Default.ListVisibility;
            Volume = Default.Volume;
        }
        public void SaveState()
        {
            Default.FileId = Current?.FileId ?? 0;
            Default.ListVisibility = (int)gdList.Visibility;
            Default.LoopChecked = (bool)tbnLoop.IsChecked;
            Default.RandomChecked = (bool)tbnRandom.IsChecked;
            Default.Position = pbPosition.Value;
            Default.Volume = Volume;
            Default.WinTop = Top;
            Default.WinLeft = Left;
            Default.LastListId = CurrentList.ListId;
            Default.Save();
        }
        #endregion


    }

    public class TimeSpanToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}
