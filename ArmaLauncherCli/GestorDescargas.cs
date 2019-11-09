using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualizaBDD
{
    // ATENCIÓN: CLASE NO USADA, DE MOMENTO.

    class GestorDescargas
    {
        
        // Numero maximo de descargas simultaneas
        int maximo_descargas_simultaneas = 2;

        // Ficheros en cola para descargar
        List<Task> lista_tareas;

        // Devuelve el número de tareas activas. Limpia la lista si no queda ninguna activa
        int TareasActivas
        {
            get
            {
                int tareas_activas = 0;

                foreach (Task t in lista_tareas)
                    if (!(t.IsCompleted || t.IsFaulted || t.IsCanceled))
                        tareas_activas++;

                if (tareas_activas == 0)
                    lista_tareas.Clear();

                return tareas_activas;
            }
        }

        public GestorDescargas()
        {
            lista_tareas = new List<Task>();
        }

        // añade un fiechero a la ista de descargas
        public void añade_fichero(string servidor, string usuario, string contraseña, string fichero_remoto, string fichero_local)
        {
            Task t1 = new Task(
            () =>
            {
                try
                {
                    //voidStringDelegate dlog = log;
                    Utiles.descarga_fichero ( servidor, usuario, contraseña,  fichero_remoto, fichero_local);
                }
                catch (Exception )
                {
                    //System.Windows.MessageBox.Show(x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                }

            });

            t1.Start();

            lista_tareas.Add ( t1 );

            // Si hemos alcanzado el máximo de descargas simultáneas, retenemos el control 
            if (TareasActivas >= maximo_descargas_simultaneas)
                t1.Wait();
        }

        // Retiene el control hasta que termina toda la lista de descargas
        public void termina_descargas()
        {
            foreach (Task t in lista_tareas)
            {
                if (t.IsCompleted || t.IsFaulted ||t.IsCanceled )
                {
                    t.Wait();
                }
            }

            //Todas las tareas deberían haber terminado o fallado
            lista_tareas.Clear();
        }

        // Procesamos la lista de solicitudes
        //private void procesar_lista_tareas()
        //{
        //    int descargas_activas = 0;

        //    foreach (Task t in lista_tareas)
        //    {
        //        if (t.IsCompleted() 
        //    }

        ////    log("Arrancando juego: ");
        //    log("Carpeta : " + carpeta);
        //    log("Ejecutable : " + ejecutable);
        //    log("Parametros : " + parametros);

        //    // Proceso principal
        //    Task t1 = new Task(
        //    () =>
        //    {
        //        try
        //        {
        //            voidStringDelegate dlog = log;
        //            Utiles.ejecuta_proceso(ejecutable, parametros, carpeta);
        //        }
        //        catch (Exception x)
        //        {
        //            System.Windows.MessageBox.Show(x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
        //        }

        //    });

        //    // Cuando termine el proceso...
        //    Task t_final = t1.ContinueWith(
        //    ant =>
        //    {
        //        encender_botones();
        //    }, TaskScheduler.FromCurrentSynchronizationContext());

        //    // Arrancamos la tarea inicial
        //    t1.Start();
       // }


        //// Una solicitud de descarga
        //class Solicitud
        //{
        //    string servidor;
        //    string usuario;
        //    string contraseña;
        //    string fichero_remoto;
        //    string fichero_local;

        //    public Solicitud(string servidor, string usuario, string contraseña, string fichero_remoto, string fichero_local)
        //    {
        //        this.servidor = servidor;
        //        this.usuario = usuario;
        //        this.contraseña = contraseña;
        //        this.fichero_remoto = fichero_remoto;
        //        this.fichero_local = fichero_local;
        //    }
        //}
    }
}
