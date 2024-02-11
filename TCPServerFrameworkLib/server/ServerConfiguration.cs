using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerFrameworkLib.server
{
    public class ServerConfiguration
    {
        public int ServerPort { get; set; }
        public int ShutdownPort { get; set; }
        public String ServerName { get; set; }
        public SourceLevels DebugLevel { get; set; }
        public String LogFilePath { get; set; }

        public ServerConfiguration() // default settings
        {
            ServerPort = 65000;
            ShutdownPort = ServerPort + 1;
            ServerName = "";
            DebugLevel = SourceLevels.Information;
            LogFilePath = ".";

        }
    }
}
