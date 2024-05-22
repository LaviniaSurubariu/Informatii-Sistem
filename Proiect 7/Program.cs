using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_7
{
    internal class Program
    {
       
            static void Main(string[] args)
            {
                Console.WriteLine("Choose mode: (1) Server (2) Client");
                string mode = Console.ReadLine();

                if (mode == "1")
                {
                    Server.StartListening();
                }
                else if (mode == "2")
                {
                    Client.StartClient();
                }
                else
                {
                    Console.WriteLine("Invalid mode selected.");
                }
            }
        

    }
}
