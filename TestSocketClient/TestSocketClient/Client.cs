using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestSocketClient
{
    public class Client
    {
        public async Task RunAsync()
        {
            TcpClient client = new TcpClient();
            string host = Dns.GetHostName();

            // Getting ip address using host name
            client.Connect(host, 1230);

            var stream = client.GetStream();
            try
            {
                do
                {
                    byte[] messsage = Encoding.UTF8.GetBytes("Hello world!<EOF>");
                    // Send hello message to the server.
                    stream.Write(messsage);
                    stream.Flush();
                    // Read message from the server.
                    string serverMessage = await ReadMessageAsync(stream);
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
        }

        async Task<string> ReadMessageAsync(Stream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes;
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
    }
}
