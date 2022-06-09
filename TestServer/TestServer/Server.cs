using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    public class Server
    {

        public void Run()
        {
            string host = Dns.GetHostName();
            TcpListener server = new TcpListener(IPAddress.Any, 1230);
            server.Start();

            while (true)
            {
                Console.WriteLine("Waiting for a client to connect...");
                // Application blocks while waiting for an incoming connection.
                // Type CNTL-C to terminate the server.
                TcpClient client = server.AcceptTcpClient();
                ProcessClient(client);
            }
        }


    //while (true)
    //{
    //    var clientConnection = server.AcceptSocket();

    //    List<byte[]> data = new List<byte[]>();
    //    byte[] bytes = new byte[1024];
    //    while (true)
    //    {
    //        if( clientConnection.Receive(bytes) > 0)
    //        {
    //            Console.WriteLine(Encoding.ASCII.GetString(bytes));
    //            //clientConnection.Send(bytes);
    //        }
    //        else
    //        {
    //            Console.WriteLine("No data received");
    //        }
    //    }
    //    clientConnection.Close();
    //}

    void ProcessClient(TcpClient client)
    {
        // A client has connected. Create the
        // SslStream using the client's network stream.
        Stream stream = client.GetStream();
        // Authenticate the server but don't require the client to authenticate.
        try
        {

            // Display the properties
            DisplayStreamProperties(stream);

            // Set timeouts for the read and write to 5 seconds.
            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;
            do
            {
                // Read a message from the client.
                Console.WriteLine("Waiting for client message...");
                string messageData = ReadMessage(stream);
                Console.WriteLine("Received: {0}", messageData);

                // Write a message to the client.
                byte[] message = Encoding.UTF8.GetBytes(messageData);
                Console.WriteLine("Sending hello message.");
                stream.Write(message);
                stream.Flush();
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
            stream.Close();
            client.Close();
            return;
        }
        finally
        {
            // The client stream will be closed with the sslStream
            // because we specified this behavior when creating
            // the sslStream.
            stream.Close();
            client.Close();
        }
    }
    string ReadMessage(Stream sslStream)
    {
        // Read the  message sent by the client.
        // The client signals the end of the message using the
        // "<EOF>" marker.
        byte[] buffer = new byte[2048];
        StringBuilder messageData = new StringBuilder();
        int bytes;
        do
        {
            // Read the client's test message.
            bytes = sslStream.Read(buffer, 0, buffer.Length);

            // Use Decoder class to convert from bytes to UTF8
            // in case a character spans two buffers.
            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            decoder.GetChars(buffer, 0, bytes, chars, 0);
            messageData.Append(chars);
            // Check for EOF or an empty message.
            if (messageData.ToString().IndexOf("<EOF>") != -1)
            {
                break;
            }
        } while (bytes != 0);

        return messageData.ToString();
    }
    void DisplayStreamProperties(Stream stream)
    {
        Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
        Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
    }
    void DisplayUsage()
    {
        Console.WriteLine("To start the server specify:");
        Console.WriteLine("serverSync certificateFile.cer");
        Environment.Exit(1);
    }
}
}
