using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Data.SQLite;
using System.Security.Cryptography;

namespace ActualizaBDD
{
    class Cache
    {
        string nombre_bdd;


        // Si el fichero es más pequeño que esto, ni se busca ni se inserta la firma md5 en la base de datos.
        // Supongo que es más rápido calcular la firma md5 de un fichero de 2 bytes que andar haciendo búsquedas en una base de datos. Y además
        // eso hace que la tabla de la base de datos sea bastante más pequeña, porque saca fuera todos los fciheritos .bisign

        // De momento, el tamnaño mínimo de cache son 5kb
        int tamano_minimo = 5 * 1024;

        // es necesario inicializar la clase antes de usarla.
        bool inicializada;

        // Aciertos / Total consultas en cache, 
        public int total_aciertos;
        public int total_consultas;

        // Función de inicializacion
        public Cache()
        {
            try
            {
                total_aciertos = 0;
                total_consultas = 0;

                string ruta = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher";

                if (!Directory.Exists(ruta))
                    Directory.CreateDirectory(ruta);

                nombre_bdd = ruta + "\\cache.db";

                // Si no existe la base de datos, creamos una nueva
                if (!File.Exists(nombre_bdd))
                {
                    using (SQLiteConnection con = new SQLiteConnection("Data Source=" + nombre_bdd))
                    {
                        con.Open();

                        SQLiteCommand cmd = new SQLiteCommand();
                        cmd.Connection = con;

                        cmd.CommandText = "create table if not exists parametros (id text, valor text)";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "insert into parametros (id, valor) values ('fecha_creacion', '" + DateTime.Now.ToString() + "')";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "create table if not exists ficheros (fichero text, tamano unsigned bigint, fecha_modificacion text, firma)";
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Hacemos una prueba de conexión
                    using (SQLiteConnection con = new SQLiteConnection("Data Source=" + nombre_bdd))
                    {
                        con.Open();
                    }
                }

                inicializada = true;
            }
            catch (Exception)
            {
                MessageBox.Show("No se ha podido inicializar la caché de firmas. El programa continuará, pero será mucho más lento.", "12BDI Lanzador", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                inicializada = false;
            }
        }

        // Esta función se usa para comprobar las firmas MD5 de los ficheros. 
        // Si el fichero no alcanza el tamaño mínimo, se calcula la firma sin más
        // Si alcanza un tamaño mínimo, se consulta la base de datos.
        // Si hay un registro con el mismo nombre, fecha de modificación y tamaño, se usa el valor md5 obtenido.
        // Si no hay un registro, se clacula el valor MD5 y se almacena.
        // Si se encuentra ma´s de un registro, se borran todos y se calcula de de nuevo.
        public string firma_md5(string fichero)
        {
            try
            {
                total_consultas++;

                string firma = "";

                FileInfo fi = new FileInfo(fichero);

                if (inicializada && fi.Length >= tamano_minimo)
                {

                    using (SQLiteConnection con = new SQLiteConnection("Data Source=" + nombre_bdd))
                    {
                        con.Open();

                        using (SQLiteCommand cmd = new SQLiteCommand("select firma from ficheros where fichero ='" + fichero + "' and tamano = " + fi.Length + " and fecha_modificacion = '" + fi.LastWriteTime + "'", con))
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {

                            if (dr.Read()) // Tenemos al menos una coincidencia.
                            {
                                total_aciertos++;
                                firma = dr.GetString(0);
                            }

                            if (dr.Read()) // Solo puede haber un resultado
                            {
                                dr.Close();
                                cmd.CommandText = "delete from ficheros where fichero ='" + fichero + "' and tamano = " + fi.Length + " and fecha_modificacion = '" + fi.LastWriteTime + "'";
                                cmd.ExecuteNonQuery();
                                firma = "";
                                total_aciertos--;
                            }
                            else
                                dr.Close();

                            if (firma == "")  // No hay ninguna coincidencia. calculamos la firma y almacenamos el resultado en cache para la proxima vez
                            {
                                firma = Utiles.calcula_firma_MD5(fichero);
                                cmd.CommandText = "insert into ficheros (fichero, tamano, fecha_modificacion, firma) values ('" + fichero + "', " + fi.Length + ", '" + fi.LastWriteTime + "', '" + firma + "')";
                                cmd.ExecuteNonQuery();
                            }

                        } // cmd y dr
                    } // con
                }

                if (firma == "")
                    firma = Utiles.calcula_firma_MD5(fichero);

                return firma;

            }
            catch (Exception)
            {
                throw;
            }
        }

        // DEstruye el fichero de caché, si existe.
        public void destruye()
        {
            if (File.Exists(nombre_bdd))
                File.Delete(nombre_bdd);
        }


    }
}
