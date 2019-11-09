using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Arma3LauncherCli.Properties;
using System.Windows;

namespace ArmaLauncherCli
{
    static public class Configuracion
    {
        // Hacemos una comproboación rápida de los valores de configuración

        public static string ServerURL;
        public static string ServerIP;

        /*
        static public string comprobacion_inicial(MainWindow w)
        {
            bool arma2_encontrado = false;
            bool arma3_encontrado = false;
            bool contraseña_servidores_especificada = false;

            if (File.Exists(w.tb_carpeta_arma2.Text + "\\arma2OA.exe"))
                arma2_encontrado = true;

            if (File.Exists(w.tb_carpeta_arma3.Text + "\\arma3.exe"))
                arma3_encontrado = true;

            if (w.pw_contraseña_servidores.Password != "")
                contraseña_servidores_especificada = true;

            // Montamos el mensaje de respuesta
            string mensaje = "";

            if (!arma2_encontrado && !arma3_encontrado)
                mensaje = "Arma2 y Arma3 no encontrados. ";
            else
            {
                if (!arma2_encontrado)
                    mensaje += "Arma2 no encontrado. ";
                if (!arma3_encontrado)
                    mensaje += "Arma3 no encontrado. ";
            }

            if (!contraseña_servidores_especificada)
                mensaje += "No se ha especificado una contraseña de acceso a los servidores. ";

            if (mensaje != "")
                mensaje = "AVISO: " + mensaje + "Revise la configuración.";
            else
                mensaje = "Configuración revisada y correcta.";

            return mensaje;
        }
        */

        // Cargamos configuración en los controles
        static public void cargar_configuracion(MainWindow w)
        {
            w.tb_carpeta_arma2.Text = Settings.Default.carpeta_a2;
            w.tb_parametros_extra.Text = Settings.Default.parametros_a2;
            w.tb_carpeta_arma3.Text = Settings.Default.carpeta_a3;
            w.tb_parametros_extra_arma3.Text = Settings.Default.parametros_a3;
            w.pw_contraseña_servidores.Password = Settings.Default.contraseña_servidores;



            if (Settings.Default.mods_a2 == "")
            {
                w.radDefaultFolder.IsChecked = true;
                w.txtUserDefined.Text = "";
                w.txtUserDefined.IsEnabled = false;
                w.btn_mods_arma2.IsEnabled = false;
            }
            else
            {
                w.radUserDefinedFolder.IsChecked = true;
                w.txtUserDefined.Text = Settings.Default.mods_a3;
                w.txtUserDefined.IsEnabled = true;
                w.btn_mods_arma2.IsEnabled = true;
            }

            w.txtIP.Text = Settings.Default.serverIP;
            w.txtPort.Text = Settings.Default.serverPort;
            w.txtUrl.Text = Settings.Default.serverUrl;

            ServerIP = Settings.Default.serverIP;

            if (Settings.Default.serverPort != "")
            {
                ServerURL = Settings.Default.serverIP +  "/" + Settings.Default.serverUrl;
            }
            else
            {
                ServerURL = Settings.Default.serverIP + ":" + Settings.Default.serverPort + "/" + Settings.Default.serverUrl;
            }


        }

        internal static void guardar_configuracion(MainWindow w)
        {
            Settings.Default.carpeta_a2 = w.tb_carpeta_arma2.Text;
            Settings.Default.parametros_a2 = w.tb_parametros_extra.Text;
            Settings.Default.carpeta_a3 = w.tb_carpeta_arma3.Text;
            Settings.Default.parametros_a3 = w.tb_parametros_extra_arma3.Text;
            Settings.Default.contraseña_servidores = w.pw_contraseña_servidores.Password;
            
            if (w.radDefaultFolder.IsChecked.Value)
            {
                Settings.Default.mods_a2 = "";
                Settings.Default.mods_a3 = "";
            }
            else
            {
                Settings.Default.mods_a2 = w.txtUserDefined.Text;
                Settings.Default.mods_a3 = w.txtUserDefined.Text;
            }

            Settings.Default.serverIP = w.txtIP.Text;
            Settings.Default.serverPort = w.txtPort.Text;
            Settings.Default.serverUrl = w.txtUrl.Text;

            Settings.Default.Save();
        }

        internal static void restablecer_configuracion(MainWindow w)
        {
            Settings.Default.Reset();
            Settings.Default.Save();
            cargar_configuracion(w);
        }

        internal static void volcar_configuracion(MainWindow w)
        {
            /*
            try
            {
                string version =
                    (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed ? System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(): "en desarrollo");

                StringBuilder s = new StringBuilder();

                s.AppendLine("VOLCADO CONFIGURACIÓN ");
                s.AppendLine("");
                s.AppendLine("Versión programa " + version + ". Fichero generado el " + DateTime.Now.ToString() + ". No se muestran contraseñas.");

                s.AppendLine("");
                s.AppendLine("- Configuración usuario.");
                s.AppendLine("");
                s.AppendLine("Carpeta arma2       : " + w.tb_carpeta_arma2.Text);
                s.AppendLine("Parámetros arma2    : " + w.tb_parametros_extra.Text);
                s.AppendLine("Carpeta arma3       : " + w.tb_carpeta_arma3.Text);
                s.AppendLine("Parámetros arma3    : " + w.tb_parametros_extra_arma3.Text);

                s.AppendLine("");
                s.AppendLine("- Configuración programa.");
                s.AppendLine("");
                s.AppendLine("a2_mods_operaciones     : " + w.a2_mods_operaciones);
                s.AppendLine("a2_mods_academia        : " + w.a2_mods_academia);
                s.AppendLine("a2_mods_pruebas         : " + w.a2_mods_pruebas);
                s.AppendLine("a2_mods_2gm             : " + w.a2_mods_2gm);
                s.AppendLine("a2_mods_vietnam         : " + w.a2_mods_vietnam);
                s.AppendLine("");
                s.AppendLine("a3_mods_operaciones     : " + w.a3_mods_operaciones);
                s.AppendLine("a3_mods_pruebas         : " + w.a3_mods_pruebas);
                s.AppendLine("");
                s.AppendLine("a2_conexion_operaciones : " + w.a2_conexion_operaciones);
                s.AppendLine("a2_conexion_academia    : " + w.a2_conexion_academia);
                s.AppendLine("a2_conexion_pruebas     : " + w.a2_conexion_pruebas);
                s.AppendLine("a2_conexion_2gm         : " + w.a2_conexion_2gm);
                s.AppendLine("a2_conexion_vietnam     : " + w.a2_conexion_vietnam);
                s.AppendLine("");
                s.AppendLine("a3_conexion_operaciones : " + w.a3_conexion_operaciones);
                s.AppendLine("a3_conexion_pruebas     : " + w.a3_conexion_pruebas);
                s.AppendLine("");
                s.AppendLine("servidor_ftp_arma2      : " + w.servidor_ftp_arma2);
                s.AppendLine("servidor_ftp_arma3      : " + w.servidor_ftp_arma3);
                s.AppendLine("usuario_ftp             : " + w.usuario_ftp);
                s.AppendLine();
                s.AppendLine("Lista completa mods arma2: " + String.Join(", ", w.lista_mods_arma2.ToArray()));
                s.AppendLine();
                s.AppendLine("Lista completa mods arma3: " + String.Join(", ", w.lista_mods_arma3.ToArray()));


                string fichero = Path.GetTempFileName() + ".txt";
                File.WriteAllText(fichero, s.ToString());
                Utiles.AbrirFichero(fichero);
            }
            catch (Exception x)
            {
                MessageBox.Show("Error al volcar la configuración : " + x.Message, "12BDI Lanzador", MessageBoxButton.OK, MessageBoxImage.Error);
            }
             */
        }
    }
}
