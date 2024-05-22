using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Management.Infrastructure;

internal class Client
{
    public static void StartClient()
    {
        byte[] bytes = new byte[1024];

        try
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sender.Connect(remoteEP);
                Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                string connectionCode = Guid.NewGuid().ToString();
                byte[] msg = Encoding.ASCII.GetBytes(connectionCode + "<EOF>");
                int bytesSent = sender.Send(msg);

                Task.Run(() =>
                {
                    while (true)
                    {
                        byte[] recvBuffer = new byte[1024];
                        int bytesRec = sender.Receive(recvBuffer);
                        string receivedData = Encoding.ASCII.GetString(recvBuffer, 0, bytesRec);
                        receivedData= receivedData.Replace("<EOF>", "");
                        Console.WriteLine("\nReceived from server: \n" + receivedData);
                    }
                });

                while (true)
                {
                    Console.Write("Enter a message: ");
                    string message = Console.ReadLine();

                    CimSession session = CimSession.Create(null);
                    IEnumerable<CimInstance> results = session.QueryInstances(@"root\cimv2", "WQL", message);
                    foreach (CimInstance result in results)
                    {
                        Console.WriteLine(result.ToString()); 
                    }



                    if (message == "exit")
                    {
                        msg = Encoding.ASCII.GetBytes("disconnect<EOF>");
                        bytesSent = sender.Send(msg);
                        break;
                    }

                    msg = Encoding.ASCII.GetBytes(message + "<EOF>");
                    bytesSent = sender.Send(msg);
                }

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
