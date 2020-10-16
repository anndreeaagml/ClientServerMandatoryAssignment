using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BookLibraryAssignemnt;
using Newtonsoft.Json;

namespace ClientServerMandatoryAssignment
{
    class Program
    {
        private static List<Book> Library = new List<Book>
        {
            new Book("They were none", "Agatha Christie", 203, "c84y2424ncmr2"),
            new Book("Murder at the vicariage", "Agatha Christie", 223, "c9825cmv25mp2"),
            new Book("Mathias Sandorf", "Jules Vernes", 950, "mc89p42wc48v5")
        };
        private static TcpListener tcpListener; 
        private static TcpClient serverSocket;
        private static string prevrequest = "";
        static void Main(string[] args)
        {
            var ip = IPAddress.Parse("127.0.0.1");
            serverSocket = new TcpClient();
            tcpListener = new TcpListener(ip, 4646);
            Console.WriteLine("Server is ready to listen for requests");
            //this is the handshake
            tcpListener.Start();
            using (serverSocket = tcpListener.AcceptTcpClient())
            {
                Console.WriteLine("Client Ip" + (IPEndPoint)(serverSocket.Client.RemoteEndPoint));
                while (serverSocket.Connected)
                {
                    //method is now multi threaded
                    Task.Run(() => Work(serverSocket));
                }

            }
            tcpListener.Stop();
        }

        public static void Work(TcpClient server)
        {
            if (server.Connected)
                using (Stream ns = server.GetStream())
                {
                    StreamWriter streamWriter = new StreamWriter(ns) { AutoFlush = true };
                    StreamReader streamReader = new StreamReader(ns);
                    var request = streamReader.ReadLine();
                    while (request != null)
                    {
                        string message = "";
                        Console.WriteLine("Client Message: " + request);
                        if (prevrequest != "")
                        {
                            if (prevrequest == "Get")
                            {

                                if (request.Length == 13)
                                {
                                    message = Get(request);
                                    prevrequest = "";
                                }
                                else
                                {
                                    if (request == "Cancel")
                                    {
                                        message = "Cancelled";
                                        prevrequest = "";
                                    }
                                    else
                                        message = "It is not typed right";
                                }

                            }
                            if (prevrequest == "Save")
                            {
                                if (request == "Cancel")
                                {
                                    message = "Cancelled";
                                    prevrequest = "";
                                }
                                else
                                {
                                    try
                                    {
                                        Book newBook = JsonConvert.DeserializeObject<Book>(request);
                                        Library.Add(newBook);
                                        message = "Saved";
                                        prevrequest = "";
                                    }
                                    catch (Exception e)
                                    {
                                        message = "Failed to save new object. Try again or Cancel";
                                        Console.WriteLine(e.Message);
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (request == "GetAll")
                            {
                                message = GetAll();
                            }
                            if (request == "Get")
                            {
                                message = "Enter isbn or Cancel";
                                prevrequest = "Get";
                            }
                            if (request == "Save")
                            {
                                message = "Enter book as json object";
                                prevrequest = "Save";
                            }

                        }
                        streamWriter.WriteLine(message);//response



                        request = streamReader.ReadLine();
                    }
                    Console.WriteLine("Stopped");
                    server.Close();
                }

        }
        public static string GetAll()
        {
            string xy = "";
            foreach (Book x in Library)
            {
                xy += JsonConvert.SerializeObject(x) + " ; ";
            }
            return xy;
        }

        public static string Get(string isbn)
        {
            string xy = "";
            foreach (Book x in Library)
            {
                if (x.Isbn == isbn)
                {
                    xy = JsonConvert.SerializeObject(x);
                    break;
                }
            }
            return xy;
        }
    }
}
