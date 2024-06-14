using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Interfaces;
using Client.Services;
using Newtonsoft.Json;

namespace Client;

public class Client
{
    private bool isLoggedIn = true;
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
        
        Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        IRealClientSocket realSocketClient = new RealClientSocket(socket);
            
        Client clientSocket = new Client(realSocketClient);

        clientSocket.ExecuteClient();
    }

    void ExecuteClient()
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostEntry.AddressList[0];
        IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 11111);

        sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            try
            {
                sender.Connect(localEndpoint);
                Console.WriteLine("Socket connected to -> {0}", sender.RemoteEndPoint.ToString());

                while (continueListening)
                {
                    while (isLoggedIn)
                    {
                        byte[] initialCommand = new byte[1024];

                        int initComm = _realClientSocket.Receive(initialCommand);

                        string jsonInitCommand = Encoding.ASCII.GetString(initialCommand, 0, initComm);
                        string encodedInitComm = JsonConvert.DeserializeObject(jsonInitCommand).ToString();

                        Console.WriteLine(encodedInitComm);

                        string command = Console.ReadLine();
                        switch (command)
                        {
                            case "add":
                            case "logout":
                            case "stop":
                            case "msg":
                            case "read":
                            case "user":
                            case "help":
                            default:
                                defaultMsg(command);
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void defaultMsg(string command)
    {
        string jsonCommand = JsonConvert.SerializeObject(command);
        byte[] msgSent = Encoding.ASCII.GetBytes(jsonCommand);
        int byteSent = _realClientSocket.Send(msgSent);
        byte[] msgReceived = new byte[1024];

        int byteReceived = _realClientSocket.Receive(msgReceived);
        string jsonString = Encoding.ASCII.GetString(msgReceived, 0, byteReceived);
        string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
        Console.WriteLine(encodingString);
    }
    
    
}