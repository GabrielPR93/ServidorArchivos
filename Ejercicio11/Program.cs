using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio11
{
    class Program
    {
        static void Main(string[] args)
        {
            ServidorArchivos servidor = new ServidorArchivos();
            //servidor.leeArchivo("prueba.txt", 2);
            servidor.iniciaServidorArchivos();
        }
    }
}
