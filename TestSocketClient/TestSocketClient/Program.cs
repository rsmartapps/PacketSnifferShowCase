
using TestSocketClient;



var client = new Client();
var sslClient = new SslTcpClient();

var task = client.RunAsync();

var task2 = sslClient.RunAsync();

Task.WaitAll(task, task2);