using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicWPF
{
    public class CProgressBar : ProgressBar
    {
        public CProgressBar()
        {
            Cursor = Cursors.Hand;
            MouseLeftButtonDown += CProgressBar_MouseLeftButtonDown;
            MouseWheel += CProgressBar_MouseWheel;
        }

        private void CProgressBar_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Value += Maximum * 0.02;
            else
                Value -= Maximum * 0.02;
        }

        private void CProgressBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(this);
            Value = Maximum * (p.X / ActualWidth);
        }
    }
}
