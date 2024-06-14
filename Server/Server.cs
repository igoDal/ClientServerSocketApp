﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Server.Interfaces;
using Server.Services;

namespace Server;
public class Server
{
    private readonly IRealServerSocket _realServerSocket;
    private Socket client;
    private bool stopped = false;
    private bool loggedIn = true;

    public Server(IRealServerSocket realServerSocket)
    {
        _realServerSocket = realServerSocket;
    }

    static void Main(string[] args)
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostEntry.AddressList[0];
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 11111);
        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        IRealServerSocket realServerSocket = new RealServerSocket(listener);
        Server serverSocket = new Server(realServerSocket);

        serverSocket.ExecuteServer();
    }

    public void ExecuteServer()
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostEntry.AddressList[0];
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 11111);
        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(ipEndPoint);
            listener.Listen(10);
            
            Console.WriteLine("Awaiting connection...");
            client = listener.Accept();
            Console.WriteLine("Connected");

            while (!stopped)
            {
                byte[] firstBytes = new byte[1024];
                string firstJsonData = null;

                int firstNumByte = client.Receive(firstBytes);
                firstJsonData += Encoding.ASCII.GetString(firstBytes, 0, firstNumByte);
                string firstData = JsonConvert.DeserializeObject(firstJsonData).ToString();

                /*if (data.ToLower() == "login")
                {
                    login();
                }*/

                while (loggedIn)
                {
                    var jsonMsg = JsonConvert.SerializeObject("Enter comm");
                    var msg = Encoding.ASCII.GetBytes(jsonMsg);
                    client.Send(msg);

                    byte[] bytes = new byte[1024];
                    string jsonData = null;
                    
                    int numByte = client.Receive(firstBytes);
                    jsonMsg += Encoding.ASCII.GetString(bytes);
                    string data = JsonConvert.DeserializeObject(jsonData).ToString();
                    Console.WriteLine("Text received -> {0}", data);

                    switch (data.ToLower())
                    {
                        case "help":
                        default:
                            helpCommand();
                            break;
                    }
                }
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void helpCommand()
    {
        var jsonMsg = JsonConvert.SerializeObject($"Available commands:\n" +
                                              $"'add' - to add new user\n" +
                                              $"'help' - to get a list of available commands with their description\n" +
                                              $"'info' - to get info about server version, server creation date\n" +
                                              $"'msg' - to send a message to other user\n" +
                                              $"'read' - to read next message\n" +
                                              $"'uptime' - to check server uptime\n" +
                                              $"'user' - to print user data" +
                                              $"'stop' - to stop the server\n" +
                                              $"'logout' - to log out");
        
        byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
        client.Send(message);
    }
    
}