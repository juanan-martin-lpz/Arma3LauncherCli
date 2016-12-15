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
using System.ComponentModel;

namespace ActualizaBDD
{
    /// <summary>
    /// Interaction logic for DownloadProgress.xaml
    /// </summary>
    public partial class DownloadProgress : Window
    {
        ObservableCollection<CommandBase> _list;
        Repository repo;  

        

        public DownloadProgress(Repository r)
        {
            InitializeComponent();

           
           
            _list = new ObservableCollection<CommandBase>((from CommandBase c in r.AllTasks where c.Progreso < 100 orderby c.Progreso descending select c).ToList<CommandBase>());

            listView.ItemsSource = _list;

            repo = r;


            listView.ItemsSource = from d in _list select d;

            listView.Items.MoveCurrentToFirst();

            r.PlanProgressChanged += (se, ev) =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.Title = "Progreso de Descarga (" + ev.Current + @"/" + repo.AllTasks.Count.ToString() + ")";
                }));
            };

            r.UpgradeRepositoryProgressChanged += (s, e) =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    /*
                    listView.Items.MoveCurrentTo(e.UserState);

                    CommandBase b = (CommandBase)listView.SelectedItem;

                    if (b != null)
                    {
                        b.Progreso = e.ProgressPercentage;
                    }
                    */

                    _list = new ObservableCollection<CommandBase>((from CommandBase c in r.AllTasks where c.Progreso < 100 orderby c.Progreso descending select c).ToList<CommandBase>());

                    listView.ItemsSource = _list;

                    ICollectionView view = CollectionViewSource.GetDefaultView(listView.ItemsSource);
                    view.Refresh();
                }));
            };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
