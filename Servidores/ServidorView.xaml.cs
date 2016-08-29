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
using System.Windows.Shapes;
using ServidoresData;

namespace Servidores
{
    /// <summary>
    /// Lógica de interacción para ServidorView.xaml
    /// </summary>
    public partial class ServidorView : Window
    {
        Servidor ser;
        List<Repository> repoList;

        public ServidorView(Servidor s, List<Repository> repos)
        {
            InitializeComponent();

            repoList = repos;
            ser = s;
            this.DataContext = ser;
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (rdArma2.IsChecked == true)
            {
                ser.Arma = "2";
            }
            else
            {
                ser.Arma = "3";
            }

            DialogResult = true;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddMod_Click(object sender, RoutedEventArgs e)
        {

            List<Mod> filtered = createRepo((rdArma2.IsChecked == true ? 2 : 3));

            ModList mod = new ModList(ser, filtered);

            mod.ShowDialog();

        }

        private List<Mod> createRepo(int version)
        {
            return repoList.Where(repo => repo.Version == version).SelectMany(repo => repo.ModList).ToList<Mod>();

        }

    }
}
