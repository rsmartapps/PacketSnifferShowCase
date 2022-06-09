using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TestServer;

var server = new Server();
var sslServer = new SSLServer();
server.Run();
sslServer.Run();