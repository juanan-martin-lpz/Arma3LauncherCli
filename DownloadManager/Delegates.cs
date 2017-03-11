using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DownloadManager
{
    public class Delegates
    {
        public delegate void DownloadProgressChange(object sender, ProgressChangedEventArgs e);
        public delegate void DownloadCompleted(object sender, AsyncCompletedEventArgs e);
        public delegate void DownloadStarted(object sender, AsyncCompletedEventArgs e);
        public delegate void DownloadPaused(object sender, AsyncCompletedEventArgs e);
        public delegate void DownloadResumed(object sender, AsyncCompletedEventArgs e);
        public delegate void DownloadCancelled(object sender, AsyncCompletedEventArgs e);
        public delegate void DownloadError(object sender, AsyncCompletedEventArgs e);

    }
}
