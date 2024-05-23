using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Management.Infrastructure;
using static System.Collections.Specialized.BitVector32;

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
                IEnumerable<CimInstance> results = null;

                Task.Run(() =>
                {
                    while (true)
                    {
                        byte[] recvBuffer = new byte[1024];
                        int bytesRec = sender.Receive(recvBuffer);
                        string receivedData = Encoding.ASCII.GetString(recvBuffer, 0, bytesRec);
                        receivedData = receivedData.Replace("<EOF>", "");
                        Console.WriteLine("\nReceived from server: \n" + receivedData);

                        if (receivedData.Substring(0, 3) == "WMI")
                        {
                            receivedData = receivedData.Replace("WMI", "");
                            string senderConnectionCode = receivedData.Split('|')[0].Trim();
                            results = null;
                            string wmiInstruction = receivedData.Split('|')[1];
                            wmiInstruction.Trim();
                            CimSession session = CimSession.Create(null);
                            results = session.QueryInstances(@"root\cimv2", "WQL", wmiInstruction);
                            string resultString = "Result for " + senderConnectionCode + "|" + connectionCode + "|" + wmiInstruction + "|";
                            foreach (CimInstance result in results)
                            {
                                resultString += result.ToString();
                            }


                            msg = Encoding.ASCII.GetBytes(resultString + " <EOF>");
                            bytesSent = sender.Send(msg);
                            Console.WriteLine("Sent to server: " + resultString);
                            receivedData = "";
                        }
                    }
                });

                while (true)
                {
                    Console.Write("Enter a WMI instruction eg: 'SELECT * FROM Win32_Battery' or exit to disconnect: ");
                    string message = Console.ReadLine();

                    if (message == "exit")
                    {
                        msg = Encoding.ASCII.GetBytes("disconnect<EOF>");
                        bytesSent = sender.Send(msg);
                        break;
                    }

                    else
                    {
                        Console.Write("Enter Clients IDs to sent the instruction like: '1,2,3,4' ");

                        string[] clients = Console.ReadLine().Split(',');
                        Console.WriteLine("Sending to clients: " + string.Join(",", clients));

                        foreach (var client in clients)
                        {
                            msg = Encoding.ASCII.GetBytes("sendto " + client + " from " + connectionCode + " " + message + "<EOF>");

                            bytesSent = sender.Send(msg);
                        }

                    }


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
