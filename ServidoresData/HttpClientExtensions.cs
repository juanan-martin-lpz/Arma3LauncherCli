using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.ComponentModel;

namespace ServidoresData
{
    public static class HttpClientExtensions
    {

        public delegate void DownloadProgress(object sender, DownloadAsyncProgressChangedEventArgs e);
        public delegate void DownloadFinished(object sender, AsyncCompletedEventArgs e);

        public static event DownloadProgress OnDownloadProgress;
        public static event DownloadFinished OnDownloadFinished;

        // Action<int, int, int> onProgress, Action onFinish
        public static async Task<Tuple<long, byte[]>> GetByteArrayAsyncWithProgress(this HttpClient client, string requestUri, object sender)
        {

            List<byte> result = new List<byte>();
            byte[] buffer;
            int bufferLength = 1024 * 1024; // 1MB
            var bytesRead = 0;
            var megaBytesTotal = 0;

            string statusCode;

            Func<int, int, int> percent = (read, total) =>
            {
                if (total == 0)
                    return 0;

                return (read * 100) / total;
            };
            Func<int, int> megaBytes = (byteLength) =>
            {
                var mb = byteLength / 1024 / 1024;
                return mb;
            };

            using (var response = await client.GetAsync(requestUri))
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
                            bytesTotal = (int) stream.Length;
                        else
                        {
                            IEnumerable<string> contentLengthValues;
                            if (response.Headers.TryGetValues("Content-Length", out contentLengthValues))
                            {
                                if (!int.TryParse(contentLengthValues.First(), out bytesTotal))
                                    bytesTotal = 0;
                            }
                        }

                        megaBytesTotal = megaBytes(bytesTotal);
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
                                DownloadAsyncProgressChangedEventArgs e = new DownloadAsyncProgressChangedEventArgs(percent(bytesRead, bytesTotal),sender);
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
            percent = null;

            return new Tuple<long, byte[]>(bytesRead,responseBytes);
        }
    }
}
