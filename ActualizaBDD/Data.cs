using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace ActualizaBDD
{
    public class Data
    {

        // Datable
        DataTable cliente;
        DataSet ds;

        SQLiteConnection con;
        SQLiteCommandBuilder cb;
        SQLiteDataAdapter adp;
        SQLiteCommand cmd;

        private string nombre_bdd;
        private string query;
        private string tbname;

        public Data(string db, string tablename)
        {
            nombre_bdd = db;
            tbname = tablename;

            //
            // 
            query = "select mod,ruta, nombre, firma from " + tbname;

            con = new SQLiteConnection("Data Source=" + nombre_bdd);
            
            cmd = new SQLiteCommand(query, con);
            adp = new SQLiteDataAdapter();
            adp.SelectCommand = cmd;
            cb = new SQLiteCommandBuilder(adp);


            con.Open();

            hacer_datatable();

            
        }

        private void hacer_datatable()
        {
            SQLiteCommand c = new SQLiteCommand(con);
            // 
            c.CommandText = "create table if not exists " + tbname + " (mod text, ruta text, nombre text, firma, tamano unsigned bigint)";
            c.ExecuteNonQuery();

            cliente = new DataTable(tbname);

            DataColumn colmod = new DataColumn("mod", typeof(string));
            DataColumn colruta = new DataColumn("ruta", typeof(string));
            DataColumn colnombre = new DataColumn("nombre", typeof(string));
            DataColumn colfirma = new DataColumn("firma", typeof(string));
            DataColumn coltamano = new DataColumn("tamano", typeof(System.Int32));


            cliente.Columns.Add(colmod);
            cliente.Columns.Add(colruta);
            cliente.Columns.Add(colnombre);
            cliente.Columns.Add(colfirma);
            cliente.Columns.Add(coltamano);

            ds = new DataSet();
            ds.Tables.Add(cliente);
            
            

        }

        public void insert(string mod, string nombre, string ruta, string firma, long tamano)
        {
            DataRow row = cliente.NewRow();

            row["mod"] = mod;
            row["ruta"] = ruta;
            row["nombre"] = nombre;
            row["firma"] = firma;
            row["tamano"] = tamano;


            cliente.Rows.Add(row);

        }

        public DataTable Table
        {
            get { return cliente; }
        }

        public void readDB()
        {
            
            adp.Fill(cliente);
        }

        public void writeDB()
        {
            using (var t = con.BeginTransaction())
            {
                adp.InsertCommand = cb.GetInsertCommand();

                adp.Update(cliente);

                t.Commit();
            }
        }

        ~Data()
        {
            //con.Close();
        }
    }

    public class FirmaMD5Comparer : IEqualityComparer<DataRow>
    {
        public bool Equals(DataRow a, DataRow b)
        {
            if (a.Field<string>("firma") == b.Field<string>("firma"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(DataRow d)
        {
            return d.Field<string>("firma").GetHashCode();
        }
    }

    public class ModComparer : IEqualityComparer<DataRow>
    {
        public bool Equals(DataRow a, DataRow b)
        {
            if (a.Field<string>("mod") == b.Field<string>("mod"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(DataRow d)
        {
            return d.Field<string>("mod").GetHashCode();
        }
    }
    

    public class RutaComparer : IEqualityComparer<DataRow>
    {
        
        public bool Equals(DataRow a, DataRow b)
        {
            if (Object.ReferenceEquals(a, b)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
                return false;

            string pathA = a.Field<string>("ruta") + @"\" + a.Field<string>("nombre");
            string pathB = b.Field<string>("ruta") + @"\" + b.Field<string>("nombre");

            return pathA == pathB;
        }

        public int GetHashCode(DataRow d)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(d, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashRuta= d.Field<string>("ruta").ToString() == null ? 0 : d.Field<string>("ruta").ToString().GetHashCode();

            //Get hash code for the Code field.
            int hashNombre = d.Field<string>("nombre").ToString().GetHashCode();

            //Calculate the hash code for the product.
            return hashRuta ^ hashNombre;

        }
    }

    public class ModData
    {
        
        public bool Selected { get; set; }
        public string Mod { get; set; }
        public int Arma { get; set; }
    }
}
