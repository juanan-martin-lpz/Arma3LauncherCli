using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Forms;
using Arma3LauncherCli.Properties;
using System.IO;
using System.ComponentModel;
//using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Collections.ObjectModel;
//using System.Text.RegularExpressions;
using System.Threading;
using NLog;
using NLog.Targets;
using Newtonsoft.Json;
using System.Windows.Controls;
using ServerManagementClient;
using System.Net.Http;
using System.Net;
using System.Collections;
using FastRsync.Core;
using FastRsync.Delta;
using FastRsync.Hash;
using FastRsync.Signature;
using FastRsync.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

/*************************************************************************************************************************************************
 * 
 * 
 *              
 *      BUGS SOLUCIONADOS
 *      
 *      
 * ***********************************************************************************************************************************************/

namespace ArmaLauncherCli
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        SynchronizationContext synchronizationContext;

        private System.Windows.Media.Brush btnBrush;

        RepositorySource source;

        ServidoresList servers;
        List<ModView> mods;

        private string webrepository;

        RepositoryBay bay;

        private static Logger logger = LogManager.GetCurrentClassLogger();


        CancellationToken token = new CancellationToken();
        CancellationTokenSource tokenSource;

        Diagnosticos diag;


        Servidor serv;

        RepositoryProxy proxy;


        Repositories repositories;

        public MainWindow()
        {

            FileTarget target = LogManager.Configuration.FindTargetByName<FileTarget>("logfile");
            string datemask = System.DateTime.Now.ToShortDateString().Replace("/", "_");
            datemask = datemask.Replace("/", "_");
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\" + "log_" + datemask + ".txt";
            target.FileName = filename;


            logger.Info("Iniciando sesion : {0}", System.DateTime.Now);


            InitializeComponent();

            mods = new List<ModView>();
            servers = new ServidoresList();
            //this.DataContext = servers;

        }




        private async void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            // Versión
            this.Title = "12BDI Lanzador V. ";

            Configuracion.cargar_configuracion(this);

            webrepository = Configuracion.ServerURL;

            logger.Info("=================================================================================");
            logger.Info("Configuracion");
            logger.Info("Arma3 folder {0}", tb_carpeta_arma3.Text);
            logger.Info("Default folder {0}", radDefaultFolder.IsChecked);
            logger.Info("UserDefined folder {0}", radUserDefinedFolder.IsChecked == true ? txtUserDefined.Text : "No definido");
            logger.Info("=================================================================================");

            //
            diag = new Diagnosticos();
            diag.obtener_estado_microfonos();
            //
            diag.PropertyChanged += diagnosticos_PropertyChanged;
            //
            foreach (string m in diag.Microfonos)
            {
                lstMicros.Items.Add(m);
            }

            btnBrush = Test_Red.Background;

            mods = new List<ModView>();

            string rpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\12bdi_launcher\\Repositories";
            string fpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\12bdi_launcher\\modlist.txt";
            string spath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\12bdi_launcher\\Servidores2.txt";

            if (File.Exists(fpath))
            {
                File.Delete(fpath);
            }

            bay = new RepositoryBay();
            source = new RepositorySource(webrepository, bay, pw_contraseña_servidores.Password);
            await source.Connect();

            logger.Info("Conectado al repositorio");


            repositories = new Repositories();

            await repositories.DownloadRepositoriesJson();

            if ((repositories != null) && (repositories.RepositoryProxyList != null))
            {
                lstRepositorios.ItemsSource = repositories.RepositoryProxyList;
                //lstMods.ItemsSource = (lstRepositorios.Items[0] as RepositoryProxy).Mods;
            }

            //rs = new Repositories(rpath);

            // Aqui habria que avisar de que se esta descargando el archivo

            await DownloadFileAsync($"{webrepository}/modlist.txt", fpath);

            logger.Info("modlist.txt descargado correctamente");

            readModList();

            logger.Info("modlist.txt cargado correctamente");

            radArma3.IsChecked = true;

            lstServidores.ItemsSource = source.Servidores;
        }

        //private void ModlistCompleted(object sender, AsyncCompletedEventArgs e)


        /* OBSOLETO
        private void ModlistCompleted(object sender, EventArgs e)
        {
            logger.Info("modlist.txt descargado correctamente");

            readModList();

            logger.Info("modlist.txt cargado correctamente");

            radArma3.IsChecked = true;
            lstServidores.ItemsSource = from s in source.Servidores where s.Arma == "3" select s;
        }
        */

        private void readModList()
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\modlist.txt";

            try
            {

                using (StreamReader r = new StreamReader(filePath))
                {
                    string l;

                    while ((l = r.ReadLine()) != null)
                    {
                        ModView m = new ModView();

                        string[] items = l.Split('|');

                        m.Arma = items[0];
                        m.Name = items[1];

                        mods.Add(m);
                    }
                }

            }
            catch
            {

            }
        }



        // Guardamos la configuración
        private void btn_guardar_configuraciion_Click(object sender, RoutedEventArgs e)
        {
            Configuracion.guardar_configuracion(this);
            Configuracion.cargar_configuracion(this);
        }

        // Restablecemos la configuración por defecto.
        private void btn_restablecer_configuracion_Click(object sender, RoutedEventArgs e)
        {
            Configuracion.restablecer_configuracion(this);
        }

        private void btn_volcar_configuracion_Click(object sender, RoutedEventArgs e)
        {
            Configuracion.volcar_configuracion(this);
        }


        // Encendemos / apagamos botones del formulatrio, cuando el programa está ocupado haciendo otras cosas.
        private void encender_botones()
        {
            btnSalir.IsEnabled = true;
            btn_guardar_configuracion.IsEnabled = true;
            btn_restablecer_configuracion.IsEnabled = true;
            btn_carpeta_arma2.IsEnabled = true;
            btn_carpeta_arma3.IsEnabled = true;
            //btn_comprobar_addons_a2.IsEnabled = true;
            //btn_comprobar_addons_a3.IsEnabled = true;
            //btn_ejecutar_aceclippi.IsEnabled = true;
            btn_volcar_configuracion.IsEnabled = true;
            //btn_comprobar_addons_a3_minimal.IsEnabled = true;

            //pluginsACRE.IsEnabled = true;
            //pluginsTFAR.IsEnabled = true;
            btnReloadAdmin.IsEnabled = true;
            btnxpress.IsEnabled = true;
            radArma2.IsEnabled = true;
            radArma3.IsEnabled = true;
            lstRepositorios.IsEnabled = true;
            lstMods.IsEnabled = true;
            UpdateAllRepos.IsEnabled = true;
            CancelUpdate.IsEnabled = false;
        }

        private void apagar_botones()
        {
            btnSalir.IsEnabled = false;
            btn_guardar_configuracion.IsEnabled = false;
            btn_restablecer_configuracion.IsEnabled = false;
            btn_carpeta_arma2.IsEnabled = false;
            btn_carpeta_arma3.IsEnabled = false;
            //btn_comprobar_addons_a2.IsEnabled = false;
            //btn_comprobar_addons_a3.IsEnabled = false;
            //btn_comprobar_addons_a3_minimal.IsEnabled = false;
            //btn_ejecutar_aceclippi.IsEnabled = false;
            btn_volcar_configuracion.IsEnabled = false;

            //pluginsACRE.IsEnabled = false;
            //pluginsTFAR.IsEnabled = false;

            btnReloadAdmin.IsEnabled = false;
            btnxpress.IsEnabled = false;
            radArma2.IsEnabled = false;
            radArma3.IsEnabled = false;
            lstRepositorios.IsEnabled = false;
            lstMods.IsEnabled = false;
            UpdateAllRepos.IsEnabled = false;
            CancelUpdate.IsEnabled = true;

        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        // Navegación carpetas
        private void btn_carpeta_Click_arma2(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_carpeta_arma2.Text = f.SelectedPath;
            }
        }

        private void btn_carpeta_Click_arma3(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_carpeta_arma3.Text = f.SelectedPath;
            }
        }


        private void btn_ejecutar_aceclippi_Click(object sender, RoutedEventArgs e)
        {
            arranca_juego(tb_carpeta_arma2.Text + @"\@ACE\clippi", "aceclippi.exe", "");
        }

        private void arranca_juego(string carpeta, string ejecutable, string parametros)
        {
            apagar_botones();

            // Proceso principal
            Task t1 = new Task(
            () =>
            {
                try
                {
                    Utiles.ejecuta_proceso(carpeta + @"\" + ejecutable, parametros, carpeta);
                }
                catch (Exception x)
                {
                    System.Windows.MessageBox.Show(x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                }

            });

            // Cuando termine el proceso...
            Task t_final = t1.ContinueWith(
            ant =>
            {
                encender_botones();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            // Arrancamos la tarea inicial
            t1.Start();
        }



        /* OBSOLETO
        private void GenerateRepoCompleted(object sender, Repository.CatalogoCompletedEventArgs e)
        {
            Progress<PublishModsProgress> pprg = new Progress<PublishModsProgress>();

            Repository target = (Repository)sender;

            if (pprg != null)
            {
                pprg.ProgressChanged += (o, pr) =>
                {
                    this.Dispatcher.Invoke(new System.Action(() =>
                    {
                        Estado2.Content = "Copiando " + pr.ProgressFile.ToString() + @"/" + pr.TotalFiles.ToString();
                        SubEstado2.Content = pr.Mod;
                        progreso2.Value = pr.Progress;
                    }));

                };
            }

            target.Publish(pprg);



            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado.Content = "Finalizado";
                SubEstado.Content = "";
                progreso.Value = 0;
            }));

        }
        */

        private void Test_Microphone_Click(object sender, RoutedEventArgs e)
        {

            if (lstMicros.SelectedItem != null)
            {

                if (Test_Microphone.Content.ToString() == "Probar")
                {
                    Test_Microphone.Content = "Detener";
                }
                else
                {
                    Test_Microphone.Content = "Probar";
                    testPB.Value = 0;
                }


                diag.probar_microfono(lstMicros.SelectedItem.ToString());
            }

        }


        private void actualiza_errores(string m)
        {
            txtErrMsg.Text += m;

        }

        void diagnosticos_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentInputLevel") //If event raised by FirstName property value change
            {
                testPB.Value = diag.CurrentInputLevel;
            }

        }

        private void Test_Red_Click(object sender, RoutedEventArgs e)
        {
            Test_Red.Background = btnBrush;

            if (Test_Red.Content.ToString() == "Probar")
            {

                Test_Red.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Lavender);


                if (diag.ping_servidor12bdi(Configuracion.ServerIP, 3, 100))
                {
                    Test_Red.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightYellow);

                }
                else
                {
                    Test_Red.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);

                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Utiles.ejecuta_proceso("dxdiag", "/whql:on", "", true);
        }


        private void btnReloadAdmin_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Se va a reiniciar el Lanzador con permisos de Administrador", "Recargar");

            RunAsAdministrator.ValidateAdministratorModeAndRestart(false);

        }

        private void btnxpress_Click(object sender, RoutedEventArgs e)
        {
            //List<ModData> mods = new List<ModData>();


            //var res = from m in mods where m.Arma == "3" select m.Name;
            //string[] totalmods = res.ToArray<string>();


            Servidor s = (Servidor)lstServidores.SelectedValue;

            serv = ((Servidor)lstServidores.SelectedItem != null ? (Servidor)lstServidores.SelectedItem : (Servidor)lstServidores.Items[0]);


            xpresslauncher launcher = new xpresslauncher(tb_carpeta_arma2.Text, tb_carpeta_arma3.Text, serv);

            launcher.ShowDialog();
            //launcher.Show();
        }




        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            /*
            string rpt_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Arma 3");
            
            var dir = new DirectoryInfo(rpt_path);
            var file = (from f in dir.GetFiles()
                          orderby f.LastWriteTime descending
                          select f).First();


            Utiles.carga_ficheroFTP(servidor_ftp_arma3,file.FullName, usuario_ftp, contraseña_ftp);

            System.Windows.MessageBox.Show("Archivo RPT subido al servidor.", "12BDI_lanzador", MessageBoxButton.OK, MessageBoxImage.Information);
            */

        }


        private void Launcher_Click(object sender, RoutedEventArgs e)
        {

            serv = ((Servidor)lstServidores.SelectedItem != null ? (Servidor)lstServidores.SelectedItem : (Servidor)lstServidores.Items[0]);

            string conexion = @"-connect=" + serv.IP + @" -port=" + serv.Puerto + @" -password=" + serv.Password;

            string lista_mods = "";
            string executable = "";
            string modp = "";
            string gfolder = "";

            if (radArma3.IsChecked.Value)
            {
                executable = "arma3.exe";
                //gfolder = tb_carpeta_arma2.Text;
                gfolder = tb_carpeta_arma3.Text;

                if (!radDefaultFolder.IsChecked.Value) { modp = txtUserDefined.Text; };
            }
            else
            {
                executable = "arma3_x64.exe";
                gfolder = tb_carpeta_arma3.Text;
                if (!radDefaultFolder.IsChecked.Value) { modp = txtUserDefined.Text; };

            }



            if (radDefaultFolder.IsChecked.Value)
            {
                lista_mods = @" -mod=" + serv.ModListToString();
            }
            else
            {
                modp = txtUserDefined.Text + @"\" + serv.Repository;
                lista_mods = @" -mod=" + serv.ModListToString(modp);
            }



            string parametros = (cb_conexion_directa.IsChecked.HasValue && cb_conexion_directa.IsChecked.Value == true ? conexion : "") + lista_mods + " " + tb_parametros_extra_arma3.Text;

            logger.Info("Se va a lanzar el juego con los siguiente parametros : ");
            logger.Info("{0}", parametros);

            arranca_juego(gfolder, executable, parametros);

        }

        private void Copiar_Click(object sender, RoutedEventArgs e)
        {
            serv = ((Servidor)lstServidores.SelectedItem != null ? (Servidor)lstServidores.SelectedItem : (Servidor)lstServidores.Items[0]);

            string conexion = @"-connect=" + serv.IP + @" -port=" + serv.Puerto + @" -password=" + serv.Password;
            string lista_mods = @" -mod=" + serv.ModListToString(); // + @";" + serv.MapListToString();

            System.Windows.Clipboard.SetText(conexion + lista_mods);

        }

        private async void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            #region "New Update Process"

            logger.Info("Iniciando proceso de actualizacion NG");
            apagar_botones();
            Estado.Content = "Actualizando Mods";

            proxy = ((RepositoryProxy)lstRepositorios.SelectedItem != null ? (RepositoryProxy)lstRepositorios.SelectedItem : (RepositoryProxy)lstRepositorios.Items[0]);

            string carpeta_juego = "";
            string varma = radArma3.IsChecked == true ? "3" : "2";


            if (radDefaultFolder.IsChecked.Value)
            {
                carpeta_juego = tb_carpeta_arma3.Text;
            }
            else
            {
                carpeta_juego = txtUserDefined.Text + @"/" + proxy.Nombre;
            }

            if (!Directory.Exists(carpeta_juego))
            {
                Directory.CreateDirectory(carpeta_juego);
            }

            synchronizationContext = SynchronizationContext.Current;

            var context = TaskScheduler.FromCurrentSynchronizationContext();

            // Core action
            //lstRepositorios.IsEnabled = false;
            //UpdateAllRepos.IsEnabled = false;

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            //CancellationToken token = new CancellationToken();

            Task t = Task.Factory.StartNew(async () => {

                await ProcessUpdate(proxy, carpeta_juego).ContinueWith((result) => {
                    resetEstado("Terminado");
                    encender_botones();
                    logger.Info("Fin del proceso de actualizacion NG");

                });

            }, token, TaskCreationOptions.None, context);

            #endregion


            #region "Obsolete"
            /*

            logger.Info("Iniciando proceso de actualizacion");

            apagar_botones();

            Estado.Content = "Actualizando Mods";

            
            proxy = ((RepositoryProxy)lstRepositorios.SelectedItem != null ? (RepositoryProxy)lstRepositorios.SelectedItem : (RepositoryProxy)lstRepositorios.Items[0]);

            //string carpeta_juego = "";
            //string varma = radArma3.IsChecked == true ? "3" : "2";


            if (radDefaultFolder.IsChecked.Value)
            {
                carpeta_juego = tb_carpeta_arma3.Text;

                //if (radArma3.IsChecked.Value) { carpeta_juego = tb_carpeta_arma3.Text; }
                //if (radArma2.IsChecked.Value) { carpeta_juego = tb_carpeta_arma2.Text; }
            }
            else
            {
                carpeta_juego = txtUserDefined.Text + @"/" + proxy.Nombre;
            }

            if (!Directory.Exists(carpeta_juego))
            {
                Directory.CreateDirectory(carpeta_juego);
            }

            // Descargamos la base de datos del servidor y repositorio indicado
            db_name = source.GetRepositoryCatalog(proxy.Nombre);

            // Creamos una instancia del repositorio local
            Estado.Content = "Catalogando Mods";

            // Progreso de la actualizacion
            Progress<CatalogModsProgress> prg = new Progress<CatalogModsProgress>();

            if (prg != null)
            {
                prg.ProgressChanged += (o, pr) =>
                {
                    Estado.Content = "Procesando " + pr.ProgressMod.ToString() + @"/" + pr.TotalMods.ToString();
                    SubEstado.Content = pr.Mod;
                    progreso.Value = pr.Progress;
                };
            }

            try
            {
                logger.Info("Cargando el repositorio {0}", proxy.Nombre);

                if (cliente != null)
                {
                    cliente = null;
                }

                cliente = new Repository(carpeta_juego, bay, proxy.Nombre, mods);

                logger.Info("Asignando manejadores de evento");

                /**************************************************************************************************************************
                 * 
                 *              CatalogoCompleted
                 *                      |
                 *            CatalogoCompareComleted
                 *                      |
                 *        UpgradeRepositoryProgressChanged
                 *             /                    \
                 *            /                      \
                 *      PlanProgressChanged     UpgradeRepositoryCompleted 
                 *      
                 * *************************************************************************************************************************/
            /*
            // Catalogado de addons en equipo cliente
            cliente.CatalogoCompleted += new Repository.CatalogoCompletedEventHandler(catalogcomplete);

            // Comparacion de catalagos entre cliente y servidor
            cliente.CatalogoCompareCompleted += new Repository.CatalogoCompareCompletedEventHandler(compareCompleted);

            // Progreso de la actualizacion
            cliente.UpgradeRepositoryProgressChanged += new Repository.UpgradeRepositoryProgressChangedEventHandler(updateTasks);

            // Antes de invocar la actualizacion
            //cliente.UpgradeRepositoryBeforeExecute += new Repository.UpgradeRepositoryBeforeExecuteEventHandler(updateTasks);

            // Progreso global
            cliente.PlanProgressChanged += new Repository.PlanProgressChangedEventHandler(progresoPlan);

            // Actualizacion terminada
            cliente.UpgradeRepositoryCompleted += new Repository.UpgradeRepositoryCompletedEventHandler(tareasCompletadas);

            

            //proxy.MustUpdate = false;
        }
        catch (Exception ex)
        {
            logger.Fatal("Se ha producido una excepcion {0}", ex.Message);
            throw ex;
        }

        Estado.Content = "Trabajando...";

        try
        {

            logger.Info("Iniciando el catalogo de addons");

            cliente.CatalogFolderAsync(bay, proxy.Nombre, prg);
        }
        catch (Exception ex)
        {
            logger.Fatal("Se ha producido una excepcion al catalogar addons {0}", ex.Message);

            throw ex;
        }

        */
            #endregion

        }

        private void actualizaProgreso(long prog)
        {

            var context = SynchronizationContext.Current;

            context.OperationStarted();

            context.Send(new SendOrPostCallback(e =>
            {
                progreso.Value = prog;
                progreso.Refresh();

            }), prog);

            context.OperationCompleted();
        }

        private void actualizaEstado(string estado)
        {
            /*
            this.Dispatcher.Invoke(new System.Action<string>((e) => {
                Estado.Content = e;
                Estado.Refresh();

            }), estado);
            */

            var context = SynchronizationContext.Current;

            context.OperationStarted();

            context.Send(new SendOrPostCallback(e =>
            {
                Estado.Content = e;
                Estado.Refresh();

            }), estado);

            context.OperationCompleted();
        }

        private void actualizaSubEstado(string estado)
        {
            /*
            this.Dispatcher.Invoke(new System.Action<string>((e) => {
                SubEstado.Content = e;
                SubEstado.Refresh();                

            }), estado);
            */

            var context = SynchronizationContext.Current;

            context.OperationStarted();

            synchronizationContext.Send(new SendOrPostCallback(e =>
            {
                SubEstado.Content = e;
                SubEstado.Refresh();

            }), estado);

            context.OperationCompleted();
        }

        private void resetEstado(string estado)
        {
            /*
            this.Dispatcher.Invoke(new System.Action<string>((e) => {
                Estado.Content = e;
                Estado.Refresh();
                SubEstado.Content = "";
                SubEstado.Refresh();
                progreso.Value = 0;
                progreso.Refresh();

                lstRepositorios.IsEnabled = !lstRepositorios.IsEnabled;
                UpdateAllRepos.IsEnabled = !UpdateAllRepos.IsEnabled;

            }),estado);
            */

            var context = SynchronizationContext.Current;

            if (context != null)
            {
                context.OperationStarted();

                context.Send(new SendOrPostCallback(e =>
                {
                    Estado.Content = e;
                    Estado.Refresh();
                    SubEstado.Content = "";
                    SubEstado.Refresh();
                    progreso.Value = 0;
                    progreso.Refresh();

                }), estado);

                context.OperationCompleted();
            }

        }

        private string GetRelativePath(string path, string root, bool includeLeadingSlash = true)
        {
            string relative = path.Replace(root, "");

            if (!includeLeadingSlash)
            {
                relative = relative.TrimStart('\\');
            }

            return relative;
        }

        private async Task<bool> ProcessUpdate(RepositoryProxy p, string targetFolder)
        {
            resetEstado("Iniciando...");

            CancelUpdate.IsEnabled = true;

            // Descargamos ficheros.json

            string ficherosjson = $"{webrepository}/{p.Nombre}/ficheros.json";
            string ficherosjsontarget = $"{bay.ToDirectoryInfo().FullName}\\{bay.GetDirectoryForRepo(p.Nombre)}\\ficheros.json";

            File.Delete(ficherosjsontarget);

            if (!await DownloadFileAsync(ficherosjson, ficherosjsontarget))
            {
                logger.Error($"Error al descargar {ficherosjson}");
                return false;
            }

            // Deserializamos ficheros.json
            var content = File.ReadAllText(ficherosjsontarget);

            List<ServerManagementClient.Models.ModViewModel> mods = new List<ServerManagementClient.Models.ModViewModel>();


            mods = JsonConvert.DeserializeObject<List<ServerManagementClient.Models.ModViewModel>>(content);



            IFileHash compute_hash = new FileHashXXHash();

            int globalRetries = 0;
            int count = 0;


            foreach (ServerManagementClient.Models.ModViewModel mod in mods)
            {

                if (token.IsCancellationRequested)
                {
                    break;
                }
                //synchronizationContext.Send(new SendOrPostCallback(e => { this.Refresh(); }), "");

                if (globalRetries == 5)
                {
                    logger.Error($"Parece que existen problemas para descargar algunos archivos. Notifique al administrador");

                    actualizaSubEstado($"Ha habido problemas durante la descarga. Operacion abortada");

                    break;
                }

                count++;

                actualizaEstado($"Actualizando mods ({count}/{mods.Count})");

                string folder = $"{targetFolder}\\{mod.Ruta}";

                string filename = $"{folder}\\{mod.Nombre}";

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (File.Exists(filename))
                {
                    // check signature
                    actualizaSubEstado($"Chequeando {filename}");

                    //string file_hash = await compute_hash.ComputeHashAsync(filename);

                    actualizaSubEstado($"Chequeado...");

                    //bool canContinue = false;

                    //while (!canContinue)
                    //{
                        //Task.WaitAll();
                        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                        TimeSpan ellapsed;

                        //if (file_hash != mod.Firma)
                        //{
                            string f = $"{webrepository}/{p.Nombre}/{mod.Ruta}/{mod.Nombre}";

                            /*
                            actualizaSubEstado($"Nueva version de {mod.Nombre} encontrada. Descargando");

                            logger.Warn($"La replica de {f} en el equipo no tiene la misma firma que en el servidor");
                            logger.Warn($"El fichero {f} se va a descargar otra vez");

                            // Si no son iguales, descargamos

                            if (!await DownloadFileAsync(f, filename))
                            {
                                nretries++;
                                actualizaSubEstado($"Error en la descarga, descargando {mod.Nombre} de nuevo {nretries}/3");

                                logger.Error($"Error al descargar {mod.Nombre}");
                            }

                            if (nretries == 3)
                            {
                                actualizaSubEstado($"Imposible descargar {mod.Nombre}. Se procede con el siguiente");

                                logger.Error($"Ha habido un problema al descargar {mod.Nombre}, pero se puede continuar");
                                canContinue = true;

                                globalRetries++;
                            }

                            */

                            // Informamos
                            actualizaSubEstado($"Archivo diferente en cliente: Generando Delta");
                            logger.Warn("Archivo diferente en cliente: Generando Delta");

                            string basisFilePath = filename;
                            string signatureFilePath = filename + @".sig";
                            string modfile = filename;
                            string deltaFilePath = filename + @".delta";
                            string newFilePath = filename + @".new";

                            var signatureBuilder = new SignatureBuilder();

                            using (var basisStream = new FileStream(basisFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (var signatureStream = new FileStream(signatureFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                actualizaSubEstado($"Generando Signature para ${Path.GetFileName(basisFilePath)}");
                                logger.Warn("Generando Signature para " + Path.GetFileName(basisFilePath));

                                watch.Start();
                                signatureBuilder.Build(basisStream, new SignatureWriter(signatureStream));
                                watch.Stop();

                                ellapsed = watch.Elapsed;
                                logger.Warn("Signature generada en " + ellapsed.Minutes + ":" + ellapsed.Seconds + ":" + ellapsed.Milliseconds);

                            }

                            ConsoleProgressReporter reporter = new ConsoleProgressReporter();

                            using (var signatureStream = new FileStream(signatureFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                actualizaSubEstado($"Enviando Signature para ${Path.GetFileName(basisFilePath)}");
                                logger.Warn("Enviando Signature para " + Path.GetFileName(basisFilePath));

                                byte[] bArray = new byte[signatureStream.Length];
                                signatureStream.Read(bArray, 0, (int)signatureStream.Length);

                                // Generate post objects

                                string n = p.Nombre; //.Replace("\\", "");
                                string r = mod.Ruta; //.Replace("\\", "");
                                string no = mod.Nombre; //.Replace("\\", "");
                                string format = Path.GetExtension(filename).Replace(".", "");

                                modfile = $"{n}\\{r}\\{no}";

                                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                                postParameters.Add("filename", modfile);
                                postParameters.Add("fileformat", format);
                                postParameters.Add("file", new FormUpload.FileParameter(bArray, modfile, "application/octet-stream"));

                                string postURL = "http://188.165.254.137/deltaapp/api/delta/getdelta";
                                string userAgent = "Launcher";

                                actualizaSubEstado($"Esperando respuesta para ${Path.GetFileName(basisFilePath)}");
                                logger.Warn("Esperando respuesta para " + Path.GetFileName(basisFilePath));

                                watch.Reset();
                                watch.Start();
                                HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, userAgent, postParameters);
                                watch.Stop();

                                ellapsed = watch.Elapsed;
                                logger.Warn("Delta descargado en " + ellapsed.Minutes + ":" + ellapsed.Seconds + ":" + ellapsed.Milliseconds);

                                // Process response
                                actualizaSubEstado($"Respuesta recibida para ${Path.GetFileName(basisFilePath)}");
                                logger.Warn("Respuesta recibida para " + Path.GetFileName(basisFilePath));

                                var responseReader = webResponse.GetResponseStream();

                                int bytesProcessed = 0;

                                actualizaSubEstado($"Escribiendo Delta para ${Path.GetFileName(basisFilePath)}");
                                logger.Warn("Escribiendo Delta para " + Path.GetFileName(basisFilePath));

                                using (var streamDeltaWriter = new FileStream(deltaFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                                {
                                    // Allocate a 1k buffer
                                    byte[] buffer = new byte[1024];
                                    int bytesRead;

                                    // Simple do/while loop to read from stream until
                                    // no bytes are returned
                                    do
                                    {
                                        // Read data (up to 1k) from the stream
                                        bytesRead = responseReader.Read(buffer, 0, buffer.Length);

                                        // Write the data to the local file
                                        streamDeltaWriter.Write(buffer, 0, bytesRead);

                                        // Increment total bytes processed
                                        bytesProcessed += bytesRead;
                                    } while (bytesRead > 0);
                                }

                                webResponse.Close();

                                var delta = new DeltaApplier
                                {
                                    SkipHashCheck = true
                                };
                                using (var basisStream = new FileStream(basisFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                using (var deltaStream = new FileStream(deltaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                using (var newFileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                                {
                                    actualizaSubEstado($"Aplicando Delta para ${Path.GetFileName(basisFilePath)}");
                                    logger.Warn("Aplicando Delta para " + Path.GetFileName(basisFilePath));

                                    watch.Reset();
                                    watch.Start();
                                    delta.Apply(basisStream, new BinaryDeltaReader(deltaStream, reporter), newFileStream);
                                    watch.Stop();

                                    ellapsed = watch.Elapsed;
                                    logger.Warn("Delta aplicado en " + ellapsed.Minutes + ":" + ellapsed.Seconds + ":" + ellapsed.Milliseconds);

                                }
                            }

                            actualizaSubEstado($"Eliminando temporales para ${Path.GetFileName(basisFilePath)}");
                            logger.Warn("Eliminando temporales para " + Path.GetFileName(basisFilePath));

                            File.Delete(basisFilePath);
                            File.Delete(signatureFilePath);
                            File.Move(newFilePath, basisFilePath);
                            File.Delete(deltaFilePath);

                            actualizaSubEstado($"{mod.Nombre} esta OK");

                            await Task.Delay(500);

                            //canContinue = true;
                        /*
                        }
                        else
                        {
                            actualizaSubEstado($"{mod.Nombre} esta OK");

                            canContinue = true;
                        }
                        */
                        
                    //}
                }
                else
                {
                    string f = $"{webrepository}/{p.Nombre}/{mod.Ruta}/{mod.Nombre}";

                    actualizaSubEstado($"Descargando {mod.Nombre}");

                    if (!await DownloadFileAsync(f, filename))
                    {
                        logger.Error($"Error al descargar {mod.Nombre}");

                        actualizaSubEstado($"Imposible descargar {mod.Nombre}. Se procede con el siguiente");

                    }
                    else
                    {
                        // check signature

                        actualizaSubEstado($"Chequeando {filename}");

                        string file_hash = compute_hash.ComputeHash(filename);

                        int nretries = 1;
                        bool canContinue = false;

                        while (!canContinue)
                        {
                            //synchronizationContext.Send(new SendOrPostCallback(e => { this.Refresh(); }), "");

                            if (file_hash != mod.Firma)
                            {
                                actualizaSubEstado($"Nueva version de {mod.Nombre} encontrada. Descargando");

                                logger.Warn($"La replica de {f} en el equipo no tiene la misma firma que en el servidor");
                                logger.Warn($"El fichero {f} se va a descargar otra vez");

                                // Si no son iguales, descargamos

                                if (!await DownloadFileAsync(f, filename))
                                {
                                    nretries++;

                                    actualizaSubEstado($"Error en la descarga, descargando {mod.Nombre} de nuevo {nretries}/3");

                                    logger.Error($"Error al descargar {mod.Nombre}");
                                }

                                if (nretries == 3)
                                {
                                    actualizaSubEstado($"Imposible descargar {mod.Nombre}. Se procede con el siguiente");

                                    logger.Error($"Ha habido un problema al descargar {mod.Nombre}, pero se puede continuar");
                                    canContinue = true;

                                    globalRetries++;
                                }

                                canContinue = true;
                            }
                            else
                            {
                                canContinue = true;
                            }
                        }
                    }
                }


                //this.Dispatcher.Invoke(new System.Action(() => this.Refresh()));
            }



            // Proceso de eliminacion:
            // 
            //      Generar lista en cliente
            //      Comparar con los elementos de servidor
            //      

            if (radUserDefinedFolder.IsChecked == true)
            {

                var dirs = Directory.EnumerateDirectories(targetFolder);

                actualizaEstado("Eliminando");

                foreach (string dir in dirs)
                {
                    string nombre = GetRelativePath(dir, targetFolder, false);

                    if (nombre.StartsWith("@"))
                    {
                        if (!mods.Exists(m => m.Mod == nombre))
                        {
                            actualizaSubEstado($"Se va a eliminar {nombre}");

                            try
                            {
                                Directory.Delete(dir, true);
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }

                    /* OBSOLETO
                    var files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories);

                    actualizaEstado("Eliminando archivos obsoletos");

                    foreach (string file in files)
                    {
                        synchronizationContext.Send(new SendOrPostCallback(e => { this.Refresh(); }), "");

                        if (!mods.Exists(m =>
                        {

                            string n = targetFolder + m.Ruta + @"\\" + m.Nombre;

                            if (Path.GetFullPath(n) == Path.GetFullPath(file))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }

                        }))
                        {
                            actualizaSubEstado($"Se va a eliminar {file}");
                            File.Delete(file);
                        }

                    }
                    */
                    //synchronizationContext.Send(new SendOrPostCallback(e => { this.Refresh(); }), "");
                }

            }


            CancelUpdate.IsEnabled = false;

            return true;

        }


        private async Task<bool> DownloadFileAsync(string url, string target)
        {
            using (var client = new HttpClientDownload(url, target))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) => {
                    this.progreso.Value = (double)progressPercentage;
                };

                try
                {
                    token.Register(() => client.CancelDownload(token));

                    logger.Info($"Descargando el fichero {url} en {target} ");
                    await client.StartDownload();
                }
                catch (Exception e)
                {
                    logger.Error($"La descarga del fichero {url} ha fallado");
                    logger.Error($"{e.Message}");

                    return false;
                }
            }

            return true;

        }

        /* OBSOLETO
        // OBSOLETO
        private void tareasCompletadas(object sender, AsyncCompletedEventArgs e)
        {       
            logger.Info("Completado el proceso de Actualizacion de Addons");

            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado.Content = "Terminado";
                SubEstado.Content = "";
                progreso.Value = 0;
                this.lstServidores.IsEnabled = true;

                encender_botones();
            }));
        }

        // OBSOLETO
        private void compareCompleted(object sender, Repository.CatalogoCompareCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado.Content = "Detenido";
                SubEstado.Content = e.NotInClient.ToString() + " se van a descargar. " + e.DistinctSignature.ToString() + " se van a sustituir";
                progreso.Value = 0;
            }));

            logger.Info("Terminado el proceso de comparacion");
            logger.Info("Iniciando el proceso de Descarga y Copia de Addons");
            cliente.UpgradeRepositoryAsync();

        }

        // OBSOLETO
        private void catalogcomplete(object sen, Repository.CatalogoCompletedEventArgs eve)
        {
            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado.Content = "Detenido";
                SubEstado.Content = eve.NewFiles.ToString() + " archivos nuevos de " + eve.Total.ToString() + " archivos";
                progreso.Value = 0;
            }));

            // Comparamos los catalagos
            logger.Info("Terminado el proceso de Catalogo");
            logger.Info("Iniciando el proceso de Comparacion");

            cliente.CompareCatalogs(db_name, proxy.Nombre, source.Address);
            

        }


        // OBSOLETO
        private void createRepoCompleted(object sen, Repository.CatalogoCompletedEventArgs eve)
        {
            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado2.Content = "Detenido";
                SubEstado2.Content = eve.NewFiles.ToString() + " archivos nuevos de " + eve.Total.ToString() + " archivos";
                progreso2.Value = 0;
            }));

            // Comparamos los catalagos
            //cliente.CompareCatalogs(db_name, serv.Repository, source.Address);

            Progress<PublishModsProgress> prg = new Progress<PublishModsProgress>();

            prg.ProgressChanged += (o, pr) => {
                this.Dispatcher.Invoke(new System.Action(() =>
                {
                    Estado2.Content = "Procesando " + pr.ProgressFile.ToString() + @"/" + pr.TotalFiles.ToString();
                    SubEstado2.Content = pr.Mod;
                    progreso2.Value = pr.Progress;
                }));
            };

            Repository re = (Repository)sen;
            re.Publish(prg);

        }

        // OBSOLETO
        private void progresoPlan(object sender, Repository.PlanProgressEventArgs e)
        {

            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado.Content = "Tarea " + e.Current.ToString() + " de " + e.Total.ToString();
                SubEstado.Content = "Descargando " + Path.GetFileName(e.CurrentFilename);
                progreso.Value = e.ProgressPercentage;
            }));

        }

        // OBSOLETO
        public string PrettyFormat(TimeSpan span)
        {

            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new System.Text.StringBuilder();
            if (span.Days > 0)
                sb.AppendFormat("{0} day{1} ", span.Days, span.Days > 1 ? "s" : String.Empty);
            if (span.Hours > 0)
                sb.AppendFormat("{0} hour{1} ", span.Hours, span.Hours > 1 ? "s" : String.Empty);
            if (span.Minutes > 0)
                sb.AppendFormat("{0} minute{1} ", span.Minutes, span.Minutes > 1 ? "s" : String.Empty);
            return sb.ToString();

        }

        // OBSOLETO
        private void updateTasks(object sender, Repository.TaskProgressProgressChanged e)
        {
            try
            {
                this.Dispatcher.Invoke(new System.Action(() =>
                {

                    //SubEstado.Content = SubEstado.Content + "(" + PrettyFormat(e.Left) + ")";
                }));
                /*
                this.Dispatcher.Invoke(new System.Action(() =>
                {
                //Estado.Content = "Ejecutando Tarea";
                    SubEstado.Content = e.Message;
                    progreso.Value = e.ProgressPercentage;           
                }));
                
            }
            catch (Exception ex)
            {
                logger.Fatal("Error en UpdateTask {0}", ex.Message);

                Console.WriteLine(ex.Message);
            }
        }


        // OBSOLETO
        private void DetallesActualizacion_Click(object sender, RoutedEventArgs e)
        {
            if ((cliente != null) && (cliente.Downloading))
            {
                dp = new DownloadProgress(cliente);
                dp.Show();
            }

        }
        */

        private void radArma3_Checked(object sender, RoutedEventArgs e)
        {
            //lstServidores.ItemsSource = from s in source.Servidores where s.Arma == "3" select s;
        }

        private void radArma2_Checked(object sender, RoutedEventArgs e)
        {
            //lstServidores.ItemsSource = from s in source.Servidores where s.Arma == "2" select s;
        }


        private void btn_mods_arma2_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtUserDefined.Text = f.SelectedPath;
            }

        }

        private void radDefaultFolder_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //a2.IsEnabled = true;
                //a3.IsEnabled = true;
                txtUserDefined.IsEnabled = false;
                txtUserDefined.Text = "";
                btn_mods_arma2.IsEnabled = false;
            }
            catch
            { }
        }

        private void radUserDefinedFolder_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                //a2.IsEnabled = false;
                //a3.IsEnabled = false;
                txtUserDefined.IsEnabled = true;
                btn_mods_arma2.IsEnabled = true;
            }
            catch
            { }
        }

        /* OBSOLETO
        private void Window_Closing(object sender, CancelEventArgs e)
        {

            if (dp != null)
            {
                dp.Close();
                dp = null;
            }

            try
            {
                if (cliente != null)
                {
                    cliente.CloseAll();
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
        */
        // Seleccion de Repositorios. List SelectionChanged
        // Referencias
        //      RepositoryProxy
        //
        private void lstRepositorios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RepositoryProxy r = (RepositoryProxy)lstRepositorios.SelectedItem;

            if (r != null)
            {
                lstMods.ItemsSource = r.Mods;
            }
        }


        // Nuevo proceso de actualizacion. Button Handler
        // Referencias
        //      RepositoryProxy
        //
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            logger.Info("Iniciando proceso de actualizacion NG");
            apagar_botones();
            Estado.Content = "Actualizando Mods";

            int count = 0;

            var context = TaskScheduler.FromCurrentSynchronizationContext();

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            foreach (RepositoryProxy proxy in repositories.RepositoryProxyList)
            {
                count++;

                //lblAll.Visibility = Visibility.Visible;
                //lblAll.Content = $"Procesando Repositorio {proxy.Nombre} ({count}/{repositories.RepositoryProxyList.Count})";

                string carpeta_juego = "";
                string varma = radArma3.IsChecked == true ? "3" : "2";

                if (radDefaultFolder.IsChecked.Value)
                {
                    carpeta_juego = tb_carpeta_arma3.Text;
                }
                else
                {
                    carpeta_juego = txtUserDefined.Text + @"/" + proxy.Nombre;
                }

                if (!Directory.Exists(carpeta_juego))
                {
                    Directory.CreateDirectory(carpeta_juego);
                }

                Task t = Task.Factory.StartNew(async () => {await ProcessUpdate(proxy, carpeta_juego);}, token, TaskCreationOptions.PreferFairness, context);

                await t.ContinueWith((result) => { resetEstado("Mod " + proxy.Nombre + " terminado"); });
            }
            
            /*
            Task t2 = t.ContinueWith((result) =>
            {
                resetEstado("Terminado");
                encender_botones();
            }, token,TaskContinuationOptions.AttachedToParent, context);
            */
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }
            
        }

        private void CancelUpdateBtn(object sender, RoutedEventArgs e)
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();

                resetEstado("Cancelado por el Usuario");
            }

        }
    }
}
