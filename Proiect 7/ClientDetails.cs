using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_7
{
    internal class ClientDetails
    {
        private string connectionCode;
        private Socket clientSocket;
        private int clientID;

        public ClientDetails(int clientId,string connectionCode, Socket clientSocket)
        {
            this.clientID = clientId;
            this.connectionCode = connectionCode;
            this.clientSocket = clientSocket;
        }



        public void Send(byte[] msg)
        {
            clientSocket.Send(msg);
        }

        public string GetConnectionCode()
        {
            return connectionCode;
        }
        public int GetClientID()
        {
            return clientID;
        }
        public Socket GetClientSocket()
        {
            return clientSocket;
        }

        public override string ToString()
        {
            return "Client ID: " + clientID + " Connection code: " + connectionCode;
        }
    }
}
