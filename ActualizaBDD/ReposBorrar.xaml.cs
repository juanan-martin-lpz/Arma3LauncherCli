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
using ServerManagementClient;

namespace ActualizaBDD
{
    /// <summary>
    /// Lógica de interacción para ReposBorrar.xaml
    /// </summary>
    
    public partial class ReposBorrar : Window
    {
        //List<Mod> elementos;

        
        public ReposBorrar(List<ModView> items)
        {
            InitializeComponent();

            //lista.ItemsSource = items != null ? items : new List<ModView>();


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
