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
using csscript;
using MyDownloader.Core;
using MyDownloader.Extension.Protocols;


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

        
        double downloadprogress = 0;

        DownloadManager dman;
        //SemaphoreSlim sem;

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
            public TimeSpan Left { get; set; }

            public TaskProgressProgressChanged(int progressPercentage, object userState) : base(progressPercentage, userState)
            {
                Downloader dow = (Downloader)userState;
                Left = dow.Left;
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
            public string CurrentFilename;

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

            // ejecucion de scripts

            //WinWebDownload dld = new WinWebDownload("http://188.165.254.137/WebRepo", Repo, "/PreInstall", Bay.ToDirectoryInfo().FullName + @"/PreInstall");

            //dld.Download();

            //string preinstall = bay.GetDirectoryForRepo(Repo).FullName + @"\Preinstall";

            //var files = Directory.GetFiles(preinstall);

            //foreach (string csf in files)
            //{
            //    string content = File.ReadAllText(csf);
            //    dynamic script = CSScriptLibrary.CSScript.Evaluator.LoadCode(content);

            //    script.Execute();

            //}

            //

            FileTarget target = LogManager.Configuration.FindTargetByName<FileTarget>("logfile");
            string datemask = System.DateTime.Now.ToShortDateString().Replace("/", "_");
            datemask = datemask.Replace("/", "_");
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\" + "log_" + datemask + ".txt";
            target.FileName = filename;

            logger.Info(@"En Repository(string folder, RepositoryBay Bay, string Repo, List<ModView> Modlist)");

            _downloading = false;
            sem = new SemaphoreSlim(12);

            basepath = folder;
            bay = Bay;
            nombre = Repo;
            basedir = new DirectoryInfo(basepath);

            modlist = new ObservableCollection<Mod>();
            _serverMods = Modlist;

            DirectoryInfo baydir = bay.GetDirectoryForRepo(nombre);

            if (!baydir.Exists)
            {
                baydir.Create();
            }

            string jsonfile = Path.Combine(Bay.ToDirectoryInfo().FullName, Repo, "ficheros.json");

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

                logger.Info("Construccion Erronea.");

                logger.Info("=======================================================================================");
                logger.Info("dcliente : {0}", dcliente == null ? true : false);
                logger.Info("basepath : {0}", basepath);
                logger.Info("repo : {0}", nombre);
                logger.Info("bay : {0}", bay.ToString());
                logger.Info("basedir : {0}", basedir);

                logger.Info("========================================================================================");

                throw;
            }

            logger.Info("Construccion finalizada.");

            logger.Info("=======================================================================================");
            logger.Info("dcliente : {0}", dcliente == null ? true : false);

            if (dcliente != null)
            {
                logger.Info("{0} registros en dcliente", dcliente.Count);
            }

            logger.Info("basepath : {0}", basepath);
            logger.Info("repo : {0}", nombre);
            logger.Info("bay : {0}", bay.ToString());
            logger.Info("basedir : {0}", basedir);
            
            logger.Info("Contenido json");
            if (File.Exists(jsonfile)) { logger.Info("{0}", File.ReadAllText(jsonfile)); } else { logger.Info("No existe el fichero {0}", jsonfile); }
            logger.Info("========================================================================================");

        }

        public Repository(string folder, RepositoryBay Bay, string Repo)
        {
            FileTarget target = LogManager.Configuration.FindTargetByName<FileTarget>("logfile");
            string datemask = System.DateTime.Now.ToShortDateString().Replace("/", "_");
            datemask = datemask.Replace("/", "_");
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\" + "log_" + datemask + ".txt";
            target.FileName = filename;

            _downloading = false;
            sem = new SemaphoreSlim(12);

            basepath = folder;
            bay = Bay;
            nombre = Repo;
            basedir = new DirectoryInfo(basepath);

            DirectoryInfo baydir = bay.GetDirectoryForRepo(nombre);

            if (!baydir.Exists)
            {
                baydir.Create();
            }

            string jsonfile = Path.Combine(Bay.ToDirectoryInfo().FullName, Repo, "ficheros.json");

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

            }
            catch (Exception ex2)
            {
                logger.Fatal("Excepcion al abrir la base de datos : {0}", ex2.Message);

                logger.Info("Construccion erronea.");

                logger.Info("=======================================================================================");
                logger.Info("dcliente : {0}", dcliente == null ? true : false);

                if (dcliente != null)
                {
                    logger.Info("{0} registros en dcliente", dcliente.Count);
                }

                logger.Info("basepath : {0}", basepath);
                logger.Info("repo : {0}", nombre);
                logger.Info("bay : {0}", bay.ToString());
                logger.Info("basedir : {0}", basedir);

                logger.Info("========================================================================================");

                throw;
            }

            logger.Info("Construccion finalizada.");

            logger.Info("=======================================================================================");
            logger.Info("dcliente : {0}", dcliente == null ? true : false);

            if (dcliente != null)
            {
                logger.Info("{0} registros en dcliente", dcliente.Count);
            }

            logger.Info("basepath : {0}", basepath);
            logger.Info("repo : {0}", nombre);
            logger.Info("bay : {0}", bay.ToString());
            logger.Info("basedir : {0}", basedir);

            logger.Info("Contenido json");
            logger.Info("{0}", File.ReadAllText(jsonfile));
            logger.Info("========================================================================================");

        }

        public Repository(string folder, string Repo, string targetFolder, List<ModView> Modlist)
        {

           _downloading = false;
            sem = new SemaphoreSlim(12);

            FileTarget targetlogger = LogManager.Configuration.FindTargetByName<FileTarget>("logfile");
            string datemask = System.DateTime.Now.ToShortDateString().Replace("/", "_");
            datemask = datemask.Replace("/", "_");
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\12bdi_launcher\\" + "log_" + datemask + ".txt";
            targetlogger.FileName = filename;

            basepath = Path.Combine(folder, Repo);

            nombre = Repo;

            basedir = new DirectoryInfo(basepath);

            target = Path.Combine(targetFolder, nombre);
            targetdir = new DirectoryInfo(target);

            modlist = new ObservableCollection<Mod>();
            _serverMods = Modlist;

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
            }
            catch (Exception ex2)
            {
                logger.Fatal("Excepcion al abrir la base de datos : {0}", ex2.Message);

                logger.Info("Construccion erronea.");

                logger.Info("=======================================================================================");
                logger.Info("dcliente : {0}", dcliente == null ? true : false);

                if (dcliente != null)
                {
                    logger.Info("{0} registros en dcliente", dcliente.Count);
                }

                logger.Info("basepath : {0}", basepath);
                logger.Info("repo : {0}", nombre);
                logger.Info("target : {0}", target);
                logger.Info("basedir : {0}", basedir);

                logger.Info("========================================================================================");

                throw;
            }

            logger.Info("Construccion finalizada.");

            logger.Info("=======================================================================================");
            logger.Info("dcliente : {0}", dcliente == null ? true : false);

            if (dcliente != null)
            {
                logger.Info("{0} registros en dcliente", dcliente.Count);
            }

            logger.Info("basepath : {0}", basepath);
            logger.Info("repo : {0}", nombre);
            logger.Info("target : {0}", target);
            logger.Info("basedir : {0}", basedir);

            if (File.Exists(jsonfile))
            {
                logger.Info("Contenido json");
                logger.Info("{0}", File.ReadAllText(jsonfile));
                logger.Info("========================================================================================");
            }
            else
            {
                logger.Info("No existe ficheros.json");
                logger.Info("========================================================================================");
            }

        }


        public bool Downloading
        {
            get
            {
                if (TasksToDo != null)
                {
                    var tsk = from CommandBase b in TasksToDo where b.Finished == false select b;

                    return tsk.Count() > 0 ? true : false;
                }
                else
                {
                    return false;
                }
            }
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


        public void CatalogRepository(IProgress<CatalogModsProgress> progress = null)
        {
            /*
             * EJECUCION DE SCRIPTS              
             */


            //

            int tfiles = 0;
            int tskip = 0;
            int tnew = 0;
            
            //DirectoryInfo rbay = Bay.GetDirectoryForRepo(Repo);
            FileInfo fi;
            //bool db_previa = false;
            DirectoryInfo di = new DirectoryInfo(target);

            if (!di.Exists)
            {
                di.Create();
            }

            //fi = new FileInfo(target + @"\ficheros.db4o");

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

                    var exist = from d in dcliente where d.Ruta == (Path.GetDirectoryName(fichero.FullName).Replace(Path.GetFullPath(basedir.FullName), "")) && d.Nombre == fichero.Name && d.Tamano == fichero.Length select d;


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
                
                CatalogoCompletedEventArgs c = new CatalogoCompletedEventArgs(null, false, null);
                c.Total = tfiles;
                c.NewFiles = tnew;
                c.SkippedFiles = tskip;

                OnCreateRepositoryCompleted(c);
            }

            
            if (File.Exists(targetdir.FullName + @"\ficheros.json"))
            {
                File.Delete(targetdir.FullName + @"\ficheros.json");
            }

            File.WriteAllText(targetdir.FullName + @"\ficheros.json", JsonConvert.SerializeObject(dcliente));                
        }


        public static string xxHashSignature(string fichero)
        {
            try
            {
                string firma_base64 = "";

                xxHash firma = null;

                if (firma == null) { firma = new xxHash(64); }

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
                logger.Fatal("Error al calcular el Hash : {0}", x.Message);

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

                var Totales = dcliente.Count();
                var dataACero = from d in dcliente where d.Tamano == 0 select d;
                var ACero = dataACero.Count();

                logger.Info("CatalogFolderAsync terminado.");

                logger.Info("=======================================================================================");
                logger.Info("Total archivos catalogados : {0}", Totales);
                logger.Info("Total archivos con tamano cero : {0}", ACero);
                logger.Info("========================================================================================");

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

                //File.WriteAllText(bay.GetDirectoryForRepo(this.nombre).FullName + @"\ficheros.json", JsonConvert.SerializeObject(dcliente));

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
                //DirectoryInfo dipre = new DirectoryInfo(targetdir.FullName + d.Ruta + @"\PreInstall");
                //DirectoryInfo dipost = new DirectoryInfo(targetdir.FullName + d.Ruta + @"\PostInstall");

                if (!di.Exists)
                {
                    di.Create();

                    //dipre.Create();
                    //dipost.Create();
                }

                /*
                if (!dipre.Exists)
                {
                    dipre.Create();
                }

                if (!dipost.Exists)
                {
                    dipost.Create();
                }
                */

                FileInfo fi = new FileInfo(basedir.FullName + d.Ruta + @"\" +  d.Nombre);

                cur++;

                PublishModsProgress pm = new PublishModsProgress();

                pm.Mod = d.Mod;
                pm.Progress = (cur * 100) / total;
                pm.ProgressFile = cur;
                pm.TotalFiles = total;

                pr.Report(pm);

                try
                {
                    fi.CopyTo(Path.Combine(di.FullName, d.Nombre), true);
                }
                catch (Exception ex)
                {
                    logger.Error("====================================================================================================================");
                    logger.Error($"Error al copiar el archivo ({di.FullName}) - {ex.Message}");
                    logger.Error("====================================================================================================================");
                }
            }
        }

        public void CompareCatalogs(string nombre_bdd, string repo, string srv)
        {
            int tnis = 0;
            int tdsig = 0;
            int tnic = 0;

            MyDownloader.Extension.Protocols.HttpFtpProtocolExtension _protocol = new HttpFtpProtocolExtension(new MyParms());

            //ProtocolProviderFactory.RegisterProtocolHandler("http", typeof(MyDownloader.Extension.Protocols.HttpProtocolProvider));

            dman = new DownloadManager();


            Task t = new Task(() =>
               {
                   TasksToDo = new List<CommandBase>();

                   try
                   {

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

                       var notInCliente = (from DBData f in dservidor select f).Except(dcliente, new RutaComparer());
                       var distinctMD5 = (from DBData f in dservidor select f).Except(dcliente, new FirmaMD5Comparer()).Except(notInCliente);
                       var notInServer = (from DBData f in dcliente select f).Except(dservidor, new RutaComparer());

                       tnis = notInServer.Count();
                       tnic = notInCliente.Count();
                       tdsig = distinctMD5.Count();
                       var tczero = (from d in dcliente where d.Tamano == 0 select d).Count();
                       var tszero = (from d in dservidor where d.Tamano == 0 select d).Count();

                       logger.Info("CatalogRepoositoryAsync terminado.");

                       logger.Info("=======================================================================================");
                       logger.Info("Archivos en el cliente : {0}", dcliente.Count);
                       logger.Info("Archivos en el servidor : {0}", dservidor.Count);

                       logger.Info("Archivos que faltan en el cliente (Catalogados): {0}", tnic);
                       logger.Info("Archivos diferentes : {0}", tdsig);
                       logger.Info("Archivos eliminados en el servidor {0}", tnis);
                       logger.Info("Archivos en cliente con tamano cero {0}", tczero);
                       logger.Info("Archivos en servidor con tamano cero {0}", tszero);

                       logger.Info("========================================================================================");


                       // notInCliente -> Add to report
                       // notInServer -> Eliminate or Keep (User choice)
                       // distinctMD5 -> Update

                       dlist = new ObservableCollection<WebDownloadAction>();
                       List<DBData> toAdd = new List<DBData>();

                       dman.OnBeginAddBatchDownloads();

                       foreach (DBData r in notInCliente)
                       {
                           if (r.Tamano > 0)
                           {
                               string ruta = r.Ruta.Replace("\\\\", "/");
                               string nombre = r.Nombre;

                               //IProgress<int> p = new Progress<int>();
                               //DownloadFileCommand dc = new DownloadFileCommand(srv, repo, ruta + "/" + nombre, basepath + @"\" + ruta + @"\" + nombre, p);
                               //dc.Description = "Descargar " + nombre;

                               string url = srv + @"/" + repo + ruta.Replace("\\", "/") + @"/" + nombre;
                               string trg = basepath.Replace("/", "\\") + ruta + @"\" + nombre;

                               //queue.Add(url, trg);

                               ResourceLocation rl = ResourceLocation.FromURL(url);
                               Downloader dl = dman.Add(rl, new ResourceLocation[] { }, trg, 1, false);

                               dl.StateChanged += new EventHandler(dlchanged);
                               dl.SegmentStarted += new EventHandler<SegmentEventArgs>(segmentStarted);


                               //dcliente.Add(r);
                               //toAdd.Add(r);

                               logger.Info("Fichero {0} anadido para descarga ({1} bytes)", r.Nombre, r.Tamano);
                           }
                           //TasksToDo.Add(dc);
                       }

                       foreach (DBData r in distinctMD5)
                       {
                           if (r.Tamano > 0)
                           {
                               string ruta = r.Ruta;
                               string nombre = r.Nombre;

                               IProgress<int> p = new Progress<int>();
                               DeleteFileCommand df = new DeleteFileCommand(new FileInfo(basepath + @"\" + ruta + @"\" + nombre), p);

                               df.Description = "Elimininar " + nombre;

                               TasksToDo.Add(df);

                               //IProgress<int> p1 = new Progress<int>();
                               //DownloadFileCommand dc = new DownloadFileCommand(srv, repo, ruta + "/" + nombre, basepath + @"\" + ruta + @"\" + nombre, p1);

                               string url = srv + @"/" + repo + @"/" + ruta + @"/" + nombre;
                               string trg = basepath + @"\" + ruta + @"\" + nombre;

                               //queue.Add(url, trg);
                               ResourceLocation rl = ResourceLocation.FromURL(url);
                               Downloader dl = dman.Add(rl, new ResourceLocation[] { }, trg, 4, false);

                               dl.StateChanged += new EventHandler(dlchanged);


                               toAdd.Add(r);

                               var theRec = from DBData cli in dcliente where cli.Nombre == r.Nombre && cli.Ruta == r.Ruta select cli;

                               string firma = "";

                               if (theRec.Any())
                               {
                                   firma = theRec.ElementAt(0).Firma;
                               }

                               logger.Info("Fichero {0} modificado en cliente ( Firma Servidor: {1} - Firma Cliente : {2})", r.Nombre, r.Firma, firma);
                           }
                           else
                           {
                               string ruta = r.Ruta;
                               string nombre = r.Nombre;

                               IProgress<int> p = new Progress<int>();
                               DeleteFileCommand df = new DeleteFileCommand(new FileInfo(basepath + @"\" + ruta + @"\" + nombre), p);

                               df.Description = "Elimininar " + nombre;

                               logger.Warn("Archivo {0} marcado para eliminar porque en el servidor tiene tamano cero", nombre);

                               TasksToDo.Add(df);

                           }
                           //TasksToDo.Add(dc);
                       }

                       List<DBData> toRemove = new List<DBData>();


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

                                   logger.Info("Fichero {0} marcado para eliminacion", r.Nombre);

                                   df.Description = "Eliminar " + nombre;

                                   var res = from DBData f in dcliente where f.Ruta == r.Ruta && f.Nombre == r.Nombre select f;

                                   foreach (DBData o in res)
                                   {
                                       toRemove.Add(o);
                                   }

                                   TasksToDo.Add(df);
                               }
                           }
                           else
                           {
                               string ruta = r.Ruta;
                               string nombre = r.Nombre;

                               IProgress<int> p = new Progress<int>();
                               DeleteFileCommand df = new DeleteFileCommand(new FileInfo(basepath + @"\" + ruta + @"\" + nombre), p);

                               df.Description = "Eliminar " + nombre;

                               TasksToDo.Add(df);

                               var res = from DBData f in dcliente where f.Ruta == r.Ruta && f.Nombre == r.Nombre select f;

                               foreach (DBData o in res)
                               {
                                   toRemove.Add(o);
                               }                               
                           }
                       }

                       foreach (DBData r in toAdd)
                       {
                           dcliente.Add(r);
                       }

                       foreach (DBData d in toRemove)
                       {
                           dcliente.Remove(d);
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

                DirectoryInfo di = bay.GetDirectoryForRepo(nombre);

                if (File.Exists(di.FullName + @"\ficheros.json"))
                {
                    File.Delete(di.FullName + @"\ficheros.json");
                }

                File.WriteAllText(di.FullName + @"\ficheros.json", JsonConvert.SerializeObject(dcliente));

                OnCatalogCompareCompleted(c);
            });

            

            t.Start();
        }

        private void segmentStarted(object sender, MyDownloader.Core.SegmentEventArgs e)
        {
            Downloader d = (Downloader)e.Downloader;

            OnUpgradeRepositoryProgressChanged(new TaskProgressProgressChanged((int)d.Progress, d));
        }
        
        private void dlchanged(object sender, EventArgs e)
        {
            Downloader d = (Downloader)sender;

           
            if (d.State == DownloaderState.Working)
            {
                OnUpgradeRepositoryProgressChanged(new TaskProgressProgressChanged((int)d.Progress, d));
            }

            if (d.State == DownloaderState.Ended)
            {
                logger.Info("Finalizado {0}, {1} bytes descargados", d.ResourceLocation.URL, d.Transfered);
            }

        }
        private void deleteIfEmpty(string p)
        {
            foreach (var directory in Directory.GetDirectories(p))
            {
                deleteIfEmpty(directory);

                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }

        public void UpgradeRepositoryAsync()
        {
            
            
            
            pplan = new PlanProgressEventArgs(0, null);

            PlanProgressChangedEventHandler plan = new PlanProgressChangedEventHandler(OnPlanProgress);

            commandList = new List<Task>();

            executeTasks();

            //queue.StartAsync();


            dman.OnEndAddBatchDownloads();

            sem = new SemaphoreSlim(6);

            MyDownloader.Core.Settings.Default.MaxRetries = 2;

            int current = 0;
            int max = dman.Downloads.Count();

            foreach (Downloader dl in dman.Downloads)
            {
                sem.Wait();

                double per = (current * 100) / max;

                PlanProgressEventArgs ev = new PlanProgressEventArgs((int)per, null);
                ev.Total = max;
                ev.Current = current;
                ev.CurrentFilename = dl.LocalFile;

                OnPlanProgress(this, ev);


                current++;

                dl.Start();
                
                dl.WaitForConclusion();

                per = (current * 100) / max;

                ev = new PlanProgressEventArgs((int)per,null);
                ev.Total = max;
                ev.Current = current;
                
                OnPlanProgress(this,ev);

                sem.Release();
            }

            dman = null;
            

            OnUpgradeRepositoryCompleted(new AsyncCompletedEventArgs(null, false, null));

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
                    /*
                    TaskProgressProgressChanged e = new TaskProgressProgressChanged(0, null);
                    e.Message = c.Description;

                    OnUpgradeRepositoryProgressChanged(e);

                    if (c is DeleteFileCommand)
                    {
                        DeleteFileCommand df = (DeleteFileCommand)c;

                        df.DeleteFileCommandCompleted += (s, ev) =>
                        {
                            TaskProgressProgressChanged ez = new TaskProgressProgressChanged(0, null);
                            OnUpgradeRepositoryProgressChanged(ez);
                        };

                    }


                    
                    if (c is DownloadFileCommand)
                    {
                        DownloadFileCommand dc = (DownloadFileCommand)c;
                        dc.DownloadFileCommandCompleted += (s, ev) =>
                        {
                            AsyncCompletedEventArgs evento = (AsyncCompletedEventArgs)ev;

                            TaskProgressProgressChanged ez = new TaskProgressProgressChanged(0, null);

                            if (!evento.Cancelled)
                            {
                                ez.Message = c.Description + " termino correctamente";
                            }
                            else
                            {
                                ez.Message = c.Description + " ha fallado";
                            }
                            
                            OnUpgradeRepositoryProgressChanged(ez);

                            lock (locker)
                            {
                                Monitor.Pulse(locker);
                            }


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
                    */

                    //pplan.Current++;

                    //currents.Add(c);

                    c.Execute();

                    /*
                    lock (locker)
                    {
                        Monitor.Enter(c);
                        c.Execute();
                    }
                    */

                    //OnPlanProgress(this, pplan);

                    

                }
                catch (Exception ex)
                {
                    TaskProgressProgressChanged e = new TaskProgressProgressChanged(0, null);
                    e.Message = c.Description + " fallo: " + ex.Message;

                    lock (locker)
                    {
                        Monitor.Pulse(locker);
                    }

                    OnUpgradeRepositoryProgressChanged(e);


                    //sem.Release();
                }                
            }

                       
            //AsyncCompletedEventArgs eve = new AsyncCompletedEventArgs(null, false, null);
            //OnUpgradeRepositoryCompleted(eve);

            deleteIfEmpty(basepath);

            _downloading = false;
            NotifyPropertyChanged("Downloading");
        }

        /*
        private void downloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {

            var tsk = from CommandBase b in TasksToDo where b.Finished == false select b;

            if (tsk.Count() == 0)
            {
                AsyncCompletedEventArgs ev = new AsyncCompletedEventArgs(null, false, null);
                OnUpgradeRepositoryCompleted(ev);
            }

            lock (locker)
            {
                Monitor.Pulse(locker);
            }
        }
        */

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

        public double RepositoryDownloadProgress
        {
            get
            {
                return downloadprogress;
            }

            set
            {
                NotifyPropertyChanged("RepositoryDownloadProgress");
            }
        }

        /*
        public void DownloadModForced(string mod)
        {
            foreach (DBData d in modsInRepo)
            {
                if (d.Mod == mod)
                {
                    IProgress<int> p1 = new Progress<int>();
                    //DownloadFileCommand dc = new DownloadFileCommand(srv, nombre, ruta + "/" + nombre, basepath + @"\" + ruta + @"\" + nombre, p1);
                }
            }    
        }
        */
    }
}
