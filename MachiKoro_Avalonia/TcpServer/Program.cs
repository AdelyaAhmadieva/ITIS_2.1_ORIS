using TCPServer;
Console.Title = "XServer";

var server = new JServer();
await server.StartAsync();
server.AcceptClients();