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
using System.Collections.ObjectModel;

namespace Servidores
{
    /// <summary>
    /// Lógica de interacción para ModList.xaml
    /// </summary>
    public partial class ModList : Window
    {
        Servidor ser;
        List<Mod> repoList;
        ObservableCollection<ListBoxModItem> lista;

        public ModList(Servidor s, List<Mod> repos)
        {
            InitializeComponent();
            this.Topmost = true;

            ser = s;
            repoList = repos;
            lista = new ObservableCollection<ListBoxModItem>();

            foreach(Mod m in repoList)
            {
                lista.Add(new ListBoxModItem() { IsChecked = ser.ModList.Contains(m), Mod = m });
            }

            this.DataContext = lista;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ObservableCollection<ListBoxModItem> items = (ObservableCollection<ListBoxModItem>) this.DataContext;

            ser.ModList.Clear();

            foreach(ListBoxModItem l in items)
            {
                if (l.IsChecked)
                {
                    ser.ModList.Add(l.Mod);
                }
            }
        }
    }
}
