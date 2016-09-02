using System.ComponentModel;
using System.Net;

namespace ServidoresData
{
    public interface IWebDownload
    {
        string Filename { get; set; }
        long Progress { get; set; }
        string Repository { get; }
        string Server { get; }
        string Target { get; set; }

        event AsyncCompletedEventHandler DownloadFileCompleted;
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event PropertyChangedEventHandler PropertyChanged;
        event ProgressChangedEventHandler WebDownloadProgressChanged;

        void Download();
        void DownloadAsync();
        void DownloadFile(string urlAddress, string repodb, string location);
        void DownloadFileAsync(string urlAddress, string repodb, string location);
    }
}