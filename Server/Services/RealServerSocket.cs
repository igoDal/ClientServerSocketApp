using System.Net.Sockets;
using Server.Interfaces;

namespace Server.Services;

public class RealServerSocket : IRealServerSocket
{
    private readonly Socket _socket;

    public RealServerSocket(Socket socket)
    {
        _socket = socket;
    }

    public int Send(byte[] buffer) => _socket.Send(buffer);

    public int Receive(byte[] buffer) => _socket.Receive(buffer);
}