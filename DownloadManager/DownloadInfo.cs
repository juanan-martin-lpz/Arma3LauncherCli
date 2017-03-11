using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager
{
    public enum DownloadStatus
    {
        Initialized = 0,
        Running,
        Stopped,
        Cancelled,
        Paused,
        WithErrors
    }
    public class DownloadInfo
    {
        public string URL { get; set; }
        public string Filename { get; set; }
        public int Size { get; set; }
        public int Progress { get; set; }
        public int BytesDownloaded {get; set;}
        public DownloadStatus Status { get; set; }
    }
}
