using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_7
{
    internal class ClientMessage
    {
        public string Data { get; set; }
        public ClientDetails ClientDetails { get; set; }
        public Socket Handler { get; set; }
    }
}
