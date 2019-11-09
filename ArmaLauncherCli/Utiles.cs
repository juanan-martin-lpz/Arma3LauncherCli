using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Windows;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Data.HashFunction;

namespace ArmaLauncherCli
{
    static class Utiles
    {

        public static string ArmaStdErr {get; set;}
        //private static FtpClient ftpCli = null;


        /*
        static private void connectFTP(string servidor, string userName, string password)
        {
            if (ftpCli == null) 
            {
                ftpCli = new FtpClient(FtpProtocol.Ftp, servidor, 21, new NetworkCredential(userName, password));
            }
        }

        static public bool descarga_ficheroFTP(string servidor, string fichero_remoto, string fichero_local)
        {
            try
            {
                

                ArxOne.Ftp.IO.FtpStream ftpfs;

                FileStream fs = new FileStream(fichero_local, FileMode.Create);

                FtpPath path = new FtpPath(servidor + fichero_remoto);

                ftpfs = (ArxOne.Ftp.IO.FtpStream)ftpCli.Retr(path,FtpTransferMode.Binary);

                
                ftpfs.CopyTo(fs);

                ftpfs.Flush();
                fs.Flush();

                ftpfs.Close();
                fs.Close();

                return true;
            }
            catch
            {
                return false;
            }

        }

        static public bool carga_ficheroFTP(string servidor, string fichero_local,string user, string pwd)
        {


            ArxOne.Ftp.IO.FtpStream ftpfs;

            connectFTP(servidor, user, pwd);

            FileStream fs = new FileStream(fichero_local, FileMode.Open);

            FtpPath path = new FtpPath(@"Reports/" + Path.GetFileName(fichero_local));

            ftpfs = (ArxOne.Ftp.IO.FtpStream)ftpCli.Stor(path, FtpTransferMode.Binary);


            fs.CopyTo(ftpfs);

            ftpfs.Flush();
            fs.Flush();

            ftpfs.Close();
            fs.Close();

            return true;
        }
        */
         
        //Stackoverflow
        static private FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, string userName, string password, bool keepAlive = false)
        {
            Uri target = new Uri(Uri.EscapeUriString(ftpDirectoryPath));
            

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
            
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
            request.Proxy = null;

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = keepAlive;
            
            request.Credentials = new NetworkCredential(userName, password);
            request.EnableSsl = false;
            //request.Timeout = 60;
            

            return request;
        }

        /*
        // Descarga un fichero por FTP. true = descarga correcta false = descarga incorrecta.
        static public bool descarga_fichero(string servidor, string usuario, string contraseña, string fichero_remoto, string fichero_local)
        {

            string s = "188.165.254.137";

            connectFTP(s,usuario,contraseña);

            return descarga_ficheroFTP(servidor, fichero_remoto, fichero_local);

            /*
            try
            {

                string f = servidor + fichero_remoto.Replace('\\', '/'); //.TrimStart('/');

                //if (f.Contains("#")) return true;


                FtpWebRequest request = CreateFtpWebRequest(f, usuario, contraseña, true);
                


                //FtpWebRequest request = (FtpWebRequest)WebRequest.Create(f);

                //request.Method = WebRequestMethods.Ftp.DownloadFile; // este me´todo descarga un fichero, contenido en la url
                //request.Credentials = new NetworkCredential(usuario, contraseña);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using(Stream responseStream = response.GetResponseStream())
                using (BinaryReader reader = new BinaryReader(responseStream))
                using (FileStream fs = new FileStream(fichero_local, FileMode.Create))
                {
                    responseStream.CopyTo(fs);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
             
        }

        */

        /*
        internal static string calcula_firma_MD5(string fichero)
        {
            try
            {
                string firma_base64 = "";

                xxHash firma = null;

                if (firma == null) { firma = new xxHash(); }

                //using (MD5 firma = MD5.Create())
                using (FileStream streamFichero = File.OpenRead(fichero))
                {
                    byte[] f = firma.ComputeHash(streamFichero);
                    firma_base64 = Convert.ToBase64String(f);
                }

                return firma_base64;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                throw x;
            }
        }

        */

        static public void ejecuta_proceso(string orden, string parametros, string dir_trabajo, bool shex = false)
        {

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(orden);
            info.UseShellExecute = shex;
            
            // Tratamos de capturar errores a ver que pasa
            
            if (!shex) info.RedirectStandardError = true;
            


            info.Arguments = parametros;
            info.WorkingDirectory = dir_trabajo;

            // black
            //
            p.EnableRaisingEvents = true;
            p.Exited += new EventHandler(Utiles.arma_Exited);
            //
            p.StartInfo = info;
            //while (!p.Start()) ; // mio

            p.Start();

            /*
            if (!shex)
            {
                // Capturamos la salida estandar de errores. Poca chicha por aqui pero nunca se sabe
                string stderr;

                while ((stderr = p.StandardError.ReadLine()) != null)
                {
                    ArmaStdErr += stderr + Environment.NewLine;

                    ArmaStdErrChanged(null, EventArgs.Empty);
                }
            }
            */
            p.WaitForExit(); 
            
        }
        
        private static void arma_Exited(object sender, System.EventArgs e)
        {
            // Aqui puedes chequear p.ExitCode             
        }

        static public void ejecuta_proceso_sin_ventana_con_espera(string orden, string parametros, string dir_trabajo)
        {

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(orden);
            info.UseShellExecute = true;
            //info.CreateNoWindow = false;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            info.Arguments = parametros;
            info.WorkingDirectory = dir_trabajo;

            p.StartInfo = info;
            while (!p.Start()) ;
            while (!p.HasExited) ;
        }

        // Abre un fichero
        static public void AbrirFichero(string fichero)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(fichero);
            info.UseShellExecute = true;
            p.StartInfo = info;
            p.Start();
            // while(!p.Start());
        }


        // Comprime-decomprime ficheros

        // NO PROBADO

        static public void comprimeFichero(string fichero)
        {
            try
            {
                // Para evitar colisiones de nombres, añadimos la extension al nombre completo (p. ej leeme.txt -> leeme.tzt.zip , leeme.doc -> leeme.doc.zip

                string fichero_destino = fichero + ".zip";

                // Borramos el fichero de destino. Si no, unzip se queda colgado preguntando si lo descomprime.               

                if (File.Exists(fichero_destino))
                    File.Delete(fichero_destino);

                // Descomprimimos-
                string orden = "\"zip.exe\"";
                string parametros = "\"" + fichero_destino + "\" \"" + fichero + "\"";
                ejecuta_proceso_sin_ventana_con_espera(orden, parametros, "");
            }
            catch (Exception)
            {
                throw;
            }
        }

        static public void descomprimeFichero(string fichero)
        {
            try
            {
                // Borramos el fichero de destino. Si no, unzip se queda colgado preguntando si lo descomprime.               

                //                if (File.Exists(fichero.Replace(".zip", ".*")))
                //                    File.Delete(fichero.Replace(".zip", ".*"));

                // Descomprimimos-
                string orden = "\"unzip.exe\"";
                string parametros = "\"" + fichero + "\"";
                ejecuta_proceso(orden, parametros, fichero);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
