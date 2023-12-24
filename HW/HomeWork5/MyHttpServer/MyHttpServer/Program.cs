using System.Net;
using MyHttpServer;

await new HttpServer(new HttpListener()).Start();