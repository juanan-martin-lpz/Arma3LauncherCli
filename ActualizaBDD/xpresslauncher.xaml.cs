using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;
using ServidoresData;

namespace ActualizaBDD
{
    /// <summary>
    /// Lógica de interacción para xpresslauncher.xaml
    /// </summary>
    public partial class xpresslauncher : Window
    {
        List<ModView> allMods;
        string Arma2Path;
        string Arma3Path;

        public xpresslauncher(string arma2path, string arma3path,List<ModView> mods)
        {
            InitializeComponent();
            allMods = mods;

            Arma2Path = arma2path;
            Arma3Path = arma3path;


            selectArma3();
                
        }


        private void selectArma3()
        {
            if (allMods == null) { return; }

            var arma3 = allMods.Where(x => x.Arma == "3");

            lista.ItemsSource = arma3;
        }


        private void selectArma2()
        {
            if (allMods == null) { return; }

            var arma2 = allMods.Where(x => x.Arma == "2");

            lista.ItemsSource = arma2;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string comando;
            string parametros = " ";
            string path;


            if (a2.IsChecked == true) 
            { 
                comando = "arma2.exe"; 
                path = Arma2Path;
            } 
            else 
            {
                comando = "arma3.exe";
                path = Arma3Path;
            }

            if (ht.IsChecked == true) { parametros += " -enableHT";}
            if (threads.IsChecked == true) { parametros += " -exThreads=" + tcount.Text + " "; }
            if (malloc.SelectedIndex > 0) { parametros += "-malloc=" + malloc.Text + " "; }
            if (winxp.IsChecked == true) { parametros += " -winxp"; }



            int maxmem;

            try
            {
                if (Int32.TryParse(txtMaxmem.Text, out maxmem) == true)
                {
                    if (maxmem > 2047) { maxmem = 2047; }

                    if (maxmem > 0) { parametros += "-maxMem=" + maxmem.ToString(); }
                }
            }
            catch
            {

            }

            var list = lista.ItemsSource;

            string smods = "";

            foreach(ModData m in list)
            {
                if (m.Selected == true) { smods += m.Mod + ";";}
            }

            //if (smods.Length > 1) { parametros += " -mods=" + smods;}

            parametros += " -mod=" + smods + " -skipintro -world=empty";

            txtExec.Text = comando + parametros;

            arranca_juego(path,comando,parametros);

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            selectArma3();

        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            selectArma2();
        }


        private void arranca_juego(string carpeta, string ejecutable, string parametros)
        {
            //apagar_botones();

            //log("Arrancando juego: ");
            //log("Parametros : " + parametros);

            // Proceso principal
            Task t1 = new Task(
            () =>
            {
                try
                {
                    //voidStringDelegate dlog = log;
                    Utiles.ejecuta_proceso(carpeta + "\\" + ejecutable, parametros, carpeta);
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
                //encender_botones();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            // Arrancamos la tarea inicial
            t1.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

       
    }
}
