
using System.Net.Sockets;
using System.Net;
using System.Text;
using TestSocketClient;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

TcpClient client = new TcpClient();
//string host = Dns.GetHostName();

//// Getting ip address using host name
//IPHostEntry ip = Dns.GetHostEntry(host);
client.Connect("localhost", 123);
//NetworkStream stream = null;
//byte[] buffer = Encoding.ASCII.GetBytes("Hola mundo!");

//Console.WriteLine("Hola mundo!");
//Console.WriteLine(buffer);
//stream = client.GetStream();
//while (true)
//{
//    stream.Write(buffer, 0, buffer.Length);

//    //stream.ReadAsync(buffer, 0, buffer.Length).Wait();

//    Console.WriteLine("Data received");
//    Console.WriteLine(Encoding.ASCII.GetString(buffer));
//    await Task.Delay(TimeSpan.FromSeconds(5));
//}

//stream.Close();
//SslTcpClient.Main();

SslStream sslStream = new SslStream(
    client.GetStream(),
    false,
    ValidateServerCertificate,
    null
    );
try
{
    sslStream.AuthenticateAsClient("localhost");
    do
    {
        byte[] messsage = Encoding.UTF8.GetBytes("Hello world!<EOF>");
        // Send hello message to the server.
        sslStream.Write(messsage);
        sslStream.Flush();
        // Read message from the server.
        string serverMessage = await ReadMessageAsync(sslStream);
        Console.WriteLine("Server says: {0}", serverMessage);
        await Task.Delay(TimeSpan.FromSeconds(2));

    } while (true);
}
catch (Exception e)
{
    Console.WriteLine("Exception: {0}", e.Message);
    if (e.InnerException != null)
    {
        Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
    }
    Console.WriteLine("Authentication failed - closing the connection.");
    client.Close();
    return;
}









async Task<string> ReadMessageAsync(SslStream sslStream)
{
    // Read the  message sent by the server.
    // The end of the message is signaled using the
    // "<EOF>" marker.
    byte[] buffer = new byte[2048];
    StringBuilder messageData = new StringBuilder();
    int bytes = -1;
    do
    {
        bytes = await sslStream.ReadAsync(buffer, 0, buffer.Length);

        // Use Decoder class to convert from bytes to UTF8
        // in case a character spans two buffers.
        Decoder decoder = Encoding.UTF8.GetDecoder();
        char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
        decoder.GetChars(buffer, 0, bytes, chars, 0);
        messageData.Append(chars);
        // Check for EOF.
        if (messageData.ToString().IndexOf("<EOF>") != -1)
        {
            break;
        }
    } while (bytes != 0);

    return messageData.ToString();
}





// Acepta todo
bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
{
    return true;
}

