using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using NAudio;
using NAudio.Wave;

namespace MusicWPF
{
    public interface IMediaPlay
    {
        TimeSpan Position { get; set; }
        double Volume { get; set; }
        string Source { get; set; }
        TimeSpan Duration { get; }
        void Open(string fileName);
        void Play();
        void Pause();
        void Stop();
        Action MediaOpened { set; }
        Action MediaEnded { set; }
    }
    public class WindowsMediaPlayer : IMediaPlay
    {
        private MediaPlayer mediaplay = new MediaPlayer();
        public Action MediaEnded
        { set { mediaplay.MediaEnded += (s, e) => value(); } }

        public Action MediaOpened
        { set { mediaplay.MediaOpened += (s, e) => value(); } }

        public TimeSpan Position
        {
            get
            { return mediaplay.Position; }
            set
            { mediaplay.Position = value; }
        }

        public string Source { get; set; }

        public double Volume
        {
            get
            { return mediaplay.Volume; }
            set
            { mediaplay.Volume = value; }
        }

        public void Open(string fileName)
        {
            mediaplay.Open(new Uri(fileName));
            Source = fileName;
        }

        public TimeSpan Duration => mediaplay.NaturalDuration.TimeSpan;

        public void Pause() => mediaplay.Pause();
        public void Play() => mediaplay.Play();
        public void Stop() => mediaplay.Stop();
    }

    public class NAudioMediaPlayer : IMediaPlay
    {
        private IWavePlayer wavePlayer;
        private WaveStream ws;
        private WaveChannel32 wc;
        public TimeSpan Duration => wc.TotalTime;

        private Action ended;
        public Action MediaEnded
        { set { ended = value; } }

        private Action opened;
        public Action MediaOpened
        { set { opened = value; } }

        private TimeSpan position;
        public TimeSpan Position
        {
            get { return wc?.CurrentTime ?? position; }
            set
            {
                position = value;
                if (wc != null)
                    wc.CurrentTime = position;
            }
        }

        public string Source { get; set; }

        private float volume = 0.2f;
        public double Volume
        {
            get { return volume; }
            set
            {
                volume = (float)value;
                if (wc != null)
                    wc.Volume = volume;
            }
        }

        public void Open(string fileName)
        {
            if (wavePlayer != null)
                wavePlayer.Dispose();
            wavePlayer = null;
            wavePlayer = new WaveOutEvent();
            if (wc != null)
            { wc.Close(); wc.Dispose(); wc = null; }
            if (ws != null)
            { ws.Close(); ws.Dispose(); ws = null; }

            ws = new MediaFoundationReader(fileName);
            wc = new WaveChannel32(ws);
            Source = fileName;
            wavePlayer.Init(wc);
            wc.Volume = (float)Volume;
            wc.CurrentTime = position;
            wavePlayer.PlaybackStopped += (s, e) => ended();
            opened();
        }

        public void Pause() => wavePlayer.Pause();
        public void Play() => wavePlayer.Play();
        public void Stop() => wavePlayer.Stop();
    }
}
