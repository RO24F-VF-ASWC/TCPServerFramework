// See https://aka.ms/new-console-template for more information
using TryMyServer;

Console.WriteLine("Hello, World!");


MyServer server = new MyServer(7, "ekko");
server.Start();