using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyDownloader.Core;
using MyDownloader.Extension.Protocols;


namespace ServidoresData
{
    public class MyParms : IHttpFtpProtocolParameters
    {
        private string _ProxyAddress, _ProxyUserName, _ProxyPassword, _ProxyDomain;
        private bool _UseProxy, _ProxyByPassOnLocal;
        private int _ProxyPort;

        #region IHttpFtpProtocolParameters Members

        public string ProxyAddress
        {
            get { return _ProxyAddress; }
            set { _ProxyAddress = value; }
        }

        public string ProxyUserName
        {
            get { return _ProxyUserName; }
            set { _ProxyUserName = value; }
        }

        public string ProxyPassword
        {
            get { return _ProxyPassword; }
            set { _ProxyPassword = value; }
        }

        public string ProxyDomain
        {
            get { return _ProxyDomain; }
            set { _ProxyDomain = value; }
        }

        public bool UseProxy
        {
            get { return _UseProxy; }
            set { _UseProxy = value; }
        }

        public bool ProxyByPassOnLocal
        {
            get { return _ProxyByPassOnLocal; }
            set { _ProxyByPassOnLocal = value; }
        }

        public int ProxyPort
        {
            get { return _ProxyPort; }
            set { _ProxyPort = value; }
        }

        #endregion
    }
}
