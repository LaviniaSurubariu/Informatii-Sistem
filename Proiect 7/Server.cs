using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proiect_7
{
    internal class Server
    {
        private static List<ClientDetails> connectedClients = new List<ClientDetails>();
        private static ConcurrentQueue<ClientMessage> messageQueue = new ConcurrentQueue<ClientMessage>();
        private static int clientCount = 1;

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

                Thread messageProcessingThread = new Thread(ProcessMessages);
                messageProcessingThread.Start();

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
                        ClientDetails clientDetails = new ClientDetails(clientCount, connectionCode, handler);
                        clientCount++;
                        connectedClients.Add(clientDetails);
                        Console.WriteLine("Client connected with connection code: " + connectionCode + ". Total clients: " + connectedClients.Count);

                        string clientList = "";
                        byte[] msg;

                        if (connectedClients.Count > 1)
                        {
                            clientList = string.Join(", ", connectedClients);
                            msg = Encoding.ASCII.GetBytes(clientList + "<EOF>");
                            foreach (var client in connectedClients)
                            {
                                clientList = string.Join("\n", connectedClients.Where(c => c != client));
                                msg = Encoding.ASCII.GetBytes("Connected clients:\n" + clientList + "<EOF>");
                                client.Send(msg);
                            }
                        }

                        while (true)
                        {
                            StringBuilder dataBuilder = new StringBuilder();
                            data = null;
                            while (true)
                            {

                                int bytesRec = handler.Receive(bytes);
                                dataBuilder.Append(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                                data = dataBuilder.ToString();

                                int eofIndex;
                                while ((eofIndex = data.IndexOf("<EOF>")) > -1)
                                {
                                    string completeMessage = data.Substring(0, eofIndex);
                                    messageQueue.Enqueue(new ClientMessage { Data = completeMessage, ClientDetails = clientDetails, Handler = handler });
                                    dataBuilder.Remove(0, eofIndex + 5);
                                    data = dataBuilder.ToString();
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        static void ProcessMessages()
        {
            while (true)
            {
                if (messageQueue.TryDequeue(out ClientMessage clientMessage))
                {
                    string data = clientMessage.Data.Replace("<EOF>", "").Trim();
                    ClientDetails clientDetails = clientMessage.ClientDetails;
                    Socket handler = clientMessage.Handler;

                    if (data == "disconnect")
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        connectedClients.Remove(clientDetails);
                        Console.WriteLine("Client disconnected. Total clients: " + connectedClients.Count);

                        string clientList = "";
                        byte[] msg;

                        if (connectedClients.Count > 1)
                        {
                            foreach (var client in connectedClients)
                            {
                                clientList = string.Join("\n", connectedClients.Where(c => c != client));
                                msg = Encoding.ASCII.GetBytes(clientList + "<EOF>");
                                client.Send(msg);
                            }
                        }
                        else
                        {
                            foreach (var client in connectedClients)
                            {
                                msg = Encoding.ASCII.GetBytes("You are the single one here!" + "<EOF>");
                                client.Send(msg);
                            }

                        }
                    }
                    else if (data.StartsWith("sendto"))
                    {
                        Console.WriteLine("\nReceived message from client: " + data);
                        string[] dataParts = data.Split(' ');
                        int clientID = int.Parse(dataParts[1]);
                        string connectionCodeFromSender = dataParts[3];
                        string message = dataParts[4];
                        for (int i = 5; i < dataParts.Length; i++)
                        {
                            message += " " + dataParts[i];
                        }
                        ClientDetails clientToSend = connectedClients.FirstOrDefault(c => c.GetClientID() == clientID);
                        if (clientToSend != null)
                        {
                            clientToSend.Send(Encoding.ASCII.GetBytes("WMI " + connectionCodeFromSender + "|" + message + "<EOF>"));
                            Console.WriteLine("\nWMI " + connectionCodeFromSender + "|" + message + "<EOF>");
                            Console.WriteLine("\nSending message to client to receive result " + clientToSend);
                        }
                    }
                    else if (data.StartsWith("Result"))
                    {
                        Console.WriteLine("\nReceived result from client: " + data);
                        data = data.Replace("Result for ", "");
                        string connectionCodeToSend = data.Split('|')[0].Trim();
                        string connectionCodeClientResult = data.Split('|')[1];
                        string wmiInstruction = data.Split('|')[2];
                        string result = data.Split('|')[3];

                        Console.WriteLine("\nClient to send: " + connectionCodeToSend);

                        ClientDetails clientToSend = connectedClients.FirstOrDefault(c => c.GetConnectionCode().Trim() == connectionCodeToSend.Trim());


                        if (clientToSend != null)
                        {
                            int clientIdToSend = connectedClients.Find(c => c.GetConnectionCode() == connectionCodeClientResult).GetClientID();
                            Console.WriteLine("\nClient ID to send: " + clientIdToSend);
                            clientToSend.Send(Encoding.ASCII.GetBytes("From client " + clientIdToSend + " \n\tinstruction: " + wmiInstruction + "\n\tresult: " + result + "<EOF>"));
                            Console.WriteLine("\nSending message to client as result " + clientToSend);
                        }
                    }
                }
            }
        }
    }
}
