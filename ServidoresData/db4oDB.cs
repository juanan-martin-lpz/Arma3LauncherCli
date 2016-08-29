using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using System.IO;

namespace ServidoresData
{
    public class db4oDB
    {

        private IObjectContainer _dbContainer;
        private Db4objects.Db4o.Config.IConfiguration _config;
        private IDb4oLinqQuery<DBData> data;



        private string nombre_bdd;
        private string tbname;

        public db4oDB(string db, string tablename,bool Config = true)
        {
            nombre_bdd = db;
            tbname = tablename;

            try
            {
                _config = Db4objects.Db4o.Db4oFactory.Configure();
                _config.AutomaticShutDown(true);
                _config.ActivationDepth(3);
                _config.UpdateDepth(3);
                _config.Add(new Db4objects.Db4o.TA.TransparentActivationSupport());
                _config.LockDatabaseFile(false);

                _dbContainer = Db4objects.Db4o.Db4oFactory.OpenFile(db);

            }
            catch
            {
                throw;
            }

        }

        public IObjectContainer Container
        {
            get { return _dbContainer;  }
        }

        public void Open()
        {
            try
            {
                _dbContainer = Db4objects.Db4o.Db4oFactory.OpenFile(nombre_bdd);
            }
            catch
            {
                throw;
            }

        }

        public void ReadDB()
        {
            if (_dbContainer != null)
            {
                try
                {

                    data = from DBData d in _dbContainer select d;

                }
                catch
                {
                    throw;
                }
            }
        }

        public void Remove(Object Target)
        {
            try
            {
                _dbContainer.Delete(Target);
            }
            catch
            {
                throw;
            }
        }

        public bool existObject(string ruta, string nombre, long len)
        {

            try
            {

                //var todos = from DBData d in _dbContainer select d;

                var q = from DBData d in _dbContainer where d.Ruta == ruta && d.Nombre == nombre && d.Tamano == len select d;

                if (q.Count() > 0) { return true; } else { return false; }

            }
            catch
            {
                
                //_config.
                throw;
            }
        }

        public void Save(object Target)
        {
            if (_dbContainer != null)
            {
                _dbContainer.Store(Target);
                
            }

        }

        public void Disconnect()
        {
            if (_dbContainer != null)
            {
                _dbContainer.Close();
                _dbContainer.Dispose();
            }
        }

        public object Query(System.Linq.Expressions.Expression LinqQuery)
        {
            if (LinqQuery != null)
            {
                var result = LinqQuery;
                return result;
            }
            else
            {
                return null;
            }
        }

        public static void Defragment(string path, string backup)
        {

            FileInfo original = new FileInfo(path);

            if (original.Exists)
            {
                try
                {
                    Db4objects.Db4o.Defragment.DefragmentConfig cfg = new Db4objects.Db4o.Defragment.DefragmentConfig(path, backup);
                    Db4objects.Db4o.Defragment.Defragment.Defrag(cfg);
                }
                catch (Db4objects.Db4o.Ext.InvalidIDException ex)
                {

                }
                catch (System.IO.IOException)
                {
                    FileInfo bak = new FileInfo(backup);

                    if (bak.Exists)
                    {
                        bak.Delete();
                    }

                    db4oDB.Defragment(path, backup);
                }
                catch
                {
                    throw;
                }

                FileInfo fi = new FileInfo(backup);

                if (fi.Exists)
                {
                    fi.Delete();
                }
            }
        }
    }
}
