using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace ServidoresData
{
    public static class HttpClientExtensions
    {
        public static async Task<byte[]> GetByteArrayAsyncWithProgress(this HttpClient client, string requestUri, Action<decimal, decimal, decimal> onProgress, Action onFinish)
        {
            // Quizas Task<Tuple<int,byte[]>> y retornamos bytes leidos y el buffer

            List<byte> result = new List<byte>();
            byte[] buffer;
            int bufferLength = 1024 * 1024; // 1MB
            var bytesRead = 0L;
            var megaBytesTotal = 0M;

            string statusCode;

            Func<long, long, decimal> percent = (read, total) =>
            {
                if (total == 0L)
                    return 0M;

                return (decimal)read / (decimal)total;
            };
            Func<long, decimal> megaBytes = (byteLength) =>
            {
                var mb = byteLength / 1024M / 1024M;
                return mb;
            };

            using (var response = await client.GetAsync(requestUri))
            {
                // Suponemos que uno de estos errores es retornado si la Uri no se encuentra, aparte del contenido del not found
                if ((response.StatusCode == System.Net.HttpStatusCode.Forbidden) || (response.StatusCode == System.Net.HttpStatusCode.NotFound))
                {
                    // Retornamos array con longitud cero o Tuple<int,byte[]>(0,null)
                }


                // El recurso existe, lo leemos
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    long bytesTotal = 0L;
                    try
                    {
                        // Longitud del recurso
                       if (stream.CanSeek)
                            bytesTotal = stream.Length;
                        else
                        {
                            IEnumerable<string> contentLengthValues;
                            if (response.Headers.TryGetValues("Content-Length", out contentLengthValues))
                            {
                                if (!long.TryParse(contentLengthValues.First(), out bytesTotal))
                                    bytesTotal = 0L;
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
                            onProgress(megaBytes(bytesRead), megaBytesTotal, percent(bytesRead, bytesTotal));
                        }
                        else
                        {
                            break;
                        }
                            
                    }

                    // Aqui ya hemos terminado
                    onFinish();
                }
            }

            var responseBytes = result.ToArray();
            result = null;
            buffer = null;
            percent = null;

            return responseBytes;
        }
    }
}
