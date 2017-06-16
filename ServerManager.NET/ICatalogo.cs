using System;
using System.Collections.Generic;
using System.Text;

namespace ServerManager
{
    public interface ICatalogo<T>
    {
        void Load();                    // Carga del catalogo.
        void Write();                   // Guarda el catalogo.
        void Generate(string from);     // Genera el catalogo desde la carpeta especificada

        IEnumerable<T> Items();         // Devuelve los elementos del catalogo
        string Name();                  // Devuelve el nombre del Catalogo
        string BaseUrl();               // Devuelve la carpeta del Catalogo
    }
}
