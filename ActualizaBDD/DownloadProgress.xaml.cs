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
using System.Collections.ObjectModel;
using ServidoresData;

namespace ActualizaBDD
{
    /// <summary>
    /// Interaction logic for DownloadProgress.xaml
    /// </summary>
    public partial class DownloadProgress : Window
    {
        List<CommandBase> _list;
        Repository repo;  

        

        public DownloadProgress(Repository r)
        {
            InitializeComponent();

            _list = r.AllTasks;

            this.DataContext = _list;

            repo = r;

            listView.ItemsSource = from d in _list select d;

            listView.Items.MoveCurrentToFirst();

            r.UpgradeRepositoryProgressChanged += (s, e) =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {

                    listView.Items.MoveCurrentTo(e.UserState);

                    CommandBase b = (CommandBase)listView.SelectedItem;

                    if (b != null)
                    {
                        b.Progreso = e.ProgressPercentage;
                    }

                }));
            };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
