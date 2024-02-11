// See https://aka.ms/new-console-template for more information
using TryMyServer;

Console.WriteLine("Hello, World!");


//MyServer server = new MyServer(7, "ekko");
MyServer server = new MyServer(@"C:\Users\PELE\source\repos\TCPServerFramework\TryMyServer");
server.Start();