using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace LocalAdminChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine("--->Local Subnet Admin Checker<---");
            Console.WriteLine("=======================================================");
            Console.WriteLine("First determining the local IP address...");
            string myIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 443);
                IPEndPoint end = socket.LocalEndPoint as IPEndPoint;
                myIP = end.Address.ToString();

            }
            Console.WriteLine("Internal IP found of: " + myIP);
            List<string> ipslist = new List<string>();
            List<string> localadmin = new List<string>();
            byte octet1;
            byte octet2;
            byte octet3;
            byte octet4;
            octet1 = IPAddress.Parse(myIP).GetAddressBytes()[0];
            octet2 = IPAddress.Parse(myIP).GetAddressBytes()[1];
            octet3 = IPAddress.Parse(myIP).GetAddressBytes()[2];
            octet4 = 0;
            string first = Convert.ToString(octet1);
            string second = Convert.ToString(octet2);
            string third = Convert.ToString(octet3);
            string fourth = Convert.ToString(octet4);
            string firstthree = first + "." + second + "." + third + "." + fourth + "/24";
            string user = Environment.UserName;
            string domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[+] Running as user " + user + " on domain " + domain);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[+] Using wmic to check this subnet for local admin rights: " + firstthree + " ...");

            for (int y = 1; y < 255; y++)
            {
                try
                {
                    string z = Convert.ToString(y);

                    string host = first + "." + second + "." + third + "." + z;
                    ipslist.Add(host);


                }


                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


            }

            Queue myque = new Queue(ipslist);
            List<Thread> threads = new List<Thread>();

            for (int i = 1; i < 255; i++)
            {
                Thread newThread = new Thread(() => Threader(myque));
                newThread.IsBackground = true;
                newThread.Start();
                threads.Add(newThread);
                
            }

            foreach(Thread thread in threads)
            {
                thread.Join();
            }
            


        }

        static void Threader(Queue myque)
        {
            while (myque.Count != 0)
            {
                var worker = myque.Dequeue();
                string worker2 = Convert.ToString(worker);
                Checker(worker2);
            }
        }

        static void Checker(String item)
        {
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Windows\\System32\\wbem\\wmic.exe",
                    Arguments = "/node:\"" + item + "\" computersystem list brief /format:list",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };


            p.Start();
            string x = p.StandardOutput.ReadToEnd();
            bool admin = x.Contains("Name=");
            if (admin == true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Local admin rights found on this host: " + item);
                Console.ForegroundColor = ConsoleColor.White;
            }

           
        }


    }
}
