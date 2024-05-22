using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_7
{
    internal class Server
    {
        private static List<ClientDetails> connectedClients = new List<ClientDetails>();
        static int clientCount = 1;

        public static void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = listener.Accept();

                    Task.Run(() =>  
                    {
                        byte[] bytes = new byte[1024];
                        string data = null;

                        while (true)
                        {
                            int bytesRec = handler.Receive(bytes);
                            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                            if (data.IndexOf("<EOF>") > -1)
                            {
                                break;
                            }
                        }

                        string connectionCode = data.Replace("<EOF>", "");
                        ClientDetails clientDetails = new ClientDetails(clientCount,connectionCode, handler);
                        clientCount++;
                        connectedClients.Add(clientDetails);
                        Console.WriteLine("Client connected with connection code: " + connectionCode + ". Total clients: " + connectedClients.Count);

                        string clientList = string.Join(", ", connectedClients);
                        byte[] msg = Encoding.ASCII.GetBytes(clientList + "<EOF>");
                        foreach (var client in connectedClients)
                        {
                            clientList = string.Join("\n", connectedClients.Where(c => c != client));
                            msg = Encoding.ASCII.GetBytes(clientList + "<EOF>");
                            client.Send(msg);
                        }


                        while (true)
                        {
                            data = null;
                            while (true)
                            {
                                bytes = new byte[1024];
                                int bytesRec = handler.Receive(bytes);
                                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                                if (data.IndexOf("<EOF>") > -1)
                                {
                                    break;
                                }
                            }

                            if (data.Replace("<EOF>", "").Trim() == "disconnect")
                            {
                                handler.Shutdown(SocketShutdown.Both);
                                handler.Close();
                                connectedClients.Remove(clientDetails);
                                Console.WriteLine("Client disconnected. Total clients: " + connectedClients.Count);

                                foreach (var client in connectedClients)
                                {
                                    clientList = string.Join("\n", connectedClients.Where(c => c != client));
                                    msg = Encoding.ASCII.GetBytes(clientList + "<EOF>");
                                    client.Send(msg);
                                }
                                break;
                            }
                        }
                    }) ;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
    }
}
