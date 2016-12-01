using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Data;
using System.Threading;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Data.HashFunction;
using NLog;
using NLog.Targets;
using Newtonsoft.Json;

namespace ServidoresData
{
    public class ModView
    {
        public string Arma { get; set; }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
    }
    
    public class CatalogModsProgress
    {
        public string Mod { get; set; }
        public int Progress { get; set; }
        public int TotalMods { get; set; }
        public int ProgressMod { get; set; }
    }


    public class PublishModsProgress
    {
        public string Mod { get; set; }
        public int Progress { get; set; }
        public int TotalFiles { get; set; }
        public int ProgressFile { get; set; }
    }

    public class Repository : INotifyPropertyChanged, IDisposable
    {

        string nombre;
        string basepath;
        string relativepath;
        string target;

        RepositoryBay bay;
        
        ObservableCollection<Mod> modlist;
        List<ModView> _serverMods;
        
        //db4oDB db;
        //db4oDB dbrepo;

        //db4oDB dcliente;
        //db4oDB dservidor;

        ObservableCollection<WebDownloadAction> dlist;

        DirectoryInfo basedir;
        DirectoryInfo targetdir;

        List<CommandBase> TasksToDo;
        List<Task> commandList;

        List<DBData> distinctMD5;

        PlanProgressEventArgs pplan;

        List<CommandBase> currents = new List<CommandBase>();

        bool _downloading = false;

        List<DBData> modsInRepo;
        ObservableCollection<DBData> dcliente;
        ObservableCollection<DBData> dservidor;

        object locker = new object();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public delegate void CatalogoCompletedEventHandler(object sender, CatalogoCompletedEventArgs e);
        public event CatalogoCompletedEventHandler CatalogoCompleted;

        public delegate void CreateRepositoryCompletedEventHandler(object sender, CatalogoCompletedEventArgs e);
        public event CreateRepositoryCompletedEventHandler CreateRepositoryCompleted;

        public delegate void CatalogoCompareCompletedEventHandler(object sender, CatalogoCompareCompletedEventArgs e);
        public event CatalogoCompareCompletedEventHandler CatalogoCompareCompleted;

        public delegate void UpgradeRepositoryProgressChangedEventHandler(object sender, TaskProgressProgressChanged e);
        public event UpgradeRepositoryProgressChangedEventHandler UpgradeRepositoryProgressChanged;

        public delegate void UpgradeRepositoryBeforeExecuteEventHandler(object sender, TaskProgressProgressChanged e);
        public event UpgradeRepositoryBeforeExecuteEventHandler UpgradeRepositoryBeforeExecute;

        public delegate void PlanProgressChangedEventHandler(object sender, PlanProgressEventArgs e);
        public event PlanProgressChangedEventHandler PlanProgressChanged;

        public delegate void UpgradeRepositoryCompletedEventHandler(object sender, AsyncCompletedEventArgs e);
        public event UpgradeRepositoryCompletedEventHandler UpgradeRepositoryCompleted;

        static SemaphoreSlim sem;
        public class TaskProgressProgressChanged : ProgressChangedEventArgs
        {
            public object Command { get; set; }
            public string Message { get; set; }

            public double Speed { get; set; }
            public double TotalBytes { get; set; }
            public double ActualBytes { get; set; }

            public TaskProgressProgressChanged(int progressPercentage, object userState) : base(progressPercentage, userState)
            {
            }
        }

        public class PublishRepositoryEventArgs : ProgressChangedEventArgs
        {
            public string Message;
            public int Total;
            public int Current;

            public PublishRepositoryEventArgs(int progressPercentage, object userState) : base(progressPercentage, userState)
            {
            }
        }

        public class CatalogoCompletedEventArgs : AsyncCompletedEventArgs
        {
            public int Total { get; set; }
            public int Processed { get; set; }
            public int NewFiles { get; set; }
            public string Message { get; set; }
            public int SkippedFiles { get; set; }


            public CatalogoCompletedEventArgs(Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
            {

            }

        }

        public class CatalogoCompareCompletedEventArgs : AsyncCompletedEventArgs
        {
            public int NotInServer { get; set; }
            public int NotInClient { get; set; }
            public int DistinctSignature { get; set; }
            public string Message { get; set; }
        

            public CatalogoCompareCompletedEventArgs(Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
            {

            }

        }

        public class PlanProgressEventArgs : ProgressChangedEventArgs
        {
            public int Total;
            public int Current;

            public PlanProgressEventArgs(int progressPercentage, object userState) : base(progressPercentage, userState)
            {
            }
        }

        public Repository()
        {
            
            modlist = new ObservableCollection<Mod>();
            modsInRepo = new List<DBData>();
        }
      	
		//  
        // folder : Carpeta donde se encuentran los mods
        // Bay : Localizacion de la base de datos
        // Repo : Nombre del repo
        // ModList : Lista de mods (todos)
        //       
        public Repository(string folder, RepositoryBay Bay, string Repo, List<ModView> Modlist)
        {
            _downloading = false;
            sem = new SemaphoreSlim(12);

            Console.WriteLine("Paso 1");

            //FileTarget target = LogManager.Configuration.FindTargetByName<FileTarget>("logfile");
            //string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\" + "log_" + System.DateTime.Now.ToShortDateString() + ".txt";
            //target.FileName = filename;

            Console.WriteLine("Paso 2");

            basepath = folder;
            bay = Bay;
            nombre = Repo;
            basedir = new DirectoryInfo(basepath);

            Console.WriteLine("Paso 3");

            modlist = new ObservableCollection<Mod>();
            _serverMods = Modlist;
            //CatalogFolderAsync(Bay,Repo);

            Console.WriteLine("Paso 4");

            DirectoryInfo baydir = bay.GetDirectoryForRepo(nombre);

            if (!baydir.Exists)
            {
                baydir.Create();
            }

            string dbfile = Path.Combine(bay.ToDirectoryInfo().FullName, nombre, "ficheros.db4o");

            string jsonfile = Path.Combine(Bay.ToDirectoryInfo().FullName, Repo, "ficheros.json");

            /*
            try
            {
                logger.Info("Se va a defragmentar la base de datos");
                db4oDB.Defragment(dbfile, dbfile + ".backup");
                logger.Info("Base de datos defragmentada con exito");
            }
            catch (Exception ex1)
            {
                logger.Fatal("Excepcion al defragmentar la base de datos : {0}", ex1.Message);
                //throw ex1;
            }
            */

            try
            {

                string jsoncontent = "";

                if (File.Exists(jsonfile))
                {
                    jsoncontent = File.ReadAllText(jsonfile);
                }

                if (jsoncontent.Length > 0)
                {
                    dcliente = (ObservableCollection<DBData>)JsonConvert.DeserializeObject<ObservableCollection<DBData>>(jsoncontent);
                }
                else
                {
                    dcliente = new ObservableCollection<DBData>();
                }
            }
            catch (Exception ex2)
            {
                logger.Fatal("Excepcion al abrir la base de datos : {0}", ex2.Message);

                throw;
            }

        }

        public Repository(string folder, RepositoryBay Bay, string Repo)
        {
            _downloading = false;
            sem = new SemaphoreSlim(12);

            Console.WriteLine("Paso 1");


            basepath = folder;
            bay = Bay;
            nombre = Repo;
            basedir = new DirectoryInfo(basepath);

            Console.WriteLine("Paso 2");

            DirectoryInfo baydir = bay.GetDirectoryForRepo(nombre);

            if (!baydir.Exists)
            {
                baydir.Create();
            }

            string dbfile = Path.Combine(bay.ToDirectoryInfo().FullName, nombre, "ficheros.db4o");

            string jsonfile = Path.Combine(Bay.ToDirectoryInfo().FullName, Repo, "ficheros.json");

            /*
            try
            {
                logger.Info("Se va a defragmentar la base de datos");
                db4oDB.Defragment(dbfile, dbfile + ".backup");
                logger.Info("Base de datos defragmentada con exito");
            }
            catch (Exception ex1)
            {
                logger.Fatal("Excepcion al defragmentar la base de datos : {0}", ex1.Message);
                //throw ex1;
            }
            */

            try
            {
                string jsoncontent = "";

                if (File.Exists(jsonfile))
                {
                    jsoncontent = File.ReadAllText(jsonfile);
                }

                if (jsoncontent.Length > 0)
                {
                    modsInRepo = (List<DBData>)JsonConvert.DeserializeObject<List<DBData>>(jsoncontent);
                }
                else
                {
                    modsInRepo = new List<DBData>();
                }

                dcliente = new ObservableCollection<DBData>();

                //dcliente = new db4oDB(dbfile, "ficheros_cliente", false);
                //dcliente.Open();
                //dcliente.ReadDB();
            }
            catch (Exception ex2)
            {
                logger.Fatal("Excepcion al abrir la base de datos : {0}", ex2.Message);

                throw;
            }

        }

        public Repository(string folder, string Repo, string targetFolder, List<ModView> Modlist)
        {
            _downloading = false;
            sem = new SemaphoreSlim(12);

            FileTarget targ = LogManager.Configuration.FindTargetByName<FileTarget>("logfile");
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\" + "log_" + System.DateTime.Now.ToShortDateString() + ".txt";
            targ.FileName = filename;

            basepath = folder;
            nombre = Repo;
            basedir = new DirectoryInfo(basepath);
            target = Path.Combine(targetFolder,nombre);
            targetdir = new DirectoryInfo(target);

            modlist = new ObservableCollection<Mod>();
            _serverMods = Modlist;

            string dbfile = Path.Combine(targetdir.FullName, "ficheros.db4o");

            string jsonfile = Path.Combine(targetdir.FullName, "ficheros.json");

            if (!targetdir.Exists)
            {
                targetdir.Create();
            }

            /*
            try
            {
                logger.Info("Se va a defragmentar la base de datos");
                db4oDB.Defragment(dbfile, dbfile + ".backup");
                logger.Info("Base de datos defragmentada con exito");
            }
            catch (Exception ex1)
            {
                logger.Fatal("Excepcion al defragmentar la base de datos : {0}", ex1.Message);
            }
            */

            try
            {

                string jsoncontent = "";

                if (File.Exists(jsonfile))
                {
                    jsoncontent = File.ReadAllText(jsonfile);
                }

                if (jsoncontent.Length > 0)
                {
                    modsInRepo = (List<DBData>)JsonConvert.DeserializeObject<List<DBData>>(jsoncontent);
                }
                else
                {
                    modsInRepo = new List<DBData>();
                }
                dcliente = new ObservableCollection<DBData>();

                //dcliente = new db4oDB(dbfile, "ficheros_cliente", false);
                //dcliente.Open();
                //dcliente.ReadDB();
            }
            catch (Exception ex2)
            {
                logger.Fatal("Excepcion al abrir la base de datos : {0}", ex2.Message);

                throw;
            }
        }


        public bool Downloading
        {
            get { return _downloading;  }
        }
        public List<Task> Tasks
        {
            get { return commandList;}
            set { }
        }

        public static Repository FromFolder(string folder, RepositoryBay Bay, string Repo, List<ModView> Modlist, IProgress<CatalogModsProgress> progress = null)
        {

            Repository r = new Repository();
            sem = new SemaphoreSlim(12);

            r.basepath = folder;
            r.bay = Bay;
            r.nombre = Repo;
            r.basedir = new DirectoryInfo(r.basepath);

            r._serverMods = Modlist;

            string dbfile = Path.Combine(Bay.ToDirectoryInfo().FullName, Repo, "ficheros.db4o");

            //db4oDB.Defragment(dbfile, dbfile + ".backup");
            //r.dcliente = new db4oDB(dbfile, "ficheros_cliente", false);
            //r.dcliente.Open();
            //r.dcliente.ReadDB();

            if (!Directory.Exists(folder)) { return r; }
			
            r.CatalogFolderAsync(Bay, Repo, progress);
            
            return r;
        }

        protected void OnCatalogCompleted(CatalogoCompletedEventArgs e)
        {
            if (CatalogoCompleted != null)
            {
                CatalogoCompleted(this, e);
            }
        }


        protected void OnCreateRepositoryCompleted(CatalogoCompletedEventArgs e)
        {
            if (CreateRepositoryCompleted != null)
            {
                CreateRepositoryCompleted(this, e);
            }
        }

        public void CatalogFolderAsync(IProgress<CatalogModsProgress> progress)
        {
            if ((bay != null) && (nombre.Length > 0))
            {
                CatalogFolderAsync(bay, nombre, progress);
            }
        }


        public void CatalogRepositoryAsync(IProgress<CatalogModsProgress> progress = null)
        {
            int tfiles = 0;
            int tskip = 0;
            int tnew = 0;

            Task t1 = new Task(
            () =>
            {
                //DirectoryInfo rbay = Bay.GetDirectoryForRepo(Repo);
                FileInfo fi;
                //bool db_previa = false;
                DirectoryInfo di = new DirectoryInfo(target);

                if (!di.Exists)
                {
                    di.Create();
                }

                fi = new FileInfo(target + @"\ficheros.db4o");

                var flist = from d in basedir.GetDirectories() where d.FullName.Contains("@") select d;


                CatalogModsProgress cp = new CatalogModsProgress();
                cp.TotalMods = flist.Count();

                foreach (DirectoryInfo fol in flist)
                {
                    Mod m = new Mod(fol.FullName, this);
                    m.Nombre = Path.GetFileName(fol.Name);
                    m.RelativePath = fol.FullName.Replace(Path.GetFullPath(basedir.FullName), "");
                    
                    modlist.Add(m);

                    List<FileInfo> files = fol.EnumerateFiles("*", SearchOption.AllDirectories).ToList<FileInfo>();

                    cp.ProgressMod++;

                    int total = files.Count();
                    int procesados = 0;

                    foreach (FileInfo fichero in files)
                    {

                        tfiles++;

                        if (progress != null)
                        {
                            procesados++;
                            cp.Mod = m.Nombre;

                            int p = (procesados * 100) / total;

                            cp.Progress = p;

                            progress.Report(cp);
                        }

                        //(Path.GetDirectoryName(fichero.FullName).Replace(Path.GetFullPath(basedir.FullName), ""), fichero.Name, fichero.Length)

                        var exist = from d in dcliente where d.Ruta == (Path.GetDirectoryName(fichero.FullName).Replace(Path.GetFullPath(basedir.FullName),"")) && d.Nombre == fichero.Name && d.Tamano == fichero.Length  select d;


                        if (exist.Count() > 0)
                        {
                            tskip++;
                        }
                        else
                        {
                            try
                            {
                                DBData data = new DBData();

                                data.Ruta = Path.GetDirectoryName(fichero.FullName).Replace(Path.GetFullPath(basedir.FullName), "");
                                data.Nombre = fichero.Name;
                                data.Firma = Repository.xxHashSignature(fichero.FullName);
                                data.Mod = m.Nombre;
                                data.Tamano = fichero.Length;

                                dcliente.Add(data);

                                tnew++;
                            }
                            catch (Exception ex)
                            {
                                logger.Fatal("Error al insertar un registro en el catalogo del repositorio : {0}", ex.Message);
                                logger.Fatal("================> Mod : {0}", m.Nombre);
                                logger.Fatal("================> Fichero : {0}", fichero.FullName);


                                throw ex;
                            }
                        }
                    }
                }


                //List<DBData> enCliente = (from DBData f in dcliente.Container select f).ToList<DBData>();


                File.WriteAllText(di + @"\ficheros.json", JsonConvert.SerializeObject(dcliente));

                CatalogoCompletedEventArgs c = new CatalogoCompletedEventArgs(null, false, null);
                c.Total = tfiles;
                c.NewFiles = tnew;
                c.SkippedFiles = tskip;

                OnCreateRepositoryCompleted(c);

            });

            // Arrancamos la tarea inicial
            t1.Start();

        }


        public static string xxHashSignature(string fichero)
        {
            try
            {
                string firma_base64 = "";

                xxHash firma = null;

                if (firma == null) { firma = new xxHash(); }

                //using (MD5 firma = MD5.Create())
                using (FileStream streamFichero = File.OpenRead(fichero))
                {
                    byte[] f = firma.ComputeHash(streamFichero);
                    firma_base64 = Convert.ToBase64String(f);
                }

                return firma_base64;
            }
            catch (Exception x)
            {
                logger.Fatal("Error al calacular el Hash : {0}", x.Message);

                throw x;
            }
        }

        public void CatalogFolderAsync(RepositoryBay Bay, string Repo, IProgress<CatalogModsProgress> progress = null)
        {
            int tfiles = 0;
            int tproc = 0;
            int tskip = 0;
            int tnew = 0;

            Task t1 = new Task(
            () =>
            {
                DirectoryInfo rbay = Bay.GetDirectoryForRepo(Repo);
                FileInfo fi;
                //bool db_previa = false;

                fi = new FileInfo(Path.Combine(rbay.FullName,"ficheros.db4o"));

                var flist = from d in basedir.GetDirectories() where d.FullName.Contains("@") select d;
               
                CatalogModsProgress cp = new CatalogModsProgress();
                cp.TotalMods = flist.Count();

                foreach (DirectoryInfo fol in flist)
                {
                    Mod m = new Mod(fol.FullName, this);
                    m.Nombre = Path.GetFileName(fol.Name);
                    m.RelativePath = @"\" + Path.GetFileName(fol.Name);

                    modlist.Add(m);

                    List<FileInfo> files = fol.EnumerateFiles("*", SearchOption.AllDirectories).ToList<FileInfo>();

                    cp.ProgressMod++;

                    
                    int total = files.Count();
                    int procesados = 0;

                    lock (files)
                    {
                        foreach (FileInfo fichero in files)
                        {

                            tfiles++;

                            if (progress != null)
                            {
                                procesados++;
                                cp.Mod = m.Nombre;

                                int p = (procesados * 100) / total;

                                cp.Progress = p;

                                progress.Report(cp);
                            }

                            var exist = from d in dcliente where d.Ruta == (Path.GetDirectoryName(fichero.FullName).Replace(Path.GetFullPath(basedir.FullName), "")) && d.Nombre == fichero.Name && d.Tamano == fichero.Length select d;


                            if (exist.Count() > 0)
                            {
                                tskip++;
                            }
                            else
                            {
                                try
                                {
                                    if (fichero.Length > 0)
                                    {
                                        DBData data = new DBData();

                                        data.Ruta = Path.GetDirectoryName(fichero.FullName).Replace(Path.GetFullPath(basedir.FullName), "");
                                        data.Nombre = fichero.Name;
                                        data.Firma = Repository.xxHashSignature(fichero.FullName);
                                        data.Mod = m.Nombre;
                                        data.Tamano = fichero.Length;

                                        dcliente.Add(data);
                                        
                                        tnew++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Fatal("Error al insertar un registro en el catalogo : {0}", ex.Message);
                                    logger.Fatal("================> Mod : {0}", m.Nombre);
                                    logger.Fatal("================> Fichero : {0}", fichero.FullName);

                                    throw;
                                }
                            }



                        }
                    }
                }

                //List<DBData> all = (from DBData p in dcliente.Container select p).ToList<DBData>();

                CatalogModsProgress cp2 = new CatalogModsProgress();
                cp2.TotalMods = dcliente.Count();

                int tot = dcliente.Count();
                int proc = 0;

                ObservableCollection<DBData> toRemove = new ObservableCollection<DBData>();

                foreach (DBData o in dcliente)
                {
                    if (progress != null)
                    {
                        proc++;
                        cp2.Mod = o.Nombre;

                        int p = (proc * 100) / tot;

                        cp2.Progress = p;

                        progress.Report(cp2);
                    }

                    string pa = basedir.FullName +  o.Ruta + @"\" + o.Nombre;

                    FileInfo fil = new FileInfo(pa);

                    if (!fil.Exists)
                    {
                        toRemove.Add(o);
                    }
                }

                foreach (DBData o in toRemove)
                {
                    dcliente.Remove(o);
                }

                File.WriteAllText(bay.GetDirectoryForRepo(this.nombre).FullName + @"\ficheros.json", JsonConvert.SerializeObject(dcliente));

                CatalogoCompletedEventArgs c = new CatalogoCompletedEventArgs(null, false, null);
                c.Total = tfiles;
                c.NewFiles = tnew;
                c.SkippedFiles = tskip;

                OnCatalogCompleted(c);
            });
         
            // Arrancamos la tarea inicial
            t1.Start();

        }

        public string Nombre
        {
            get
            {
                return nombre;
            }

            set
            {
                nombre = value; NotifyPropertyChanged("Nombre");
            }
        }

        public string RelativePath
        {
            get
            {
                return relativepath;
            }

            set
            {
                relativepath = value; NotifyPropertyChanged("RelativePath");
            }
        }


        public ObservableCollection<Mod> ModList
        {
            get { return modlist;  }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        /*
        public void CreateDB()
        {
            string path = basepath + @"\" + relativepath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

        	dbname = basepath + @"\" + relativepath + @"\" + "ficheros.db";
        	
        	//data = new RepositoryData(dbname,"ficheros_servidor");
        	
        }
        */

        public void Publish(IProgress<PublishModsProgress> pr)
        {

            if (!targetdir.Exists)
            {
                targetdir.Create();
            }
           
            var ficheros = from DBData data in dcliente select data;

            int total = ficheros.Count();
            int cur = 0;

            foreach(DBData d in ficheros)
            {
                DirectoryInfo di = new DirectoryInfo(targetdir.FullName + d.Ruta);

                if (!di.Exists)
                {
                    di.Create();
                }

                FileInfo fi = new FileInfo(basedir.FullName + d.Ruta + @"\" +  d.Nombre);

                cur++;

                PublishModsProgress pm = new PublishModsProgress();

                pm.Mod = d.Mod;
                pm.Progress = (cur * 100) / total;
                pm.ProgressFile = cur;
                pm.TotalFiles = total;

                pr.Report(pm);

                fi.CopyTo(Path.Combine(di.FullName, d.Nombre), true);
            }

            /*
            foreach(Mod m in modlist)
            {
                foreach(ModFile f in m.Files)
                {
                    
                    //FileInfo fi = new FileInfo( )
                    Console.WriteLine(f.Basename);
                }
            }
            */
        }

        public void CompareCatalogs(string nombre_bdd, string repo, string srv)
        {
            int tnis = 0;
            int tdsig = 0;
            int tnic = 0;

            Task t = new Task(() =>
               {
                   TasksToDo = new List<CommandBase>();

                   try
                   {

                       //log("Comparando catálogos.");

                       //dservidor = new db4oDB(nombre_bdd, "ficheros_servidor",false);
                       //dservidor.Open();
                       //dservidor.ReadDB();

                       dservidor = new ObservableCollection<DBData>();

                       string jsoncontent = "";

                       if (File.Exists(nombre_bdd))
                       {
                           jsoncontent = File.ReadAllText(nombre_bdd);
                       }

                       if (jsoncontent.Length > 0)
                       {
                           dservidor = (ObservableCollection<DBData>)JsonConvert.DeserializeObject<ObservableCollection<DBData>>(jsoncontent);
                       }
                       

                       //List<DBData> enCliente = (from DBData f in dcliente.Container select f).ToList<DBData>();
                       //List<DBData> enServidor = (from DBData f in dservidor select f).ToList<DBData>();


                       //var distinctMD5 = enServidor.Except(enCliente, new FirmaMD5Comparer());

                       distinctMD5 = new List<DBData>();

                       foreach(DBData s in dservidor)
                       {
                           var c = from DBData cli in dcliente where cli.Ruta == s.Ruta && cli.Nombre == s.Nombre && cli.Firma != s.Firma select cli;

                           if (c.Count() > 0)
                           {
                               foreach (DBData d in c)
                               {
                                   Console.WriteLine(s.Ruta + "|" + d.Ruta + " - " + s.Nombre + "|" + d.Nombre + " - " + s.Firma + "|" + d.Firma);
                               }

                               distinctMD5.Add(s);
                           }
                       }

                       //var distinctMD5 = (from DBData f in dcliente.Container select f).Except(enServidor, new FirmaMD5Comparer());
                       var notInCliente = (from DBData f in dservidor select f).Except(dcliente, new RutaComparer());
                       var notInServer = (from DBData f in dcliente select f).Except(dservidor, new RutaComparer());

                       tnis = notInServer.Count();
                       tnic = notInCliente.Count();
                       tdsig = distinctMD5.Count();

                        // notInCliente -> Add to report
                        // notInServer -> Eliminate or Keep (User choice)
                        // distinctMD5 -> Update

                        dlist = new ObservableCollection<WebDownloadAction>();

                        //ficheros_a_descargar = notInCliente.Count() + distinctMD5.Count();
                        //ficheros_a_borrar = notInServer.Count();

                       foreach (DBData r in notInCliente)
                       {
                           string ruta = r.Ruta;
                           string nombre = r.Nombre;

                           IProgress<int> p = new Progress<int>();
                           DownloadFileCommand dc = new DownloadFileCommand(srv, repo, ruta + "/" + nombre, basepath + @"\" + ruta + @"\" + nombre, p);
                           dc.Description = "Descargar " + nombre;

                           dcliente.Add(r);

                           TasksToDo.Add(dc);
                       }


                       foreach (DBData r in distinctMD5)
                       {
                           string ruta = r.Ruta;
                           string nombre = r.Nombre;

                           IProgress<int> p1 = new Progress<int>();
                           DownloadFileCommand dc = new DownloadFileCommand(srv, repo, ruta + "/" + nombre, basepath + @"\" + ruta + @"\" + nombre, p1);


                           dc.Description = "Descargar " + nombre;

                           /*
                           var res = from DBData f in dcliente where f.Ruta == r.Ruta && f.Nombre == r.Nombre select f;

                           foreach (DBData o in res)
                           {
                               o.Firma = r.Firma;
                               dcliente.Add(o);
                           }
                           */

                           dcliente.Add(r);

                           TasksToDo.Add(dc);
                        }


                       foreach (DBData r in notInServer)
                       {
                           // Opcion #1 : Repo global = Directorio del Arma =>  Se mira la lista de mods para ver si algun otro repo lo necesita
                           //                                                   Si no se necesita se borra. Si se necesita se deja
                           // Opcion #2 : Repo custom = Directorio de Usuario => Se borra

                           if (IsDefaultFolder(basepath))
                           {
                               var result = from ModView m in _serverMods where m.Name == r.Mod select m;

                               if (result.Count() == 0)
                               {
                                   string ruta = r.Ruta;
                                   string nombre = r.Nombre;

                                   IProgress<int> p = new Progress<int>();
                                   DeleteFileCommand df = new DeleteFileCommand(new FileInfo(basepath + @"\" + ruta + @"\" + nombre), p);

                                   df.Description = "Elimininar " + nombre;

                                   var res = from DBData f in dcliente where f.Ruta == r.Ruta && f.Nombre == r.Nombre select f;

                                   
                                   TasksToDo.Add(df);
                               }
                           }
                           else
                           {
                               string ruta = r.Ruta;
                               string nombre = r.Nombre;

                               IProgress<int> p = new Progress<int>();
                               DeleteFileCommand df = new DeleteFileCommand(new FileInfo(basepath + @"\" + ruta + @"\" + nombre), p);

                               df.Description = "Elimininar " + nombre;

                               TasksToDo.Add(df);

                               var res = from DBData f in dcliente where f.Ruta == r.Ruta && f.Nombre == r.Nombre select f;

                               foreach (DBData o in res)
                               {
                                   dcliente.Remove(o);
                               }

                               
                           }

                           DirectoryInfo di = new DirectoryInfo(basepath + @"\" + r.Ruta);

                           deleteIfEmpty(di);
                       }

                   }
                   catch (Exception ex)
                   {
                       logger.Fatal("Error al comparar Catalogos : {0}", ex.Message);

                       throw;
                   }
            });

            Task t1 = t.ContinueWith(ant =>
            {
                CatalogoCompareCompletedEventArgs c = new CatalogoCompareCompletedEventArgs(null, false, null);
                c.NotInClient = tnic;
                c.NotInServer = tnis;
                c.DistinctSignature = tdsig;

                OnCatalogCompareCompleted(c);
            });

            

            t.Start();
        }


        private void deleteIfEmpty(DirectoryInfo p)
        {
            var fls = p.EnumerateFiles();
            var drs = p.EnumerateDirectories();

            if ((fls.Count() == 0) && (drs.Count() == 0))
            {
                DirectoryInfo parent = p.Parent;

                p.Delete();

                deleteIfEmpty(parent);
            }
        }

        public void UpgradeRepositoryAsync()
        {
            
            pplan = new PlanProgressEventArgs(0, null);

            PlanProgressChangedEventHandler plan = new PlanProgressChangedEventHandler(OnPlanProgress);

            //sem = new SemaphoreSlim(0, 1);

            //Task t = new Task(() =>
            //{
                commandList = new List<Task>();

                //createTasks();
                executeTasks();
            //});

            AsyncCompletedEventArgs e = new AsyncCompletedEventArgs(null, false, null);

            /*
            while (currents.Count > 0)
            {
                // do nothing
            }
            */

            OnUpgradeRepositoryCompleted(e);

            //File.Copy(bay.GetDirectoryForRepo(this.nombre) + @"\timestamp_" + this.nombre + ".txt", this.targetdir + @"\timestamp_" + this.nombre + ".txt");

            string ccc = bay.GetDirectoryForRepo(this.nombre).FullName;

            long sum = System.Convert.ToInt64(File.ReadAllText(ccc + @"\timestamp_" + this.nombre + ".txt"));

            DateTime d = DateTime.FromBinary(sum);

            File.Delete(ccc + @"\timestamp_" + this.nombre + ".txt");
            File.WriteAllText(ccc + @"\timestamp_" + this.nombre + ".txt", d.AddDays(1).ToBinary().ToString());

        }

        
        private void executeTasks()
        {

            PlanProgressEventArgs pplan = new PlanProgressEventArgs(0, null);
            pplan.Total = TasksToDo.Count();
            pplan.Current = 0;

            _downloading = true;
            NotifyPropertyChanged("Downloading");

            foreach (CommandBase c in TasksToDo)
            {
                try
                {
                    TaskProgressProgressChanged e = new TaskProgressProgressChanged(0, null);
                    e.Message = c.Description;

                    OnUpgradeRepositoryProgressChanged(e);

                    
                    

                    if (c is DownloadFileCommand)
                    {
                        DownloadFileCommand dc = (DownloadFileCommand)c;
                        dc.DownloadFileCommandCompleted += (s, ev) =>
                        {
                            TaskProgressProgressChanged ez = new TaskProgressProgressChanged(0, null);

                            ez.Message = c.Description + " termino correctamente";
                            OnUpgradeRepositoryProgressChanged(ez);

                            dc.DownloadFileCommandCompleted = new CommandBase.CommandCompletedEventHandler(downloadFileCompleted);

                            currents.Remove(c);

                            //pplan.Current++;

                            //OnPlanProgress(this, pplan);

                        };

                        dc.DownloadFileBeforeExecute += (ss, eev) =>
                        {
                            if (UpgradeRepositoryBeforeExecute != null)
                            {
                                TaskProgressProgressChanged evv = new TaskProgressProgressChanged(0, null);
                                evv.Message = c.Description;
                                UpgradeRepositoryBeforeExecute(this, e);
                            }
                        };

                        dc.Progress.ProgressChanged += (s, pr) =>
                        {
                            TaskProgressProgressChanged ev = new TaskProgressProgressChanged(pr, null);
                            ev.Message = c.Description;
                            OnUpgradeRepositoryProgressChanged(ev);

                            dc.Progreso = pr;

                        };
                    }

                    pplan.Current++;
                    
                    //pplan.ProgressPercentage = (pplan.Current * 100) / pplan.Total;

                    OnPlanProgress(this, pplan);

                    currents.Add(c);

                       //sem.Wait();                    

                    lock (locker)
                    {
                        Monitor.Enter(c);
                        c.Execute();
                    }
                   
                }
                catch (Exception ex)
                {
                    TaskProgressProgressChanged e = new TaskProgressProgressChanged(0, null);
                    e.Message = c.Description + " fallo: " + ex.Message;

                    OnUpgradeRepositoryProgressChanged(e);

                    sem.Release();
                }
            }

            _downloading = false;
            NotifyPropertyChanged("Downloading");
        }

        private void downloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            lock (locker)
            {
                Monitor.Pulse(locker);
            }
        }

        private void OnPlanProgress(object sender, PlanProgressEventArgs e)
        {
            if (PlanProgressChanged != null)
            {
                NotifyPropertyChanged("Progreso");

                PlanProgressChanged(this, e);
            }
        }


        private void OnCommandProgressChanged(DownloadAsyncProgressChangedEventArgs e)
        {
            TaskProgressProgressChanged v = new TaskProgressProgressChanged(e.ProgressPercentage, null);
            OnUpgradeRepositoryProgressChanged(v);
        }

        protected void OnCatalogCompareCompleted(CatalogoCompareCompletedEventArgs e)
        {
            if (CatalogoCompareCompleted != null)
            {
                CatalogoCompareCompleted(this, e);
            }
        }

        protected void OnUpgradeRepositoryCompleted(AsyncCompletedEventArgs e)
        {
            if (UpgradeRepositoryCompleted != null)
            {
                UpgradeRepositoryCompleted(this, e);
            }
        }

        public void OnUpgradeRepositoryProgressChanged(TaskProgressProgressChanged e)
        {
            if (UpgradeRepositoryProgressChanged != null)
            {          
                UpgradeRepositoryProgressChanged(this, e);
            }
        }

        public static  bool IsDefaultFolder(string folder)
        {
            DirectoryInfo d = new DirectoryInfo(folder);

            var x = from f in d.GetFiles().AsEnumerable() where f.Name == "arma3.exe" select f;

            return x.Count() > 0 ? true : false;
        }


        public int Progreso
        {
            get
            {
                return pplan.ProgressPercentage;
            }
        }

        public List<CommandBase> AllTasks
        {
            get { return TasksToDo;  }
            set { }
        }

        public void CloseAll()
        {
            //dcliente.Disconnect();
        }

        public List<string> GenerateModlist(string juego)
        {
            List<string> lines = new List<string>();

            RepositoryBay b = new RepositoryBay();

            DirectoryInfo rbay = b.GetDirectoryForRepo(nombre);
            FileInfo fi;
            
            fi = new FileInfo(Path.Combine(rbay.FullName, "ficheros.db4o"));

            //dcliente = new db4oDB(fi.FullName, "ficheros_cliente");
            //dcliente.ReadDB();

            
            List<DBData> todos = (from DBData file in dcliente select file).ToList<DBData>();

            var unicos = todos.Distinct(new ModComparer());


            
            foreach (DBData l in unicos)
            {
                string line = juego + "|" + l.Mod;
                lines.Add(line);
            }

            return lines;
        }

        public virtual void Dispose()
        {            
            //dcliente.Disconnect();
            dcliente = null;
            GC.SuppressFinalize(this);
        }
    }
}
