using System.Net;
using System.Net.Sockets;
using Server.Interfaces;
using Server.Services;

namespace Server;
public class Server
{
    private readonly IRealServerSocket _realServerSocket;
    private Socket client;

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
            
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}