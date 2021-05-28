using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ejercicio11
{
    class ServidorArchivos
    {
        private bool conexion = true;
        private Socket s;
        public static readonly object l = new object();
        public string leeArchivo(string nombreArchivo, int nLineas)
        {
            int cont = 0;
            string texto = "";
            string linea;

            try
            {

                using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("EXAMEN") + "\\" + nombreArchivo))
                {

                    while ((linea = sr.ReadLine()) != null)
                    {
                        cont++;
                        if (cont <= nLineas)
                        {
                            texto += linea + "\r\n";

                        }
                    }

                }
            }
            catch (IOException)
            {
                Console.WriteLine("Error al acceder al archivo");
                texto = "<ERROR_IO>";


            }
            return texto;

        }

        public int leePuerto()
        {
            int puerto;
            try
            {
                puerto = Convert.ToInt32(leeArchivo("puerto.txt", 1));

                if (puerto >= IPEndPoint.MinPort && puerto <= IPEndPoint.MaxPort)
                {
                    return puerto;
                }
                else
                {
                    puerto = 31416;
                }
            }
            catch (FormatException) //EVITAR EXCEPTION
            {
                Console.WriteLine("Error de formato");
                puerto = 31416;
            }

            return puerto;
        }

        public void guardaPuerto(int numero)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("EXAMEN") + "/puerto.txt"))
                {
                    sw.WriteLine(numero);

                }
            }
            catch (IOException)
            {

                Console.WriteLine("Error en archivo");
            }

        }

        public string listaArchivos()
        {

            string archivos = "";
            string[] files = Directory.GetFiles(Environment.GetEnvironmentVariable("EXAMEN"));

            foreach (string item in files)
            {
                if (Path.GetExtension(item) == ".txt")
                {
                    archivos += Path.GetFileName(item) + "\r\n"; //tambien se puede usar Enviroment.NewLine en lugar de \r\n

                }
            }


            return archivos;
        }


        public void iniciaServidorArchivos()
        {
            int puerto = leePuerto();

            IPEndPoint ie = new IPEndPoint(IPAddress.Any, puerto);
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                s.Bind(ie);
                s.Listen(3);
                Console.WriteLine("Conectado al puerto: " + ie.Port);
            }
            catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine("Puerto ocupado");
                conexion = false;

            }

            while (conexion)
            {
                lock (l)
                {
                    if (conexion)
                    {
                        try
                        {

                            Socket sCliente = s.Accept();
                            Thread hilo = new Thread(hiloCliente);
                            hilo.Start(sCliente);
                        }
                        catch (SocketException)
                        {

                            conexion = false;
                        }
                    }
                }
            }
        }

        public void hiloCliente(object socket)
        {
            string mensaje;
            bool flag = true;
            bool msgConexion = true;
            string opcion;
            string restoMensaje = "";
            Socket sCliente = (Socket)socket;
            IPEndPoint ieCliente = (IPEndPoint)sCliente.RemoteEndPoint;
            Console.WriteLine("Cliente {0} conectado en el puerto {1}", ieCliente.Address, ieCliente.Port);

            while (flag)
            {
                try
                {

                    using (NetworkStream ns = new NetworkStream(sCliente))
                    using (StreamReader sr = new StreamReader(ns))
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        if (msgConexion)
                        {
                            sw.WriteLine("CONEXION ESTABLECIDA");
                            sw.Flush();
                            msgConexion = false;
                        }


                        mensaje = sr.ReadLine();                                                   //GET archivo,n:
                                                                                                   //PORT numero:
                        if (mensaje != null)                                                       //LIST:
                        {
                            try
                            {

                                if (mensaje.IndexOf(" ") != -1) //Compruebo si hay un espacio
                                {
                                    string[] opciones = mensaje.Split(' ');
                                    opcion = opciones[0];
                                    restoMensaje = opciones[1];

                                }
                                else
                                {
                                    opcion = mensaje;

                                }
                                switch (opcion)
                                {
                                    case "GET":
                                        try
                                        {
                                            if (restoMensaje.IndexOf(",") != -1)
                                            {
                                                string[] partes = restoMensaje.Split(',');
                                                string archivo = partes[0];
                                                int num = Convert.ToInt32(partes[1]);

                                                sw.WriteLine(leeArchivo(archivo, num));
                                                sw.Flush();

                                            }

                                        }
                                        catch (FormatException e)
                                        {

                                            Console.WriteLine(e.Message);
                                        }
                                        catch (OverflowException e)
                                        {

                                            Console.WriteLine(e.Message);
                                        }
                                        break;
                                    case "PORT":

                                        guardaPuerto(Convert.ToInt32(restoMensaje));
                                        sw.WriteLine("Puerto guardado correctamente");
                                        sw.Flush();

                                        break;
                                    case "LIST":

                                        sw.WriteLine(listaArchivos());
                                        sw.Flush();

                                        break;
                                    case "CLOSE":

                                        sw.WriteLine("Hasta pronto");
                                        sw.Flush();
                                        sCliente.Close();
                                        flag = false;

                                        break;
                                    case "HALT":
                                        s.Close();
                                        flag = false;
                                        lock (l)
                                        {
                                            conexion = false;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (ArgumentOutOfRangeException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }


                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);

                }
            }

        }


    }
}
