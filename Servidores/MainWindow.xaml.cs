/*
 * Creado por SharpDevelop.
 * Usuario: Juan
 * Fecha: 18/11/2015
 * Hora: 11:15
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using ServidoresData;
using System.IO;

namespace Servidores
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        ServidoresList servers;
        ServerConfig scon;
        List<Repository> repos;
        Servidor s;
        
		public MainWindow()
		{
            InitializeComponent();

            servers = new ServidoresList();

            /* Mashup
            servers.Servidores.Add(new Servidor() { Nombre = "Dediserver", Arma="3"});
            servers.Servidores.Add(new Servidor() { Nombre = "Operaciones", Arma="2" });
            servers.Servidores.Add(new Servidor() { Nombre = "Vietnam",Arma="2" });
            */  

            this.DataContext = servers;

            scon = new ServerConfig();
            loadConfig();
            repos = new List<Repository>();

            loadModFolder();

            //mods = repos.Where(repo => repo.Version == 3).SelectMany(repo => repo.ModList).ToList<Mod>();
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


        private void NuevoSrv_Click(object sender, RoutedEventArgs e)
        {
           	s = new Servidor(scon);

            ServidorView view = new ServidorView(s,repos);

            if (view.ShowDialog() == true)
            {
                servers.Servidores.Add(s);
            }
        }

        private void Publicar_Click(object sender, RoutedEventArgs e)
        {
        	int v;
        	Int32.TryParse(s.Arma,out v);

            s.PublishedAt = scon.RepoBasePath + @"\" + @"arma3\dediserver";

        	Repository r = new Repository(s);

            r.CreateDB();

            //r.RelativePath = @"arma3\dediserver";

        	//r.ModList = s.ModList;

            r.Publish();

        	
        }
        
        private void UpdateButtons(object sender, RoutedEventArgs e)
        {
        	s = (Servidor) sview.SelectedItem;
        	
        	PublicarSrv.IsEnabled = s.ModList.Count > 0;
        	EliminarSrv.IsEnabled = s.PublishedAt.Length > 0;
            EditarSrv.IsEnabled = true;
        }

        private void Configuracion_Click(object sender, RoutedEventArgs e)
        {
            Configuracion c = new Configuracion(scon);

            c.ShowDialog();
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (File.Exists("config.txt"))
            {
                File.Delete("config.txt");
            }

            string[] l = new string[1];

            l[0] = scon.Path + "|" + scon.ModPath + "|" + scon.ConfigFile + "|" + scon.ProfileFile + "|" + scon.ServerConfigFile + "|" + scon.RepoBasePath;

            System.IO.File.WriteAllLines("config.txt", l);
        }

        private void EditarSrv_Click(object sender, RoutedEventArgs e)
        {
            Servidor copy = s.Clone();

            ServidorView view = new ServidorView(s, repos);

            if (view.ShowDialog() == false)
            {
                s = copy;
            }
        }
	}
}