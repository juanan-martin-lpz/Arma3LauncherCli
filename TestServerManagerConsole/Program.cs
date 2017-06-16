using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerManager;

namespace TestServerManagerConsole
{
    class Program
    {
        static CatalogoLocal localCat;

        static void Main(string[] args)
        {
            localCat = new CatalogoLocal("Local",@"E:\ModsLocal", "catalogo.json", true);

            localCat.Load();

            //localCat.Generate(@"E:\Mods3\Arma3Minimo");

            Console.WriteLine("Working.....");

            foreach (ItemCatalogo i in localCat.Items())
            {
                Console.WriteLine("{0}", i.Name);

                foreach (ItemCatalogo it in i.Items)
                {
                    Console.WriteLine("         {0} - {1} => {2}", it.Name, it.Size, it.Hash);
                }
            }
            //
            //localCat.Write();
            //
            Console.WriteLine("Terminado");
            Console.ReadKey();
        }
    }
}
