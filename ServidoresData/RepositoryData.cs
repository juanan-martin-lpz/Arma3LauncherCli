/*
 * Creado por SharpDevelop.
 * Usuario: Juan
 * Fecha: 11/19/2015
 * Hora: 12:10
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
//using System.Data.SQLite;


namespace ServidoresData
{
	/// <summary>
	/// Description of RepositoryData.
	/// </summary>
    /*
	public class RepositoryData
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

		public RepositoryData(string db, string tablename)
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

		~RepositoryData()
		{
			//con.Close();
		}
	}
	*/

	public class FirmaMD5Comparer : IEqualityComparer<DBData>
	{
        public bool Equals(DBData a, DBData b)
        {
            //Check whether the objects are the same object. 
            if (Object.ReferenceEquals(a, b)) return true;

            // ((a.Ruta == b.Ruta) && (a.Nombre == b.Nombre) && (a.Firma == b.Firma))
            if (a.Firma == b.Firma)
            {
                return true;
            }
            else
            {
                Console.WriteLine(a.Ruta + "|" + b.Ruta + " - " + a.Nombre + "|" + b.Nombre + " - " + a.Firma + "|" + b.Firma);
                return false;
            }
        }

        public int GetHashCode(DBData obj)
        {
            int hashFirma = (obj.Firma.GetHashCode() ^ obj.Ruta.GetHashCode() ^ obj.Nombre.GetHashCode());
            return hashFirma;
        }

        
	}

	public class ModComparer : IEqualityComparer<DBData>
	{
		public bool Equals(DBData a, DBData b)
		{
			if (a.Mod == b.Mod)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public int GetHashCode(DBData d)
		{
			return d.Mod.GetHashCode();
		}
	}
	

	public class RutaComparer : IEqualityComparer<DBData>
	{
		
		public bool Equals(DBData a, DBData b)
		{
			if (Object.ReferenceEquals(a, b)) return true;

			//Check whether any of the compared objects is null.
			if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
				return false;

			string pathA = a.Ruta + @"\" + a.Nombre;
			string pathB = b.Ruta + @"\" + b.Nombre;

			if ((a.Ruta == b.Ruta) && (a.Nombre == b.Nombre))
            {
                return true;
            }
            else
            {
                return false;
            }

			//return pathA == pathB;
		}

		public int GetHashCode(DBData d)
		{
			//Check whether the object is null
			if (Object.ReferenceEquals(d, null)) return 0;

			//Get hash code for the Name field if it is not null.
			int hashRuta= d.Ruta == null ? 0 : d.Ruta.GetHashCode();

			//Get hash code for the Code field.
			int hashNombre = d.Nombre.GetHashCode();

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
