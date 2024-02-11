using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;

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
        private readonly List<Task> clients = new List<Task>();


        protected TraceSource _trace;
        protected int _traceId;
        protected const String CONFIG_FILE = "TCPConfigFile.xml";


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
         
      

        /*
         * Different constructors
         * 
         * one for port & name, one for path to XML config-file and one for an configuration object
         * 
         */
        

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

            _trace = new TraceSource(NAME);
            SetUpTracing(SourceLevels.All, name + "-server");
        }


        /// <summary>
        /// Constructor supporting reading a configuration File
        /// </summary>
        /// <param name="configFilePath">The path to the configurationfile</param>
        public AbstractTCPServer(String configFilePath)
        {
            ServerConfiguration conf = new ServerConfiguration();

            String fullConfigFilename = configFilePath + @"\" + CONFIG_FILE;
            if (!File.Exists(fullConfigFilename))
            {
                throw new FileNotFoundException(fullConfigFilename);
            }

            XmlDocument configDoc = new XmlDocument();
            configDoc.Load(fullConfigFilename);

            /*
             * Read Serverport
             */
            XmlNode? portNode = configDoc.DocumentElement.SelectSingleNode("ServerPort");
            if (portNode != null)
            {
                String str = portNode.InnerText.Trim();
                conf.ServerPort = Convert.ToInt32(str);
            }

            /*
             * Read Shutdown port
             */
            XmlNode? sdportNode = configDoc.DocumentElement.SelectSingleNode("StopServerPort");
            if (sdportNode != null)
            {
                String str = sdportNode.InnerText.Trim();
                conf.ShutdownPort = Convert.ToInt32(str);
            }

            /*
             * Read server name
             */
            XmlNode? nameNode = configDoc.DocumentElement.SelectSingleNode("ServerName");
            if (nameNode != null)
            {
                conf.ServerName = nameNode.InnerText.Trim();
            }

            /*
             * Read Debug Level
             */
            XmlNode? debugNode = configDoc.DocumentElement.SelectSingleNode("DebugLevel");
            if (debugNode != null)
            {
                string str = debugNode.InnerText.Trim();
                SourceLevels level = SourceLevels.All;
                SourceLevels.TryParse(str, true, out level);
                conf.DebugLevel = level;
            }

            /*
             * Read Log Files location
             */
            XmlNode? logFilesNode = configDoc.DocumentElement.SelectSingleNode("LogFilesPath");
            if (logFilesNode != null)
            {
                conf.LogFilePath = logFilesNode.InnerText.Trim();
            }

            SetupAbstractTCPServer(conf);
        }

        /// <summary>
        /// Constructor supporting reading a configuration File
        /// </summary>
        /// <param name="configFilePath">The name of the configurationfile</param>
        public void SetupAbstractTCPServer(ServerConfiguration conf)
        {
            PORT = conf.ServerPort;
            STOPPORT = conf.ShutdownPort;
            NAME = conf.ServerName;

            _trace = new TraceSource(NAME);
            SetUpTracing(conf.DebugLevel, conf.LogFilePath);
        }


        /*
         * Help for setting up tracing
         */
        private void SetUpTracing(SourceLevels level, String filename)
        {
            _traceId = PORT;

            _trace.Switch = new SourceSwitch(NAME + "trace", level.ToString());

            _trace.Listeners.Add(new ConsoleTraceListener());

            TraceListener txtLog = new TextWriterTraceListener(filename + "-Log.txt");
            _trace.Listeners.Add(txtLog);

            TraceListener xmlLog = new XmlWriterTraceListener(filename + "-Log.xml");
            _trace.Listeners.Add(xmlLog);


        }


        /*
         * Code for the server
         */



        /// <summary>
        /// Starts the server, this include a stopserver  
        /// </summary>
        public void Start()
        {
            Task.Run(TheStopServer); // kort for Task.Run( ()=>{ TheStopServer(); });

            TcpListener listener = new TcpListener(ListOnIPAddress, PORT);
            listener.Start();
            _trace.TraceEvent(TraceEventType.Warning, _traceId, $"Server {NAME} is started on {PORT}");

            while (running)
            {
                if (listener.Pending()) // der findes en klient
                {
                    TcpClient client = listener.AcceptTcpClient();
                    _trace.TraceEvent(TraceEventType.Information, _traceId, "Client incoming");
                    _trace.TraceEvent(TraceEventType.Information, _traceId, ($"remote (ip,port) = ({client.Client.RemoteEndPoint})"));

                    clients.Add(
                        Task.Run(() =>
                            {
                                TcpClient tmpClient = client;
                                DoOneClient(client);
                            })
                        );
                }
                else // der er PT ingen klient
                {
                    Thread.Sleep(2*SEC);
                }

            }
            // vente på alle task bliver færdige
            Task.WaitAll(clients.ToArray());

            _trace.TraceEvent(TraceEventType.Warning, _traceId, $"Server {NAME} is stoped on {PORT}");
            _trace.Close();
        }

        private void DoOneClient(TcpClient sock)
        {
            using (StreamReader sr = new StreamReader(sock.GetStream()))
            using (StreamWriter sw = new StreamWriter(sock.GetStream()))
            {
                sw.AutoFlush = true;
                _trace.TraceEvent(TraceEventType.Information, _traceId, "Handle one client");

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
            _trace.TraceEvent(TraceEventType.Warning, _traceId, $"the Server {NAME} - stop server started on {STOPPORT}");
            TcpClient client = listener.AcceptTcpClient();
            //todo tjek om det er lovligt fx et password

            StoppingServer();
            listener.Stop(); // bare for at være pæn - det hele lukker alligevel
        }

    }
}
