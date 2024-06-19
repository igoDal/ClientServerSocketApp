using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Interfaces;
using Client.Services;
using Newtonsoft.Json;

namespace Client;

public class Client
{
    private static bool isLoggedIn = false;
    private static bool isOnline = false;
    private static bool continueListening = true;
    private static Socket sender;
    private static bool notLoggedInFlag = false;
    //private readonly IRealClientSocket _realClientSocket;

    public bool IsLoggedIn
    {
        get
        {
            return isLoggedIn;
        }
    }

    // public Client(IRealClientSocket realClientSocket)
    // {
    //     _realClientSocket = realClientSocket;
    // }

    static void Main(string[] args)
    {
        // IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        // IPAddress ipAddress = ipHostEntry.AddressList[0];
        // IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 11111);
        //
        // Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        // IRealClientSocket realSocketClient = new RealClientSocket(socket);
        //     
        // Client clientSocket = new Client(realSocketClient);

        ExecuteClient();
    }

    static void ExecuteClient()
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
                    Menu();
                    
                    while (isLoggedIn)
                    {
                        byte[] initialCommand = new byte[1024];

                        int initComm = sender.Receive(initialCommand);

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

    private static void defaultMsg(string command)
    {
        string jsonCommand = JsonConvert.SerializeObject(command);
        byte[] msgSent = Encoding.ASCII.GetBytes(jsonCommand);
        int byteSent = sender.Send(msgSent);
        byte[] msgReceived = new byte[1024];

        int byteReceived = sender.Receive(msgReceived);
        string jsonString = Encoding.ASCII.GetString(msgReceived, 0, byteReceived);
        string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
        Console.WriteLine(encodingString);
    }
    
    public static void login(string username, string password)
    {
        Console.WriteLine();
        string msg = "login";

        var userReq = usernameRequest(msg);


        if (userReq.ToLower().Contains("username")) enterUsername(username);


        var passRequested = passwordRequest();

        if (passRequested.ToLower().Contains("password"))
        {
            enterPassword(password);
        }
        else
        {
            Console.WriteLine(passRequested);
            return;
        }
            
        byte[] receiveLoginAnswer = new byte[1024];
        int loginAnswerReceived = sender.Receive(receiveLoginAnswer);
        string jsonLoginAnswer = Encoding.ASCII.GetString(receiveLoginAnswer, 0, loginAnswerReceived);
        string encodingLoginAnswer = JsonConvert.DeserializeObject(jsonLoginAnswer).ToString();

        if (encodingLoginAnswer == "loggedIn") 
        {
            Console.WriteLine($"\n{username} has logged in.");
            isLoggedIn = true;
        }
        else
        {
            Console.WriteLine(encodingLoginAnswer);
        }
    }
    
    private static void Menu()
    {
        Console.WriteLine("\nType '1' to login\n" +
                          "Type '2' to create new user\n"); //+
        char choice = Console.ReadKey().KeyChar;
        Console.WriteLine();
        if (choice == '1')
        {
            Console.WriteLine("\nPodaj login: ");
            string username = Console.ReadLine();
            Console.WriteLine("\nPodaj hasło: ");
            string password = Console.ReadLine();
            login(username, password);
        }
        else if (choice == '2')
        {
            addUser();
        }
        else
        {
            return;
        }
    }
    
    private static void addUser()
    {
        Console.WriteLine();
        string msg = "add";

        usernameRequest(msg);

        string username = Console.ReadLine();
        enterUsername(username);
        passwordRequest();

        string password = Console.ReadLine();
        enterPassword(password);
        byte[] messageReceivedPass = new byte[1024];
        int byteRcvdPass = sender.Receive(messageReceivedPass);
        string jsonStringpass = Encoding.ASCII.GetString(messageReceivedPass, 0, byteRcvdPass);
        string encodingStringpass = JsonConvert.DeserializeObject(jsonStringpass).ToString();
        Console.WriteLine(encodingStringpass);
    }
    private static string usernameRequest(string command)
    {
        string jsonCommand = JsonConvert.SerializeObject(command);
        byte[] messageSentUsername = Encoding.ASCII.GetBytes(jsonCommand);
        sender.Send(messageSentUsername);
        byte[] messageReceivedUser = new byte[1024];
        int byteRcvdUser = sender.Receive(messageReceivedUser);
        string jsonString = Encoding.ASCII.GetString(messageReceivedUser, 0, byteRcvdUser);
        string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();
        return encodingString;
    }

    private static void enterUsername(string username)
    {
        string jsonSendUsername = JsonConvert.SerializeObject(username);
        byte[] sendUsername = Encoding.ASCII.GetBytes(jsonSendUsername);
        sender.Send(sendUsername);
    }

    private static string passwordRequest()
    {
        byte[] receivePasswordRequest = new byte[1024];
        int passwordRequestReceived = sender.Receive(receivePasswordRequest);
        string jsonStringPasswordRequest = Encoding.ASCII.GetString(receivePasswordRequest, 0, passwordRequestReceived);
        string encodingStringPasswordRequest = JsonConvert.DeserializeObject(jsonStringPasswordRequest).ToString();
        //Console.WriteLine(encodingStringPasswordRequest);
        if (encodingStringPasswordRequest.ToLower().Equals("user doesn't exist."))
        {
            notLoggedInFlag = true;
        }

        return encodingStringPasswordRequest;
    }

    private static void enterPassword(string password)
    {
        string jsonSendPassword = JsonConvert.SerializeObject(password);
        byte[] sendPassword = Encoding.ASCII.GetBytes(jsonSendPassword);
        sender.Send(sendPassword);
    }
    
    private static void readMessage(string command)
    {
        defaultMsg(command);
    }

    private static void sendMessage(string command)
    {
        //Call sendMessage method on server side
        string jsonCommand = JsonConvert.SerializeObject(command);
        byte[] msgCommand = Encoding.ASCII.GetBytes(jsonCommand);
        int byteSent = sender.Send(msgCommand);
        byte[] msgReceived = new byte[1024];
        int byteRcvd = sender.Receive(msgReceived);
        string jsonString = Encoding.ASCII.GetString(msgReceived, 0, byteRcvd);
        string encodingString = JsonConvert.DeserializeObject(jsonString).ToString();

        Console.WriteLine(encodingString);
        
        //Request for username (message receiver)
        string userToSend = Console.ReadLine();
        string jsonUserToSend= JsonConvert.SerializeObject(userToSend);
        byte[] usernameSent = Encoding.ASCII.GetBytes(jsonUserToSend);
        int byteUserToSend = sender.Send(usernameSent);

        byte[] userToSendReceived = new byte[1024];

        int byteUserRcvd = sender.Receive(userToSendReceived);
        string jsonUserString = Encoding.ASCII.GetString(userToSendReceived, 0, byteUserRcvd);
        string encodingUserString = JsonConvert.DeserializeObject(jsonUserString).ToString();
        Console.WriteLine(encodingUserString);

        string message = Console.ReadLine();
        const int MAX_LENGTH = 255;
        if (message.Length > MAX_LENGTH)
        {
            message = message.Substring(0, MAX_LENGTH);
        }
        string jsonMessage = JsonConvert.SerializeObject(message);
        byte[] messageToSend = Encoding.ASCII.GetBytes(jsonMessage);
        int byteMessageSent = sender.Send(messageToSend);

        byte[] messageReceived = new byte[1024];
        int byteMessageRcvd = sender.Receive(messageReceived);
        string jsonStringMessage = Encoding.ASCII.GetString(messageReceived, 0, byteMessageRcvd);
        string encodingStringMessage = JsonConvert.DeserializeObject(jsonStringMessage).ToString();
        Console.WriteLine(encodingStringMessage);
    }
    
    
}