using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.ComponentModel;
using System.Net;

namespace ServidoresData
{
    public class HttpDownload
    {
        public delegate void DownloadProgress(object sender, DownloadAsyncProgressChangedEventArgs e);
        public delegate void DownloadFinished(object sender, AsyncCompletedEventArgs e);

        public event DownloadProgress OnDownloadProgress;
        public event DownloadFinished OnDownloadFinished;

        private HttpClient client;

        List<byte> result = new List<byte>();
        
        int bufferLength = 256 * 1024 * 1024; // 1MB

        long bytesRead = 0;
#pragma warning disable CS0414 // El campo 'HttpDownload.megaBytesTotal' está asignado pero su valor nunca se usa
        long megaBytesTotal = 0;
#pragma warning restore CS0414 // El campo 'HttpDownload.megaBytesTotal' está asignado pero su valor nunca se usa
        long percent;
        long bytesTotal = 0;

        long totalreaded;

#pragma warning disable CS0169 // El campo 'HttpDownload.statusCode' nunca se usa
        string statusCode;
#pragma warning restore CS0169 // El campo 'HttpDownload.statusCode' nunca se usa

        const int BUFFER_SIZE = 1024 * 1024;
        byte[] buffer;

        WebRequest req;
        WebResponse response;
        System.IO.Stream responseStream;

        string reqUri;

        public HttpDownload(string requestUri)
        {
            client = new HttpClient();

            buffer = new byte[BUFFER_SIZE];

            reqUri = requestUri;

            buffer = new byte[BUFFER_SIZE];

            try
            {
                req = WebRequest.Create(reqUri);
                response = req.GetResponse();
                responseStream = response.GetResponseStream();

                bytesTotal = response.ContentLength;


            }
            catch (System.Net.WebException ex)
            {
                throw ex;
            }
        }


        public async Task<Tuple<long, byte[]>> GetByteArrayAsync()
        {            

            while (responseStream.CanRead)
                {
                    //buffer = null;
                    //buffer = new byte[BUFFER_SIZE];

                    bytesRead = await responseStream.ReadAsync(buffer, 0, BUFFER_SIZE);

                    totalreaded += bytesRead;

                    if (OnDownloadProgress != null)
                    {
                        if (bytesRead > 0)
                        {
                            percent = (totalreaded * 100) / bytesTotal;
                        }
                        else
                        {
                            percent = 0;
                        }

                        Console.WriteLine("Leidos {0} bytes de {1}", totalreaded, bytesTotal);

                        DownloadAsyncProgressChangedEventArgs e = new DownloadAsyncProgressChangedEventArgs((int) percent, this);

                        OnDownloadProgress(this, e);
                            
                    }

                    if ((totalreaded == bytesTotal) && (totalreaded > 0))
                    {
                        if (OnDownloadFinished != null)
                        {
                            AsyncCompletedEventArgs e = new AsyncCompletedEventArgs(null, false, this);

                            OnDownloadFinished(this, e);
                        }
                    }

                    return new Tuple<long, byte[]>(bytesRead, buffer);

                }       //(bytesRead > 0);

            return null;
        }

        // Action<int, int, int> onProgress, Action onFinish
        public async Task<Tuple<long, byte[]>> GetByteArrayAsyncWithProgress(string requestUri, object sender)
        {

            System.Console.WriteLine("Maximo buffer de respuesta : {0}", client.MaxResponseContentBufferSize);

            using (var response = await client.GetAsync(requestUri,HttpCompletionOption.ResponseHeadersRead))
            {
                // Suponemos que uno de estos errores es retornado si la Uri no se encuentra, aparte del contenido del not found
                if ((response.StatusCode == System.Net.HttpStatusCode.Forbidden) || (response.StatusCode == System.Net.HttpStatusCode.NotFound))
                {
                    // Retornamos array con longitud cero o Tuple<int,byte[]>(0,null)
                    return new Tuple<long, byte[]>(0L, null);
                }


                // El recurso existe, lo leemos
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    int bytesTotal = 0;
                    try
                    {
                        // Longitud del recurso
                        if (stream.CanSeek)
                            bytesTotal = (int)stream.Length;
                        else
                        {
                            IEnumerable<string> contentLengthValues;
                            if (response.Headers.TryGetValues("Content-Length", out contentLengthValues))
                            {
                                if (!int.TryParse(contentLengthValues.First(), out bytesTotal))
                                    bytesTotal = 0;
                            }
                        }
                        //megaBytesTotal = megaBytes(bytesTotal);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    while (stream.CanRead)
                    {
                        buffer = new byte[bufferLength];
                        // web streams aren't seekable, so the offset is always 0
                        var read = await stream.ReadAsync(buffer, 0, bufferLength);

                        if (read > 0)
                        {
                            if (read == bufferLength)
                                result.AddRange(buffer);
                            else
                                result.AddRange(buffer.Take(read));

                            bytesRead += read;

                            if (OnDownloadProgress != null)
                            {
                                if ((bytesRead > 0) && (bytesTotal > 0))
                                {
                                    percent = (int)(bytesRead * 100) / bytesTotal;
                                }
                                else
                                {
                                    percent = 0;
                                }

                                DownloadAsyncProgressChangedEventArgs e = new DownloadAsyncProgressChangedEventArgs((int)percent, sender);
                                OnDownloadProgress(sender, e);

                            }
                            //onProgress(bytesRead, bytesTotal, percent(bytesRead, bytesTotal));
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Aqui ya hemos terminado
                    if (OnDownloadFinished != null)
                    {
                        AsyncCompletedEventArgs e = new AsyncCompletedEventArgs(null, false, sender);

                        OnDownloadFinished(sender, e);
                    }

                    //onFinish();
                }
            }

            var responseBytes = result.ToArray();
            result = null;
            buffer = null;
            //percent = null;

            return new Tuple<long, byte[]>(bytesRead, responseBytes);
        }

    }
}
