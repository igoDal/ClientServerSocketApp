namespace Server.Interfaces;

public interface IRealServerSocket
{
    int Send(byte[] buffer);
    int Receive(byte[] buffer);
}