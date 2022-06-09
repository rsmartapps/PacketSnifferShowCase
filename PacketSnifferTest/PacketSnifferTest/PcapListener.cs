using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketSnifferTest
{
    public class PcapListener
    {
        public string IP { get; set; }
        public int Port { get; set; }
        LibPcapLiveDevice deviceOut;
        public PcapListener(LibPcapLiveDevice device, string ip, int port)
        {
            deviceOut = device;

            IP = ip;
            Port = port;
            deviceOut.Open();
            deviceOut.Filter = $"ip and port {port}";
            //deviceOut.Filter = $"ip net {ip} and port {port}";
            deviceOut.OnPacketArrival += new PacketArrivalEventHandler(OnPaketOut);
            deviceOut.StartCapture();
            //deviceOut.Open();
            //deviceOut.Filter = $"src net {ip}";
            //deviceOut.OnPacketArrival += new PacketArrivalEventHandler(OnPaketIn);
            //deviceOut.StartCapture();
            Console.WriteLine($"Listening for packets for filter: {deviceOut.Filter}");
        }

        private void OnPaketOut(object s, PacketCapture e)
        {
            var packet = Packet.ParsePacket(e.Device.LinkType, e.Data.ToArray());
            Console.WriteLine($"Packet Info:");
            if(packet.PayloadPacket is IPv4Packet ipv4Packet)
            {
                Console.WriteLine($"Source: {ipv4Packet.SourceAddress} and Destination: {ipv4Packet.DestinationAddress}");
                InternalPaloadPacket(packet);

            }
            Console.WriteLine();
        }

        private bool InternalPaloadPacket(Packet packet)
        {
            if (packet.HasPayloadData)
            {
                Console.WriteLine("Data payload");
                foreach (var bite in packet.Bytes)
                {
                    Console.Write(bite + " ");
                }
                try
                {
                    Console.WriteLine();
                    Console.WriteLine("Try parse to string bytes without decrypting");
                    Console.WriteLine(Encoding.ASCII.GetString(packet.Bytes));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine();
                //Console.WriteLine(Encoding.ASCII.GetString(packet.PayloadData));
                return true;
            }
            else if (packet.HasPayloadPacket)
            {
                Console.WriteLine("Packate Data");
                return InternalPaloadPacket(packet.PayloadPacket);
            }
            return false;
        }
        private void OnPaketIn(object s, PacketCapture e)
        {

            Console.WriteLine($"Packet IN on ip: {IP}");
            Console.WriteLine($"Header timeval: {e.Header.Timeval}");
            foreach (var bite in e.Data)
            {
                Console.Write(bite + " ");
            }
            Console.WriteLine();
        }

    }

    class PacketRequestContent
    {
        //size 16
        public byte[] Header;
        public byte[] Payload;
    }
    class PacketResponseContent
    {
        // size 15
        public byte[] Header;
        // size 5
        public byte[] Payload;
    }


}
