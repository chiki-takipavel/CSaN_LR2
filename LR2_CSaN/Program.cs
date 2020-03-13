using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LR2_CSaN
{
    class Program
    {
        const int TYPE_ECHO_REQUEST = 8;

        static void Main(string[] args)
        {
            byte[] message = new byte[1024];

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            string name = Console.ReadLine();
            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(name);
                IPEndPoint destIPEndPoint = new IPEndPoint(ipHostEntry.AddressList[0], 0);

                message = Encoding.ASCII.GetBytes("ICMP packet!");
                ICMPPacket packet = new ICMPPacket(TYPE_ECHO_REQUEST, message);

                Traceroute(socket, packet, destIPEndPoint);
            }
            catch (SocketException)
            {
                Console.WriteLine("Не удается разрешить системное имя узла {0}.", name);
            }            
            socket.Close();
        }

        static void Traceroute(Socket socket, ICMPPacket packet, IPEndPoint destIPEndPoint)
        {
            int timeStart, timeEnd, responseSize, errCount = 0;
            byte[] responseMessage = new byte[1024];
        
            EndPoint hopEndPoint = destIPEndPoint;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
            for (int i = 1; i < 50; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, i);
                    timeStart = Environment.TickCount;
                    socket.SendTo(packet.getBytes(), packet.PacketSize, SocketFlags.None, destIPEndPoint);
                    try
                    {
                        responseSize = socket.ReceiveFrom(responseMessage, ref hopEndPoint);
                        timeEnd = Environment.TickCount;
                        ICMPPacket response = new ICMPPacket(responseMessage, responseSize);
                        if ((response.Type == 0) || (response.Type == 11))
                        {
                            Console.Write("{0} мс\t", timeEnd - timeStart);
                            if (j == 2)
                            {
                                Console.WriteLine("{0}: {1}", i, hopEndPoint.ToString());
                            }
                        }
                        if ((response.Type == 0) && (j == 2))
                        {
                            Console.WriteLine("Трассировка завершена.");
                            return;
                        }
                        errCount = 0;
                    }
                    catch (SocketException)
                    {
                        Console.Write("*\t");
                        if (j == 2)
                        {
                            Console.WriteLine("{0}: Превышен интервал ожидания для запроса.", i); //
                        }
                        errCount++;
                        if (errCount == 30)
                        {
                            Console.WriteLine("Невозможно связаться с удаленным хостом."); //
                            return;
                        }
                    }
                }
            }
        }
    }
}
