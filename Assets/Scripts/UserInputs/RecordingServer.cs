using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

public class RecordingServer : MonoBehaviour
{
    private const byte START_RECORDING = 1;
    private const byte STOP_RECORDING = 0;
    private const byte CLOSE_CONNECTION = 2;
    public const int PORT = 50056;

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Thread thread;

    public void Start()
    {
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, PORT);
            server.Start();
            client = server.AcceptTcpClient();
            Debug.Log("Client connected");
            stream = client.GetStream();
            send_message(START_RECORDING, 1);
            send_message(STOP_RECORDING, 10);
            send_message(CLOSE_CONNECTION, 2);
        }
        catch (SocketException e)
        {
            Debug.Log(e);
        }
        finally
        {
            server.Stop();
        }
    }

    public void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    public void send_message(byte message_type, int wait_before)
    {
        Thread.Sleep(wait_before*1000);
        stream.Write(new byte[] {message_type}, 0, 1);
        Debug.Log("Wrote command " + message_type.ToString());
    }
}