namespace Client.Interfaces;

public interface IRealClientSocket
{
    int Send(byte[] buffer);
    int Receive(byte[] buffer);
}