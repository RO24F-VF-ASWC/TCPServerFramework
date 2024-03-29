﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerFrameworkLib.server;

namespace TryMyServer
{
    public class MyServer : AbstractTCPServer
    {
        /// <summary>
        /// Create an echo server listen on port having the name 
        /// </summary>
        /// <param name="port">The port number of this server</param>
        /// <param name="name">The name of this server</param>
        public MyServer(int port, string name) : base(port, name)
        {
        }

        /// <summary>
        /// Create an echo server with configuration from a file
        /// </summary>
        /// <param name="configFilePath">The path to the config file</param>
        public MyServer(string configFilePath) : base(configFilePath)
        {
        }

        protected override void TcpServerWork(StreamReader sr, StreamWriter sw)
        {
            // echo
            sw.WriteLine(sr.ReadLine());
        }
    }
}
