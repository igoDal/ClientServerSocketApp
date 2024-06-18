using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Server.Interfaces;
using Server.Services;

namespace Server;
public class Server
{
    //private readonly IRealServerSocket _realServerSocket;
    private static Socket clientSocket;
    private readonly static string serverVersion = "0.0.3";
    private readonly static DateTime serverCreationDate = DateTime.Now;
    private static byte[] message;
    private static string jsonMsg;
    private static readonly byte[] bytes = new byte[1024];
    private static readonly byte[] bytesU = new byte[1024];
    private static readonly byte[] bytesP = new byte[1024];
    private static bool loggedIn = false;
    private static bool stopped = false;
    private static string currentRole;
    private static string loggedInUser;

    // public Server(IRealServerSocket realServerSocket)
    // {
    //     _realServerSocket = realServerSocket;
    // }

    static void Main(string[] args)
    {
        // IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        // IPAddress ipAddress = ipHostEntry.AddressList[0];
        // IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 11111);
        // Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //
        // IRealServerSocket realServerSocket = new RealServerSocket(listener);
        // Server serverSocket = new Server(realServerSocket);

        ExecuteServer();
    }

    public static void ExecuteServer()
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
            clientSocket = listener.Accept();
            Console.WriteLine("Connected");

            while (!stopped)
            {
                byte[] firstBytes = new byte[1024];
                string firstJsonData = null;

                int firstNumByte = clientSocket.Receive(firstBytes);
                firstJsonData += Encoding.ASCII.GetString(firstBytes, 0, firstNumByte);
                string firstData = JsonConvert.DeserializeObject(firstJsonData).ToString();

                if (firstData.ToLower() == "login")
                {
                    login();
                }
                else if (firstData.ToLower() == "add")
                {
                    addUser();
                }
                else
                    break;
                
                while (loggedIn)
                {
                    jsonMsg = JsonConvert.SerializeObject("Enter comm");
                    message = Encoding.ASCII.GetBytes(jsonMsg);
                    clientSocket.Send(message);

                    byte[] bytes = new byte[1024];
                    string jsonData = null;
                    
                    int numByte = clientSocket.Receive(bytes);
                    jsonData += Encoding.ASCII.GetString(bytes, 0, numByte);
                    string data = JsonConvert.DeserializeObject(jsonData).ToString();
                    Console.WriteLine("Text received -> {0}", data);

                    switch (data.ToLower())
                    {
                        case "help":
                            helpCommand();
                            break;
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

    private static void helpCommand()
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
        clientSocket.Send(message);
    }
    
    private static void login()
    {
        jsonMsg = JsonConvert.SerializeObject($"Enter username:");
        message = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(message);

        string username;
        string password;
        int numByte = clientSocket.Receive(bytesU);
        string jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
        username = JsonConvert.DeserializeObject(jsonUsername).ToString();
        var file = $"{username}.json";

        if (File.Exists(file))
        {
            var fileRead = File.ReadAllText(file);
            jsonMsg = JsonConvert.SerializeObject($"Enter password:");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);

            JsonReader line;
            int numBytePassword = clientSocket.Receive(bytesP);
            string jsonPassword = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
            password = JsonConvert.DeserializeObject(jsonPassword).ToString();

            var singleUserData = JsonConvert.DeserializeObject<User>(fileRead);
            string getPassword = singleUserData.Password;
            currentRole = singleUserData.Role;
            loggedInUser = singleUserData.Userame;
            


            Console.WriteLine(getPassword);
            if (getPassword.Equals(password))
            {
                loggedIn = true;
                jsonMsg = JsonConvert.SerializeObject($"loggedIn");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);
            }
            else
            {
                jsonMsg = JsonConvert.SerializeObject($"Incorrect password!");
                message = Encoding.ASCII.GetBytes(jsonMsg);
                clientSocket.Send(message);
            }
        }
        else
        {
            jsonMsg = JsonConvert.SerializeObject($"user doesn't exist.");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }
    }
    private static void logout()
    {
        loggedIn = false;

        jsonMsg = JsonConvert.SerializeObject("logout");
        byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(message);
    }
    private static void addUser()
    {
        jsonMsg = JsonConvert.SerializeObject($"Enter username:");
        message = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(message);

        string username;
        string password;
        int numByte = clientSocket.Receive(bytesU);
        string jsonUsername = Encoding.ASCII.GetString(bytesU, 0, numByte);
        username = JsonConvert.DeserializeObject(jsonUsername).ToString();

        if (!File.Exists($"{username}.json"))
        {
            jsonMsg = JsonConvert.SerializeObject($"Enter password:");
            message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);

            int numBytePassword = clientSocket.Receive(bytesP);
            string jsonPassword = Encoding.ASCII.GetString(bytesP, 0, numBytePassword);
            password = JsonConvert.DeserializeObject(jsonPassword).ToString();


            User user = new User()
            {
                Userame = username,
                Password = password,
                Role = "user"
            };

            using (StreamWriter file = File.CreateText($"{username}.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, user);
            }

            jsonMsg = JsonConvert.SerializeObject($"User {username} has been added.");
            message = Encoding.ASCII.GetBytes(jsonMsg);
        }

        else
        {
            jsonMsg = JsonConvert.SerializeObject($"User {username} already exists.");
            message = Encoding.ASCII.GetBytes(jsonMsg);
        }
        clientSocket.Send(message);
    }
    private static void deleteUser()
    {
        Console.WriteLine("Enter user (username) to delete: ");
        string username = Console.ReadLine();
        if (File.Exists($"{username}.txt"))
            File.Delete($"{username}.txt");

        jsonMsg = JsonConvert.SerializeObject($"User {username} has been removed.");
        byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(message);
    }
    private static void incorrectCommand()
    {
        jsonMsg = JsonConvert.SerializeObject($"Incorrect command. Type 'help' to get list of commands.");
        byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(message);
    }
    private static void stopCommand()
    {

        try
        {
            jsonMsg = JsonConvert.SerializeObject("stop");
            byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
            clientSocket.Send(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while sending 'stop' command: " + ex.ToString());
        }
        finally
        {
            // Close the client socket
            //clientSocket.Close();
            loggedIn = false; // Exit the loggedIn loop
            stopped = true;
        }
    }
    private static void uptimeCommand()
    {
        DateTime serverCurrentDate = DateTime.Now;
        jsonMsg = JsonConvert.SerializeObject($"Server is up for {serverCurrentDate - serverCreationDate}");
        byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(message);
    }
    private static void infoCommand()
    {
        jsonMsg = JsonConvert.SerializeObject($"Server version: {serverVersion}\n" +
                                              $"Server Creation Date: {serverCreationDate}");
        byte[] message = Encoding.ASCII.GetBytes(jsonMsg);
        clientSocket.Send(message);
    }
}