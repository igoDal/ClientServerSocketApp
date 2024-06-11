﻿using System.Net;
using System.Net.Sockets;
using Client.Interfaces;

namespace Client;

public class Client
{
    private bool isLoggedIn = false;
    private bool isOnline = false;
    private bool continueListening = true;
    private Socket sender;
    private readonly IRealClientSocket _realClientSocket;

    public bool IsLoggedIn
    {
        get
        {
            return isLoggedIn;
        }
    }

    public Client(IRealClientSocket realClientSocket)
    {
        _realClientSocket = realClientSocket;
    }

    static void Main(string[] args)
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostEntry.AddressList[0];
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 11111);
    }
}