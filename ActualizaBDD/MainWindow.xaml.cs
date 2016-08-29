﻿using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Forms;
using ActualizaBDD.Properties;
using System.IO;
using System.ComponentModel;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Collections.ObjectModel;
using ServidoresData;
using System.Text.RegularExpressions;
using System.Threading;
using Db4objects.Db4o.Linq;
using NLog;
using NLog.Targets;

namespace ActualizaBDD
{    
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        internal string servidor_12bdi = "188.165.254.137";
        private System.Windows.Media.Brush btnBrush;

        ServidoresData.RepositorySource source;

        ServidoresList servers;
        List<ModView> mods;

        private string webrepository = "http://188.165.254.137/WebRepo";

        WebDownload wbm;
        RepositoryBay bay;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        
        Diagnosticos diag;
        
        Repository cliente;
        string db_name;
        Servidor serv;

        Repository re;
        

        public MainWindow()
        {

            FileTarget target = LogManager.Configuration.FindTargetByName<FileTarget>("logfile");
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\" + "log_" + System.DateTime.Now.ToShortDateString().Replace("/","_") + ".txt";
            target.FileName = filename;


            logger.Info("Iniciando sesion : {0}", System.DateTime.Now);
            

            InitializeComponent();

            mods = new List<ModView>();
            servers = new ServidoresList();
            //this.DataContext = servers;

        }
        

        

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            // Versión
            this.Title = "12BDI Lanzador V. ";

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                this.Title += "" + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                solapa_administracion.IsEnabled = false;

                logger.Info("Version : {0}", System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString());
                logger.Info("");

            }
            else
            {
                this.Title += "desarrollo";
                solapa_administracion.IsEnabled = true;

                logger.Info("Version : {0}", "Desarrollo");
                logger.Info("");

            }

            Configuracion.cargar_configuracion(this);
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

            string fpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\modlist.txt";
            string spath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\Servidores2.txt";
            
            if (File.Exists(fpath))
            {
                File.Delete(fpath);
            }

            bay = new RepositoryBay();
            source = new RepositorySource(webrepository, bay, pw_contraseña_servidores.Password);
            source.Connect();

            logger.Info("Conectado al repositorio");

            Repositories rs = new Repositories(@"E:\Mods");

            // La idea es hacerlo en repositorysource
            wbm = new WebDownload();
            wbm.DownloadFileCompleted += new AsyncCompletedEventHandler(ModlistCompleted);
            wbm.DownloadFileAsync(webrepository, "/modlist.txt", fpath);

            logger.Info("modlist.txt descargado correctamente");
        }

        private void ModlistCompleted(object sender, AsyncCompletedEventArgs e)
        {
            readModList();

            logger.Info("modlist.txt cargado correctamente");

            radArma3.IsChecked = true;
        }

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
            btn_crear_repositorio_a2.IsEnabled = true;
            btn_crear_repositorio_a3.IsEnabled = true;
            btn_guardar_configuracion.IsEnabled = true;
            btn_restablecer_configuracion.IsEnabled = true;
            btn_carpeta_arma2.IsEnabled = true;
            btn_carpeta_arma3.IsEnabled = true;
            btn_comprobar_addons_a2.IsEnabled = true;
            btn_comprobar_addons_a3.IsEnabled = true;
            btn_ejecutar_aceclippi.IsEnabled = true;
            btn_volcar_configuracion.IsEnabled = true;
            btn_comprobar_addons_a3_minimal.IsEnabled = true;

            btn_catalogo_seguro.IsEnabled = true;
            pluginsACRE.IsEnabled = true;
            pluginsTFAR.IsEnabled = true;
            btnReloadAdmin.IsEnabled = true;
            btnxpress.IsEnabled = true;
            radArma2.IsEnabled = true;
            radArma3.IsEnabled = true;

        }

        private void apagar_botones()
        {
            btnSalir.IsEnabled = false;
            btn_crear_repositorio_a2.IsEnabled = false;
            btn_crear_repositorio_a3.IsEnabled = false;
            btn_guardar_configuracion.IsEnabled = false;
            btn_restablecer_configuracion.IsEnabled = false;
            btn_carpeta_arma2.IsEnabled = false;
            btn_carpeta_arma3.IsEnabled = false;
            btn_comprobar_addons_a2.IsEnabled = false;
            btn_comprobar_addons_a3.IsEnabled = false;
            btn_comprobar_addons_a3_minimal.IsEnabled = false;
            btn_ejecutar_aceclippi.IsEnabled = false;
            btn_volcar_configuracion.IsEnabled = false;

            btn_catalogo_seguro.IsEnabled = false;
            pluginsACRE.IsEnabled = false;
            pluginsTFAR.IsEnabled = false;

            btnReloadAdmin.IsEnabled = false;
            btnxpress.IsEnabled = false;
            radArma2.IsEnabled = false;
            radArma3.IsEnabled = false;

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

        private void btn_crear_repositorio_a3_Click(object sender, RoutedEventArgs e)
        {
            
            apagar_botones();

            //log("Generando repositorio: ");

            string carpeta_entrada = tb_carpeta_base_arma3.Text;
            string carpeta_salida = tb_carpeta_repositorio.Text;
            string repositorio = txtRepo.Text;

            // Proceso principal
            Task t1 = new Task(
            () =>
            {
                try
                {
                    //voidStringDelegate dlog = log;

                    //var res = from m in mods where m.Arma == "3" select m.Name;

                    re = new Repository(carpeta_entrada, repositorio, carpeta_salida, this.mods);
                    re.CreateRepositoryCompleted += new Repository.CreateRepositoryCompletedEventHandler(GenerateRepoCompleted);

                    Progress<CatalogModsProgress> prg = new Progress<CatalogModsProgress>();

                    if (prg != null)
                    {
                        prg.ProgressChanged += (o, pr) =>
                        {
                            this.Dispatcher.Invoke(new System.Action(() =>
                            {
                                Estado2.Content = "Procesando " + pr.ProgressMod.ToString() + @"/" + pr.TotalMods.ToString();
                                SubEstado2.Content = pr.Mod;
                                progreso2.Value = pr.Progress;
                            }));

                        };
                    }

                    re.CatalogRepositoryAsync(prg);

                    

                    //
                    /*
                    string[] dirs = Directory.GetDirectories(carpeta_entrada);

                    var res = from d in dirs where d.Contains("@") select new DirectoryInfo(d).Name;
                    string[] totalmods = res.ToArray<string>();

                    Repositorio r = new Repositorio(this, totalmods,webrepository,carpeta_salida);

                    r.crear(carpeta_entrada, carpeta_salida);
                    */
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

        private void GenerateRepoCompleted(object sender, Repository.CatalogoCompletedEventArgs e)
        {
            Progress<PublishModsProgress> pprg = new Progress<PublishModsProgress>();

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

            re.Publish(pprg);



            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado.Content = "Finalizado";
                SubEstado.Content = "";
                progreso.Value = 0;
            }));

        }

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

                
                if (diag.ping_servidor12bdi(servidor_12bdi, 3, 100))
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
           Utiles.ejecuta_proceso("dxdiag", "/whql:on", "",true);
        }


        private void btnReloadAdmin_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Se va a reiniciar el Lanzador con permisos de Administrador", "Recargar");

            RunAsAdministrator.ValidateAdministratorModeAndRestart(false);

        }

        private void btnxpress_Click(object sender, RoutedEventArgs e)
        {
            //List<ModData> mods = new List<ModData>();

            var res = from m in mods where m.Arma == "3" select m.Name;
            string[] totalmods = res.ToArray<string>();

            xpresslauncher launcher = new xpresslauncher(tb_carpeta_arma2.Text,tb_carpeta_arma3.Text, mods);

            launcher.ShowDialog();
            //launcher.Show();
        }

        private void btn_gen_repositorio_a2_Click(object sender, RoutedEventArgs e)
        {

            string carpeta_entrada = tb_carpeta_base_arma3.Text;
            string carpeta_salida = tb_carpeta_repositorio.Text;
            string repositorio = txtRepo.Text;

            DirectoryInfo wrepo = new DirectoryInfo(carpeta_salida);

            List<string> lines = new List<string>();
            FileInfo modlist = new FileInfo(carpeta_salida + @"\modlist.txt");

            if (modlist.Exists) { modlist.Delete(); }

            foreach (DirectoryInfo i in wrepo.GetDirectories())
            {
                generateModFile(lines, "3", i);
            }


            System.IO.File.WriteAllLines(modlist.FullName, lines);
        }

        private void generateModFile(List<string> lines,string juego,DirectoryInfo mod)
        {

            var todos = from DirectoryInfo i in mod.GetDirectories() select i;

            List<string> lista = new List<string>();

            foreach (DirectoryInfo r in todos)
            {
                if (!lista.Contains(r.Name))
                {
                    lista.Add(r.Name);
                }
            }

            foreach (string l in lista)
            {
                string line = juego + "|" + l;
                lines.Add(line);
            }
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

            serv = ((Servidor)lstServidores.SelectedItem != null ? (Servidor)lstServidores.SelectedItem : (Servidor) lstServidores.Items[0]);

            string conexion = @"-connect=" + serv.IP + @" -port=" + serv.Puerto + @" -password=" + serv.Password;
            
            string lista_mods = "";
            string executable = "";
            string modp = "";
            string gfolder = "";

            if (radArma2.IsChecked.Value)
            {
                executable = "arma2oa.exe";
                gfolder = tb_carpeta_arma2.Text;

                if (!radDefaultFolder.IsChecked.Value) { modp = txtUserDefined.Text; };
            }
            else
            {
                executable = "arma3.exe";
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
            string lista_mods = @" -mod=" + serv.ModListToString();

            System.Windows.Clipboard.SetText(conexion + lista_mods);

        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {

            logger.Info("Iniciando proceso de actualizacion");

            apagar_botones();

            this.lstServidores.IsEnabled = false;

            Estado.Content = "Actualizando Mods";

            serv = ((Servidor)lstServidores.SelectedItem != null ? (Servidor)lstServidores.SelectedItem : (Servidor)lstServidores.Items[0]);

            string carpeta_juego = "";
            string varma = radArma3.IsChecked == true ? "3" : "2";


            if (radDefaultFolder.IsChecked.Value)
            {
                if (radArma3.IsChecked.Value) { carpeta_juego = tb_carpeta_arma3.Text; }
                if (radArma2.IsChecked.Value) { carpeta_juego = tb_carpeta_arma2.Text; }
            }
            else
            {
                carpeta_juego = txtUserDefined.Text + @"/" + serv.Repository;
            }

            if (!Directory.Exists(carpeta_juego))
            {
                Directory.CreateDirectory(carpeta_juego);
            }

            // Descargamos la base de datos del servidor y repositorio indicado
            db_name = source.GetRepositoryCatalog(serv.Repository);

            // Creamos una instancia del repositorio local
            Estado.Content = "Catalogando Mods";

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
                logger.Info("Cargando el repositorio {0}", serv.Repository);
                cliente = new Repository(carpeta_juego, bay, serv.Repository, mods);

                logger.Info("Asignando manejadores de evento");

                cliente.CatalogoCompleted += new Repository.CatalogoCompletedEventHandler(catalogcomplete);
                cliente.CatalogoCompareCompleted += new Repository.CatalogoCompareCompletedEventHandler(compareCompleted);
                cliente.UpgradeRepositoryProgressChanged += new Repository.UpgradeRepositoryProgressChangedEventHandler(updateTasks);
                cliente.UpgradeRepositoryBeforeExecute += new Repository.UpgradeRepositoryBeforeExecuteEventHandler(updateTasks);
                cliente.PlanProgressChanged += new Repository.PlanProgressChangedEventHandler(progresoPlan);
                cliente.UpgradeRepositoryCompleted += new Repository.UpgradeRepositoryCompletedEventHandler(tareasCompletadas);
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

                cliente.CatalogFolderAsync(bay, serv.Repository, prg);
            }
            catch (Exception ex)
            {
                logger.Fatal("Se ha producido una excepcion al catalogar addons {0}", ex.Message);

                throw ex;
            }
        }

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

            cliente.CompareCatalogs(db_name, serv.Repository, source.Address);


        }

        
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
        

        private void progresoPlan(object sender, Repository.PlanProgressEventArgs e)
        {

            this.Dispatcher.Invoke(new System.Action(() =>
            {
                Estado.Content = "Tarea " + e.Current.ToString() + " de " + e.Total.ToString();
            }));

        }

        private void updateTasks(object sender, Repository.TaskProgressProgressChanged e)
        {

            try
            {
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


        private void DetallesActualizacion_Click(object sender, RoutedEventArgs e)
        {
            DownloadProgress dp = new DownloadProgress(cliente);
            dp.Show();
        }

        private void radArma3_Checked(object sender, RoutedEventArgs e)
        {
            lstServidores.ItemsSource = from s in source.Servidores where s.Arma == "3" select s;
        }

        private void radArma2_Checked(object sender, RoutedEventArgs e)
        {
            lstServidores.ItemsSource = from s in source.Servidores where s.Arma == "2" select s;
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
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
    }
}