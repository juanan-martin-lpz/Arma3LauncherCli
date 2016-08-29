using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ServidoresData;
using System.IO;

namespace Servidores
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow1 : Window
    {

        ObservableCollection<Servidor> data;

        ServidoresList servers;

        Servidor s;

        ObservableCollection<Repository> repos;

        List<Mod> mods;

        ServerConfig scon;

        public MainWindow1()
        {
            InitializeComponent();

            scon = new ServerConfig();

            loadConfig();

            repos = new ObservableCollection<Repository>();

            mods = new List<Mod>();

            servers = new ServidoresList();

            this.DataContext = s;
            
            lstServidores.ItemsSource = servers.Servidores;


            loadModFolder();

            mods = repos.Where(repo => repo.Version == 3).SelectMany(repo => repo.ModList).ToList<Mod>();

            lstMods.DataContext = mods;
        }


        private void loadModFolder()
        {
            string a2folder = scon.ModPath + @"arma2";
            string a3folder = scon.ModPath + @"arma3";

            repos.Add(Repository.FromFolder(a2folder, 2));
            repos.Add(Repository.FromFolder(a3folder, 3));


        }


        private void loadConfig()
        {
            if (File.Exists("config.txt"))
            {
                string[] l = File.ReadAllLines("config.txt");

                string[] items = l[0].Split('|');

                scon.Path = items[0];
                scon.ModPath = items[1];
                scon.ConfigFile = items[2];
                scon.ProfileFile = items[3];
                scon.ServerConfigFile = items[4];
                scon.RepoBasePath = items[5];

            }
            else
            {
                string basePath = @"C:\Arma\";

                scon.Path = basePath + @"arma3server.exe";
                scon.ModPath = @"C:\Juegos\Mods\";
                scon.ConfigFile = basePath + @"Perfiles\server.cfg";
                scon.ProfileFile = basePath + @"Perfiles\12bdi.cfg";
                scon.ServerConfigFile = basePath + @"Perfiles\server_config.cfg";
                scon.RepoBasePath = @"C:\Juegos\12bdi_launcher\Mods";

            }
        }


        #region "Visual"
        private void habilitarEdicion()
        {
            cargar.IsEnabled = false;
            nuevo.IsEnabled = false;
            guardar.IsEnabled = false;
            //borrar.IsEnabled = false;
            //lstServidores.IsEnabled = false;
            datos.IsEnabled = true;
            txtnombre.Focus();
        }

        private void deshabilitarModificacion()
        {
            txtnombre.IsEnabled = false;
            //txtPuerto.IsEnabled = false;
            cmbArma2.IsEnabled = false;
            cmbArma3.IsEnabled = false;

        }

        private void habilitarModificacion()
        {
            txtnombre.IsEnabled = true;
            //txtPuerto.IsEnabled = true;
            cmbArma2.IsEnabled = true;
            cmbArma3.IsEnabled = true;

        }

        private void deshabilitarEdicion()
        {
            cargar.IsEnabled = true;
            nuevo.IsEnabled = true;
            guardar.IsEnabled = true;
            //borrar.IsEnabled = true;
            //lstServidores.IsEnabled = true;
            datos.IsEnabled = false;
        }


        #endregion

        #region "Eventos"
        private void nuevo_Click(object sender, RoutedEventArgs e)
        {
            s = new Servidor(scon);


            //createRepo(3);

            cmbArma3.IsChecked = true;

            datos.DataContext = s;
            habilitarEdicion();
            habilitarModificacion();
        }


        private void createRepo(int version)
        {
            mods = repos.Where(repo => repo.Version == version).SelectMany(repo => repo.ModList).ToList<Mod>();

            lstMods.DataContext = mods;

            /*
            s.ModList.Clear();

            foreach (Mod m in mods)
            {
                s.ModList.Add(m.Clone());
            }
             */
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {

            deshabilitarEdicion();


            
            
            
            //var selected = from m in mods where m.Selected select m;


            //s.ModList = selected.ToList<Mod>();

            //var lista = lstMods.DataContext;

            //s.ModList = new ObservableCollection<Mod>(lista);

            if (!servers.Servidores.Contains(s)) 
            {
                servers.Servidores.Add(s);
            }


        }

        private void batCancelar_Click(object sender, RoutedEventArgs e)
        {
            //if (txtnombre.IsEnabled) { servers.Servidores.Remove(s); }
            
            deshabilitarEdicion();
        }



        private void lstServidores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            s = (Servidor) lstServidores.SelectedItem;

            datos.DataContext = s;

            //lstMods.ItemsSource = s.ModList;


            if (s.Arma == "2")
            {
                this.cmbArma2.IsChecked = true;
            }
            else
            {
                this.cmbArma3.IsChecked = true;
            }

            habilitarEdicion();
            deshabilitarModificacion();
            
        }

        private void cmbArma2_Checked(object sender, RoutedEventArgs e)
        {
            if (txtnombre.IsEnabled)
            {
                //createRepo(2);
                s.Arma = "2";

            }
            else
            {
                //lstMods.ItemsSource = s.ModList;
            }
        }

        private void cmbArma3_Checked(object sender, RoutedEventArgs e)
        {
            if (txtnombre.IsEnabled)
            {
                //createRepo(3);
                s.Arma = "3";
            }
            else
            {
                //lstMods.ItemsSource = s.ModList;
            }


        }


        private void guardar_Click(object sender, RoutedEventArgs e)
        {
            string fpath = @"C:\JUEGOS\Mods\servidores.txt";
            char separator = '|';

            if (File.Exists(fpath))
            {
                File.Delete(fpath);
            }

            try
            {
                using (var f = new StreamWriter(fpath))
                {
                    foreach(Servidor ser in data)
                    {

                        string mods = ser.ModListToString().TrimEnd(';');

                        f.WriteLine(ser.Arma + separator + ser.Nombre + separator + ser.Puerto + separator + mods);


                    }
                }
            }
            catch
            {

            }
        }

        private void borrar_Click(object sender, RoutedEventArgs e)
        {
            
            data.Remove(s);

            deshabilitarEdicion();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            Configuracion c = new Configuracion(scon);

            c.ShowDialog();

        }

        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (File.Exists("config.txt"))
            {
                File.Delete("config.txt");
            }

            string[] l = new string[1];

            l[0] = scon.Path + "|" + scon.ModPath + "|" + scon.ConfigFile + "|" + scon.ProfileFile + "|" + scon.ServerConfigFile + "|" + scon.RepoBasePath;
            
            System.IO.File.WriteAllLines("config.txt",l);

        }

    }
}
