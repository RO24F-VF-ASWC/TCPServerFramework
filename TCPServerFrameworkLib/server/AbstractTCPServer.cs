using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerFrameworkLib.server
{
    /// <summary>
    /// An Template TCP server to set up the tcp server on a given port and with an given name 
    /// the server can be soft closed down using a stop server stated on the given port plus one
    /// </summary>
    public abstract class AbstractTCPServer
    {
        // instance fields
        private const int SEC = 1000;
        private readonly IPAddress ListOnIPAddress = IPAddress.Any;
        private bool running = true;

        //properties
        /// <summary>
        /// Get the port number the server is starting on
        /// </summary>
        public int PORT { get; private set; }
        /// <summary>
        /// Get the the port number of the stopping server
        /// </summary>
        public int STOPPORT { get; private set; }
        /// <summary>
        /// Get the name given to the server
        /// </summary>
        public String NAME { get; private set; }
        

        /// <summary>
        /// The constructor to template server initilyzing the port and name of the server
        /// </summary>
        /// <param name="port">The port number the server will start / listen on</param>
        /// <param name="name">The name of the server</param>
        protected AbstractTCPServer(int port, string name)
        {
            PORT = port;
            STOPPORT = port+1;
            NAME = name;
        }

        /// <summary>
        /// Starts the server, this include a stopserver  
        /// </summary>
        public void Start()
        {
            Task.Run(TheStopServer); // kort for Task.Run( ()=>{ TheStopServer(); });

            TcpListener listener = new TcpListener(ListOnIPAddress, PORT);
            listener.Start();
            Console.WriteLine($"Server {NAME} id started on {PORT}");

            while (running)
            {
                if (listener.Pending()) // der findes en klient
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client incoming");
                    Console.WriteLine($"remote (ip,port) = ({client.Client.RemoteEndPoint})");

                    Task.Run(() =>
                    {
                        TcpClient tmpClient = client;
                        DoOneClient(client);
                    });
                }
                else // der er PT ingen klient
                {
                    Thread.Sleep(2*SEC);
                }

            }
        }

        private void DoOneClient(TcpClient sock)
        {
            using (StreamReader sr = new StreamReader(sock.GetStream()))
            using (StreamWriter sw = new StreamWriter(sock.GetStream()))
            {
                sw.AutoFlush = true;
                Console.WriteLine("Handle one client");

                // her aktiveres the template method
                TcpServerWork(sr, sw);
            }

        }

        // the template method der skal implementeres

        /// <summary>
        /// This method implement what is specific for this server 
        /// e.g. if this is an echo server read from sr and write to sw
        /// </summary>
        /// <param name="sr">The streamreader from where you can read strings from the socket</param>
        /// <param name="sw">The streamwriter whereto you can write strings to the socket</param>
        protected abstract void TcpServerWork(StreamReader sr, StreamWriter sw);
        

        /*
         * stop server
         */
        private void StoppingServer()
        {
            running = false;
        }

        private void TheStopServer()
        {
            TcpListener listener = new TcpListener(ListOnIPAddress, STOPPORT);
            listener.Start();
            Console.WriteLine($"Server {NAME} id started on {STOPPORT}");
            TcpClient client = listener.AcceptTcpClient();
            //todo tjek om det er lovligt fx et password

            StoppingServer();
            listener.Stop(); // bare for at være pæn - det hele lukker alligevel
        }

    }
}
