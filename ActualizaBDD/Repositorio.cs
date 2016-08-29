using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data.SQLite;
using System.IO;
using System.Windows.Threading;
using System.Security.Cryptography;
using System.Data;
using System.Threading.Tasks;
using System.Threading;


namespace ActualizaBDD
{

    class Repositorio
    {
        // Datable
        DataTable cliente;
        DataSet ds;


        Data dcliente;
        Data dservidor;


        Data catalogoSrv;

        string ce;

        // Bombre de la bbdd con los ficheros del servidor.
        string nombre_bdd;

        // Por seguridad sólo vamos a procesar las carpetas de mods oficiales.
        string[] lista_mods;

        MainWindow w;

        long ficheros_a_descargar = 0;
        long ficheros_a_borrar = 0;
        bool errores_comunicacion = false;

        DownloadList dlist;
        string _url;
        string _target;

        SemaphoreSlim s;

        public Repositorio(MainWindow w, string[] lista_mods, string url, string target)
        {
            this.lista_mods = lista_mods;
            this.w = w;
            nombre_bdd = "";

            dlist = new DownloadList();
            _url = url;
            _target = target;

            s = new SemaphoreSlim(6);
            

        }


        private void crearDatasetDesdeCarpeta(string mod,string carpeta)
        {
            string[] lista_ficheros = Directory.GetFiles(carpeta, "*.*", SearchOption.AllDirectories);

            foreach (string f in lista_ficheros)
            {
                string nombre = Path.GetFileName(f);
                
                string ruta = Path.GetDirectoryName(f).Replace(ce, "");

                if (nombre_valido(ruta) && nombre_valido(nombre))
                {
                    string firma_base64 = Utiles.calcula_firma_MD5(f);
                    FileInfo fi = new FileInfo(f);

                    catalogoSrv.insert(mod, nombre, ruta, firma_base64, fi.Length);
                }
            }
        }

        // Para crear un repositorio, copiamos los ficheros y creamos una base de datos nueva. Esta función sólo se ejecuta en el servidor
        public void crear(string carpeta_entrada, string carpeta_salida)
        {

            string config_mods_file;

            ce = carpeta_entrada;

            try
            {
                // Eliminamos todos los contenidos del directorio de salida, y volvemos a crear la bdd.
                if (Directory.Exists(carpeta_salida)) { Directory.Delete(carpeta_salida, true); }

                Directory.CreateDirectory(carpeta_salida);

                nombre_bdd = carpeta_salida + "\\ficheros.db";
                //config_mods_file = carpeta_salida + "\\mods_activos.txt";
                

                if (File.Exists(nombre_bdd))
                    File.Delete(nombre_bdd);

                //if (File.Exists(config_mods_file))
                //    File.Delete(config_mods_file);

                //FileStream fmods = File.Create(config_mods_file);

                // generamos la base de datos y hacemos la copia de ficheros.

                catalogoSrv = new Data(nombre_bdd, "ficheros_servidor");
                //Data dser = new Data(nombre_bdd, "ficheros_servidor");

                foreach (string mod in lista_mods)
                {
                    log("Procesando " + mod);

                    // Si el mod no existe (porque ha sido borrado), continuamos
                    if (!Directory.Exists(carpeta_entrada + "\\" + mod)) continue;

                    string carpeta = Path.Combine(carpeta_entrada, mod);

                    crearDatasetDesdeCarpeta(mod, carpeta);


                }

                catalogoSrv.writeDB();

                int nr = catalogoSrv.Table.Rows.Count;

                log("Finalizado");
                log("Detectados " + nr.ToString() + " archivos");
                log("Copiando...");

                int n = 0;

                foreach (DataRow r in catalogoSrv.Table.Rows)
                {
                    n++;

                    string ruta_destino = carpeta_salida + "\\" + r.Field<string>("ruta").ToString();

                    Directory.CreateDirectory(ruta_destino);

                    string f = carpeta_entrada + "\\" + r.Field<string>("ruta").ToString() + "\\" + r.Field<string>("nombre").ToString();

                    log_progreso("Copiando mod " + r.Field<string>("mod").ToString() + " - Fichero " + n + "/" + nr);

                    File.Copy(f, ruta_destino + "\\" + r.Field<string>("nombre").ToString());
                }

                #region "Old Code"

                //List<string> lista_carpetas = new List<string>(Directory.GetDirectories(carpeta_entrada + "\\" + mod, "*.*", SearchOption.AllDirectories));
                    //lista_carpetas.Add(carpeta_entrada + "\\" + mod);

                    //foreach (string carpeta in lista_carpetas)
                    //{

                        

                        /*
                        string[] lista_ficheros = Directory.GetFiles(carpeta, "*.*", SearchOption.TopDirectoryOnly);

                        foreach (string fichero in lista_ficheros)
                        {
                            string nombre = Path.GetFileName(fichero);
                            string ruta = Path.GetDirectoryName(fichero).Replace(carpeta_entrada, "");

                            // calculamos la firma y el tamaño

                            string firma_base64 = Utiles.calcula_firma_MD5(fichero);
                            FileInfo fi = new FileInfo(fichero);

                            // insertamos en la base de datos, ignorando las que tengan comillas
                            if (nombre_valido(nombre) && nombre_valido(ruta))
                            {
                                dser.insert(mod,nombre, ruta, firma_base64, fi.Length);
                                //cmd.CommandText = "insert into ficheros_servidor (ruta, nombre, firma, tamano) values ('" + ruta + "', '" + nombre + "', '" + firma_base64 + "', " + fi.Length + ") ";
                                //cmd.ExecuteNonQuery();

                                // Comprimimos y copiamos . 

                                string ruta_destino = carpeta_salida + ruta;
                                Directory.CreateDirectory(ruta_destino);

                                Task t = new Task(() => { File.Copy(fichero, ruta_destino + "\\" + nombre); });

                                t.Start();

                            }

                            // NOTA: Por lo que estoy viendo, la compresión no ayuda mucho porque los ficheros ya viene  comprimidos, y son un quebradero de cabeza a la hora de 
                            // actualizar. El código de abajo funciona, pero lo dejo comprimido.

                            //// Si el fichero es mayor de 5 megas lo comprimimos
                            //if (fi.Length > 5 * 1024 * 1024)
                            //{
                            //    Utiles.comprimeFichero(ruta_destino + "\\" + nombre);
                            //    File.Delete(ruta_destino + "\\" + nombre);
                            //}
                        }
                        */
                //}
                #endregion




                #region "Old code"
                /*
                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + nombre_bdd))
                {
                    con.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {

                        cmd.Connection = con;

                        cmd.CommandText = "create table if not exists ficheros_servidor (ruta text, nombre text, firma, tamano unsigned bigint)";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "create table if not exists ficheros_cliente (ruta text, nombre text, firma, tamano unsigned bigint)";
                        cmd.ExecuteNonQuery();

                        foreach (string mod in lista_mods)
                        {
                            log("Procesando " + mod);

                            // Si el mod no existe (porque ha sido borrado), continuamos
                            if (!Directory.Exists(carpeta_entrada + "\\" + mod))
                                continue;

                            List<string> lista_carpetas = new List<string>(Directory.GetDirectories(carpeta_entrada + "\\" + mod, "*.*", SearchOption.AllDirectories));
                            lista_carpetas.Add(carpeta_entrada + "\\" + mod);

                            foreach (string carpeta in lista_carpetas)
                            {
                                string[] lista_ficheros = Directory.GetFiles(carpeta, "*.*", SearchOption.TopDirectoryOnly);

                                foreach (string fichero in lista_ficheros)
                                {
                                    string nombre = Path.GetFileName(fichero);
                                    string ruta = Path.GetDirectoryName(fichero).Replace(carpeta_entrada, "");

                                    // calculamos la firma y el tamaño

                                    string firma_base64 = Utiles.calcula_firma_MD5(fichero);
                                    FileInfo fi = new FileInfo(fichero);

                                    // insertamos en la base de datos, ignorando las que tengan comillas
                                    if (nombre_valido(nombre) && nombre_valido(ruta))
                                    {
                                        cmd.CommandText = "insert into ficheros_servidor (ruta, nombre, firma, tamano) values ('" + ruta + "', '" + nombre + "', '" + firma_base64 + "', " + fi.Length + ") ";
                                        cmd.ExecuteNonQuery();

                                        // Comprimimos y copiamos . 

                                        string ruta_destino = carpeta_salida + ruta;
                                        Directory.CreateDirectory(ruta_destino);
                                        File.Copy(fichero, ruta_destino + "\\" + nombre);
                                    }

                                    // NOTA: Por lo que estoy viendo, la compresión no ayuda mucho porque los ficheros ya viene  comprimidos, y son un quebradero de cabeza a la hora de 
                                    // actualizar. El código de abajo funciona, pero lo dejo comprimido.

                                    //// Si el fichero es mayor de 5 megas lo comprimimos
                                    //if (fi.Length > 5 * 1024 * 1024)
                                    //{
                                    //    Utiles.comprimeFichero(ruta_destino + "\\" + nombre);
                                    //    File.Delete(ruta_destino + "\\" + nombre);
                                    //}
                                }
                            }

                        }
                    }

                    con.Close();
                } */
                #endregion
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        // Las siguientes tres funciones se usan para actualizar los ficheros del cliente

        internal string descargar_bbdd(string wrepository, string repo)
        {
            try
            {
                log("Descargando catálogo de addons del servidor...");

                nombre_bdd = Path.GetTempPath() + "ficheros_" + DateTime.Now.Ticks + ".db";

                WebDownload wd = new WebDownload();

                wd.DownloadFileAsync(wrepository, repo + "/" + "ficheros.db", nombre_bdd);

                
                //File.Copy(@"C:\bdi\arma3\ficheros.db", nombre_bdd);

                dcliente = new Data(nombre_bdd, "ficheros_cliente");
                dservidor = new Data(nombre_bdd, "ficheros_servidor");

                return nombre_bdd;
            }
            catch (Exception x)
            {
                MessageBox.Show("No se pudo descargar el catálogo del servidor.", "12BDI Lanzador", MessageBoxButton.OK, MessageBoxImage.Error);
                throw x;
            }
        }


        internal string descargar_bbddS(string wrepository, string repo)
        {
            try
            {
                log("Descargando catálogo de addons del servidor...");

                nombre_bdd = Path.GetTempPath() + "ficheros_" + DateTime.Now.Ticks + ".db";

                WebDownload wd = new WebDownload();

                wd.DownloadFileSync(wrepository, repo + "/" + "ficheros.db", nombre_bdd);


                //File.Copy(@"C:\bdi\arma3\ficheros.db", nombre_bdd);

                dcliente = new Data(nombre_bdd, "ficheros_cliente");
                dservidor = new Data(nombre_bdd, "ficheros_servidor");

                return nombre_bdd;
            }
            catch (Exception x)
            {
                MessageBox.Show("No se pudo descargar el catálogo del servidor.", "12BDI Lanzador", MessageBoxButton.OK, MessageBoxImage.Error);
                throw x;
            }
        }
        // Esta función construye llena la tabla ficheros_cliente con los ficheros del cliente
        internal void comprobar(string nombre_bdd, string carpeta_juego, string servidor_ftp, string usuario_ftp, string contraseña_ftp)
        {
            comprobar(nombre_bdd, carpeta_juego, servidor_ftp, usuario_ftp, contraseña_ftp, false);
        }


        // Crea el catalogo del lado cliente
        public void crearcatalogo(string nombre_bdd, string carpeta_juego)
        {
            try
            {
                Cache cache = new Cache();

                int i = 0;
                foreach (string mod in lista_mods)
                {
                    i++;
                    log_progreso("Catalogando addons instalados paso " + i + "/" + lista_mods.Length + " - " + mod);

                    // Si la carpeta no existe, continuamos. Evidentemente no vamos a encontrar ficheros dentro
                    //if (!Directory.Exists(carpeta_juego + "\\" + mod))
                    //    continue;

                    List<string> lista_carpetas;

                    try
                    {
                        lista_carpetas = new List<string>(Directory.GetDirectories(carpeta_juego + "\\" + mod, "*.*", SearchOption.AllDirectories));
                        lista_carpetas.Add(carpeta_juego + "\\" + mod);
                    }
                    catch (Exception e)
                    {
                        lista_carpetas = new List<string>();
                    }
                    
                    

                    foreach (string carpeta in lista_carpetas)
                    {
                        string[] lista_ficheros = Directory.GetFiles(carpeta, "*.*", SearchOption.TopDirectoryOnly);

                        foreach (string fichero in lista_ficheros)
                        {
                            string nombre = Path.GetFileName(fichero);
                            string ruta = Path.GetDirectoryName(fichero).Replace(Path.GetFullPath(carpeta_juego), "");


                            // insertamos en la base de datos, ignorando los nombres que tengan comillas.
                            if (nombre_valido(nombre) && nombre_valido(ruta))
                            {
                                // calculamos la firma
                                string firma_base64 = cache.firma_md5(fichero);
                                FileInfo fi = new FileInfo(fichero);

                                dcliente.insert(mod, nombre, ruta, firma_base64, fi.Length);
                                
                            }
                        }
                    }

                   
                    dcliente.writeDB();

                    log(" - Cache : " + cache.total_aciertos.ToString() + " / " + cache.total_consultas.ToString() + " - OK.");
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("No se pudieron procesar los ficheros del cliente: " + x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                throw x;
            }
        }


        public DownloadList GetDownloadListFromCatalago()
        {
      
            return dlist;
        }

        // Obsoleta ????
        internal void comprobar(string nombre_bdd, string carpeta_juego, string servidor_ftp, string usuario_ftp, string contraseña_ftp, bool pausa)
        {
            try
            {
                Cache cache = new Cache();

                int i = 0;
                foreach (string mod in lista_mods)
                {
                    i++;
                    log_progreso("Catalogando addons instalados paso " + i + "/" + lista_mods.Length + " - " + mod);

                    // Si la carpeta no existe, continuamos. Evidentemente no vamos a encontrar ficheros dentro
                    if (!Directory.Exists(carpeta_juego + "\\" + mod))
                        continue;

                    List<string> lista_carpetas = new List<string>(Directory.GetDirectories(carpeta_juego + "\\" + mod, "*.*", SearchOption.AllDirectories));
                    lista_carpetas.Add(carpeta_juego + "\\" + mod);

                    foreach (string carpeta in lista_carpetas)
                    {
                        string[] lista_ficheros = Directory.GetFiles(carpeta, "*.*", SearchOption.TopDirectoryOnly);

                        foreach (string fichero in lista_ficheros)
                        {
                            string nombre = Path.GetFileName(fichero);
                            string ruta = Path.GetDirectoryName(fichero).Replace(carpeta_juego, "");

                            // insertamos en la base de datos, ignorando los nombres que tengan comillas.
                            if (nombre_valido(nombre) && nombre_valido(ruta))
                            {
                                // calculamos la firma
                               
                                string firma_base64 = cache.firma_md5(fichero);
                                FileInfo fi = new FileInfo(fichero);

                                dcliente.insert(mod, nombre, ruta, firma_base64, fi.Length);
                      
                                
                            }
                        }
                        
                    }

                    if (pausa)
                        MessageBox.Show("Haciendo una pausa, pulsa Aceptar cuando pasen unos segundos.", "12BDI Lanzador", MessageBoxButton.OK);

                    dcliente.writeDB();

                    log(" - Cache : " + cache.total_aciertos.ToString() + " / " + cache.total_consultas.ToString() + " - OK.");
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("No se pudieron procesar los ficheros del cliente: " + x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                throw x;
            }
        
                /*

                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + nombre_bdd))
                {
                    // inicializamos cache de firmas MD5
                    Cache cache = new Cache();

                    Data dcliente = new Data(nombre_bdd,"ficheros_cliente");

                    //con.Open();

                    //using (SQLiteCommand cmd = new SQLiteCommand(con))
                    //{

                        // Por si acaso, limpiamos la tabla
                        //cmd.CommandText = "delete from  ficheros_cliente ";
                        //cmd.ExecuteNonQuery();

                    //NewRepo nr = new NewRepo();

                    //nr.index(carpeta_juego,@"@");

                        int i = 0;
                        foreach (string mod in lista_mods)
                        {
                            i++;
                            log_progreso("Catalogando addons instalados paso " + i + "/" + lista_mods.Length+ " - "+mod);

                            // Si la carpeta no existe, continuamos. Evidentemente no vamos a encontrar ficheros dentro
                            if (!Directory.Exists(carpeta_juego + "\\" + mod))
                                continue;

                            List<string> lista_carpetas = new List<string>(Directory.GetDirectories(carpeta_juego + "\\" + mod, "*.*", SearchOption.AllDirectories));
                            lista_carpetas.Add(carpeta_juego + "\\" + mod);

                            foreach (string carpeta in lista_carpetas)
                            {
                                string[] lista_ficheros = Directory.GetFiles(carpeta, "*.*", SearchOption.TopDirectoryOnly);

                                foreach (string fichero in lista_ficheros)
                                {
                                    string nombre = Path.GetFileName(fichero);
                                    string ruta = Path.GetDirectoryName(fichero).Replace(carpeta_juego, "");

                                    // insertamos en la base de datos, ignorando los nombres que tengan comillas.
                                    if (nombre_valido(nombre) && nombre_valido(ruta))
                                    {
                                        // calculamos la firma
                                        string firma_base64 = cache.firma_md5(fichero);
                                        FileInfo fi = new FileInfo(fichero);

                                        //DataRow row = cliente.NewRow();

                                        //row["ruta"] = ruta;
                                        //row["nombre"] = nombre;
                                        //row["firma"] = firma_base64;
                                        //row["tamano"] = fi.Length;

                                        //cliente.Rows.Add(row);

                                        dcliente.insert(nombre, ruta, firma_base64, fi.Length);


                                        //cmd.CommandText = "insert into ficheros_cliente (ruta, nombre, firma, tamano) values ('" + ruta + "', '" + nombre + "', '" + firma_base64 + "', " + fi.Length + ") ";
                                        //cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            if (pausa)
                                MessageBox.Show("Haciendo una pausa, pulsa Aceptar cuando pasen unos segundos.", "12BDI Lanzador", MessageBoxButton.OK);


                            /*
                            using (var t = con.BeginTransaction())
                            {

                                cmd.CommandText = "insert into ficheros_cliente (ruta, nombre, firma, tamano) values (@0,@1,@2,@3) ";

                                foreach (DataRow r in cliente.Rows)
                                {
                                    cmd.CommandText = "insert into ficheros_cliente (ruta, nombre, firma, tamano) values (@0,@1,@2,@3) ";

                                    cmd.Parameters.AddWithValue("@0", r["ruta"]);
                                    cmd.Parameters.AddWithValue("@1", r["nombre"]);
                                    cmd.Parameters.AddWithValue("@2", r["firma"]);
                                    cmd.Parameters.AddWithValue("@3", r["tamano"]);

                                    cmd.ExecuteNonQuery();
                                }

                                t.Commit();
                            }
                            
                        //}
                    } // cmd

                    
                    dcliente.writeDB();

                    log(" - Cache : "+cache.total_aciertos.ToString() +" / " +cache.total_consultas.ToString() + " - OK."); 

                } // con
                
            }
            catch (Exception x)
            {
                MessageBox.Show("No se pudieron procesar los ficheros del cliente: " + x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                throw x;
            }
            */
        }

        public bool ShouldUpdate
        {
            get { return dlist.Downloads.Count > 0; }
        }


        public void CompareCatalogs(string nombre_bdd, string repo)
        {
            long ficheros_descargados = 0;

            try
            {

                log("Comparando catálogos.");

                Data dservidor = new Data(this.nombre_bdd, "ficheros_servidor");
                Data dcliente = new Data(nombre_bdd, "ficheros_cliente");

                dservidor.readDB();
                dcliente.readDB();

                var notInCliente = (from f in dservidor.Table.AsEnumerable() select f).Except(from file in dcliente.Table.AsEnumerable() select file, new RutaComparer());
                var distinctMD5 = (from f in dcliente.Table.AsEnumerable() select f).Except(from file in dservidor.Table.AsEnumerable() select file, new FirmaMD5Comparer());
                var notInServer = (from f in dcliente.Table.AsEnumerable() select f).Except(from file in dservidor.Table.AsEnumerable() select file, new RutaComparer());

                // notInCliente -> Add to report
                // notInServer -> Eliminate or Keep (User choice)
                // distinctMD5 -> Update


                ficheros_a_descargar = notInCliente.Count() + distinctMD5.Count();
                ficheros_a_borrar = notInServer.Count();

                List<Task> tasks = new List<Task>();


                foreach (DataRow r in notInCliente)
                {
                    ficheros_descargados++;
                    string ruta = r.Field<string>("ruta").ToString();
                    string nombre = r.Field<string>("nombre").ToString();

                    dlist.Downloads.Add(new WebDownload(_url,repo, ruta + "/" + nombre,_target));
                }


                foreach (DataRow r in distinctMD5)
                {
                    ficheros_descargados++;
                    string ruta = r.Field<string>("ruta").ToString();
                    string nombre = r.Field<string>("nombre").ToString();

                    dlist.Downloads.Add(new WebDownload(_url, repo, ruta + "/" + nombre, _target));

                }
            }
            catch
            {

            }
        }
            

        // Actualizar
        // Esta función iguala los ficheros del cliente con los del servidor




                // - Ficheros en el servidor pero no en el cliente : los bajamos
                // - Ficheros con distintas firmas en cliente y servidor: los bajamos
                // - Ficheros en el cliente pero no en el servidor: los borramos
                // - Ficheros con misma firma: los dejamos tal cual
        public bool actualizar(string nombre_bdd, string carpeta_juego, string servidor_ftp, string usuario_ftp, string contraseña_ftp, bool hacer_cambios, out string informe_detallado, bool minimo = false)
        {
            long ficheros_descargados = 0;

            try
            {

                log("Comparando catálogos.");

                StringBuilder informe = new StringBuilder();

                Data dservidor = new Data(this.nombre_bdd, "ficheros_servidor");
                Data dcliente = new Data(nombre_bdd, "ficheros_cliente");

                dservidor.readDB();
                dcliente.readDB();


                //var distintos = dcliente.Table.AsEnumerable().Distinct<System.Data.DataRow>(new FirmaMD5Comparer());

                //var notInCliente = dservidor.Table.AsEnumerable().Except(dcliente.Table.AsEnumerable(), new RutaComparer());

                //var notInCliente = dservidor.Table.AsEnumerable().Where(dr => dcliente.Table.AsEnumerable().Any(dr2 => dr2.Field<string>("firma") != dr.Field<string>("firma")));

                var notInCliente = (from f in dservidor.Table.AsEnumerable() select f).Except(from file in dcliente.Table.AsEnumerable() select file, new RutaComparer());
                var distinctMD5 = (from f in dcliente.Table.AsEnumerable() select f).Except(from file in dservidor.Table.AsEnumerable() select file ,new FirmaMD5Comparer());
                var notInServer = (from f in dcliente.Table.AsEnumerable() select f).Except(from file in dservidor.Table.AsEnumerable() select file, new RutaComparer());


                if (minimo)
                {
                    notInCliente = notInCliente.Where(i => lista_mods.Contains(i.Field<string>("Mod").ToString()));
                    distinctMD5 = distinctMD5.Where(i => lista_mods.Contains(i.Field<string>("Mod").ToString()));
                    notInServer = notInServer.Where(i => lista_mods.Contains(i.Field<string>("Mod").ToString()));

                }
                // notInCliente -> Add to report
                // notInServer -> Eliminate or Keep (User choice)
                // distinctMD5 -> Update


                ficheros_a_descargar = notInCliente.Count() + distinctMD5.Count();
                ficheros_a_borrar = notInServer.Count();

                List<Task> tasks = new List<Task>();

                
                foreach (DataRow r in notInCliente)
                {
                    ficheros_descargados++;
                    string ruta = r.Field<string>("ruta").ToString();
                    string nombre = r.Field<string>("nombre").ToString();
                    informe.AppendLine("NUEVO: " + ruta + " " + nombre);


                    dlist.Downloads.Add(new WebDownload(_url,"", ruta + "/" + nombre,_target));


                    //Task t = new Task(() => { realizarDescarga(r, hacer_cambios, carpeta_juego, servidor_ftp, usuario_ftp, contraseña_ftp, informe, ficheros_descargados); });

                    //tasks.Add(t);

                    //t.Start();
                }


                foreach (DataRow r in distinctMD5)
                {
                    ficheros_descargados++;
                    string ruta = r.Field<string>("ruta").ToString();
                    string nombre = r.Field<string>("nombre").ToString();
                    informe.AppendLine("MODIFICADO: " + ruta + " " + nombre);


                    dlist.Downloads.Add(new WebDownload(_url, "", ruta + "/" + nombre, _target));

                    //Task t = new Task(() => { realizarDescarga(r, hacer_cambios, carpeta_juego, servidor_ftp, usuario_ftp, contraseña_ftp, informe, ficheros_descargados); });

                    //tasks.Add(t);

                    //t.Start();
                }


                //Task.WaitAll(tasks.ToArray());
                /*
                if (!minimo)
                {
                    long ficheros_borrados = 0;
                    bool borrar_ficheros = false;

                    if (hacer_cambios && ficheros_a_borrar != 0 && MessageBox.Show("¿Desea eliminar los addons marcados para BORRAR en el informe?", "12BDI Lanzador", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        borrar_ficheros = true;
                    }

                
                    foreach (DataRow r in notInServer)
                    {
                        string ruta = r.Field<string>("ruta").ToString();
                        string nombre = r.Field<string>("nombre").ToString();

                        informe.AppendLine("BORRAR: " + ruta + " " + nombre);

                        if (hacer_cambios && borrar_ficheros)
                        {
                            if (nombre.Length > 0)
                            {
                                File.Delete(carpeta_juego + ruta + "\\" + nombre); // Borramos el fichero local
                            }
                            else
                            {
                                Directory.Delete(carpeta_juego + ruta);
                            }

                            ficheros_borrados++;
                            log_progreso("Borrados : " + ficheros_borrados.ToString() + " / " + ficheros_a_borrar);
                        }

                    }
                    
                }
                */


                #region "Old code"
                /*
                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + nombre_bdd))
                {
                    con.Open();

                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.Connection = con;

                    // Totales - habría que sacarlo a un método externo.

                    string query = "select count(*) as conteo, sum(tamano) as total_tamano from ficheros_servidor ";
                    query += " where ruta||'\'||nombre not in (select ruta||'\'||nombre from ficheros_cliente) ";

                    cmd.CommandText = query;
                    ficheros_a_descargar = long.Parse(cmd.ExecuteScalar().ToString());

                    query = "select count(*) as conteo, sum(c.tamano) as total_tamano  ";
                    query += "from ficheros_servidor as s inner join ficheros_cliente as c where s.ruta = c.ruta and s.nombre = c.nombre and s.firma <> c.firma ";

                    cmd.CommandText = query;
                    ficheros_a_descargar += long.Parse(cmd.ExecuteScalar().ToString());

                    query = "select count(*) as conteo from ficheros_cliente ";
                    query += "  where ruta||'\'||nombre not in (select ruta||'\'||nombre from ficheros_servidor) ";
                    cmd.CommandText = query;
                    ficheros_a_borrar = long.Parse(cmd.ExecuteScalar().ToString());

                    // los que están en el servidor, pero no en el cliente

                    query = "select ruta, nombre, firma from ficheros_servidor ";
                    query += " where ruta||'\'||nombre not in (select ruta||'\'||nombre from ficheros_cliente) ";

                    cmd.CommandText = query;

                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            ficheros_descargados++;

                            if (hacer_cambios)
                                log_progreso("Descargando : " + ficheros_descargados + " / " + ficheros_a_descargar);

                            string ruta = dr[0].ToString();
                            string nombre = dr[1].ToString();
                            //int fsize = (int) dr[2];
                            //string md5 = dr[3].ToString();

                            informe.AppendLine("NUEVO: " + ruta + " " + nombre);

                            if (hacer_cambios)
                            {
                                if (!Directory.Exists(carpeta_juego + "\\" + ruta))
                                    Directory.CreateDirectory(carpeta_juego + "\\" + ruta);
                                File.Delete(carpeta_juego + ruta + "\\" + nombre); // Borramos el fichero local

                                if (!Utiles.descarga_fichero(servidor_ftp, usuario_ftp, contraseña_ftp, ruta + "\\" + nombre, carpeta_juego + ruta + "\\" + nombre))
                                    errores_comunicacion = true;
                            }

                        }
                        dr.Close();
                    }

                    // Los que están en las dos tablas, pero tienen firmas distintas
                    query = "select c.ruta, c.nombre, c.firma ";
                    query += "from ficheros_servidor as s inner join ficheros_cliente as c where s.ruta = c.ruta and s.nombre = c.nombre and s.firma <> c.firma ";

                    cmd.CommandText = query;

                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            ficheros_descargados++;
                            if (hacer_cambios)
                                log_progreso("Descargando : " + ficheros_descargados + " / " + ficheros_a_descargar);

                            string ruta = dr[0].ToString();
                            string nombre = dr[1].ToString();
                            //int fsize = (int)dr[2];
                            //string md5 = dr[3].ToString();

                            informe.AppendLine("CAMBIADO: " + ruta + " " + nombre);

                            if (hacer_cambios)
                            {
                                if (!Directory.Exists(carpeta_juego + "\\" + ruta))
                                    Directory.CreateDirectory(carpeta_juego + "\\" + ruta);
                                File.Delete(carpeta_juego + ruta + "\\" + nombre); // Borramos el fichero local

                                if (!Utiles.descarga_fichero(servidor_ftp, usuario_ftp, contraseña_ftp, ruta + "\\" + nombre, carpeta_juego + ruta + "\\" + nombre))
                                    errores_comunicacion = true;
                            }
                        }
                        dr.Close();
                    }

                    // Los que están en el cliente, pero no en el servidor.
                    bool borrar_ficheros = false;

                    if (hacer_cambios &&
                        ficheros_a_borrar != 0 &&
                        MessageBox.Show("¿Desea eliminar los addons marcados para BORRAR en el informe?", "12BDI Lanzador", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        borrar_ficheros = true;
                    }

                    query = "select * from ficheros_cliente ";
                    query += "  where ruta||'\'||nombre not in (select ruta||'\'||nombre from ficheros_servidor) ";
                    cmd.CommandText = query;
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {

                        long ficheros_borrados = 0;
                        while (dr.Read())
                        {
                            string ruta = dr[0].ToString();
                            string nombre = dr[1].ToString();

                            informe.AppendLine("BORRAR: " + ruta + " " + nombre);

                            if (hacer_cambios && borrar_ficheros)
                            {
                                File.Delete(carpeta_juego + ruta + "\\" + nombre); // Borramos el fichero local
                                ficheros_borrados++;
                                log_progreso("Borrados : " + ficheros_borrados.ToString() + " / " + ficheros_a_borrar);
                            }

                        }
                        dr.Close();
                    }
                    con.Close();
                } */
                #endregion

                // Se han producido errores de comunicación ? avisamos al usuario
                if (errores_comunicacion)
                    MessageBox.Show("Se han producido errores de comunicación al trasnmitir uno o más ficheros. Es necesario repetir el proceso.", "12BDI Lanzador", MessageBoxButton.OK, MessageBoxImage.Information);

                // Salida para el usuario

                informe_detallado = Path.GetTempFileName() + ".txt";
                File.WriteAllText(informe_detallado, informe.ToString());

                //Hay que hacer bajarse cosas ?
                if (informe.Length == 0)
                    return false;
                else
                    return true;

            }
            catch (Exception x)
            {
                MessageBox.Show("No se pudo finalizar la descarga: " + x.Message, x.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                throw x;
            }
        }

        private void realizarDescarga(DataRow dr, bool hacer_cambios, string carpeta_juego, string servidor_ftp, string usuario_ftp, string contraseña_ftp, StringBuilder informe, long descargados)
        {

            s.Wait();


            string ruta = dr.Field<string>("ruta").ToString();
            string nombre = dr.Field<string>("nombre").ToString();
          

            
            if (hacer_cambios)
            {
                //log_progreso("Descargando : " + descargados + " / " + ficheros_a_descargar + "\n");
                log("Descargando " + nombre);
            }


            //int fsize = (int) dr[2];
            //string md5 = dr[3].ToString();

            //informe.AppendLine("NUEVO: " + ruta + " " + nombre);

            if (hacer_cambios)
            {
                if (!Directory.Exists(carpeta_juego + "\\" + ruta))
                    Directory.CreateDirectory(carpeta_juego + "\\" + ruta);
                File.Delete(carpeta_juego + ruta + "\\" + nombre); // Borramos el fichero local

                WebDownload wb = new WebDownload();
                wb.DownloadFileAsync(usuario_ftp, contraseña_ftp + "/" + ruta + "/" + nombre, carpeta_juego + ruta + "\\" + nombre);


                //if (!Utiles.descarga_fichero(servidor_ftp, usuario_ftp, contraseña_ftp, ruta + "\\" + nombre, carpeta_juego + "\\" + ruta + "\\" + nombre))
                //    errores_comunicacion = true;
            }

            

            s.Release();

        }


        // Indica si el nombre de fichero o carpeta es valido. Los nombres validos no contienen caracteres incluidos en el array que se define arriba
        private bool nombre_valido(string s)
        {
            string[] caracteres_a_ignorar = { "'", "#" }; // Si un fichero tiene estos caracteres en su nombre, se ignoran

            foreach (string v in caracteres_a_ignorar)
            {
                if (s.Contains(v))
                    return false;
            }

            return true;
        }

        // Visualiza un mensaje en la ventana principal a través de su "dispatcher"
        void log(string s)
        {
            w.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                w.log(s, true);
            }));
        }
        void log_progreso(string s)
        {
            w.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                w.log(s, false);
            }));
        }

    }
}
