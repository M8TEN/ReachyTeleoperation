using UnityEngine;
using TeleopReachy;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class RecordingServer : MonoBehaviour
{
    private const byte START_RECORDING = 1;
    private const byte STOP_RECORDING = 0;
    private const byte CLOSE_CONNECTION = 2;
    public const int PORT = 50056;

    private TcpClient client;
    private NetworkStream stream;
    private Thread thread;
    private string robot_ip_address = "192.168.68.60";

    [SerializeField]
    private HandsTracker hand_tracker;
    private HandController controller;
    private bool was_pressed = false;
    private bool active_recording = false;

    public void Start()
    {
        EventManager.StartListening(EventNames.StartMirrorScene, SpawnTread);
    }

    void Update()
    {
        controller = hand_tracker.leftHand;
        // Button was just pressed
        if (controller.is_record_button_pressed && !was_pressed)
        {
            // Stop currently active recording
            if (active_recording)
            {
                send_message(STOP_RECORDING, 0);
                Debug.Log("Stopping Recording");
                active_recording = false;
            }
            else
            {
                send_message(START_RECORDING, 0);
                Debug.Log("Starting Recording");
                active_recording = true;
            }
        }
        was_pressed = controller.is_record_button_pressed;
    }

    private void SpawnTread()
    {
        string pref_string = PlayerPrefs.GetString("robot_ip");
        robot_ip_address = (pref_string != "none") ? pref_string : robot_ip_address;
        Debug.Log("Robot IP = " + robot_ip_address);
        if (client != null) {
            ClosePreviousConnection();
        }
        thread = new Thread(new ThreadStart(SetupClient));
        thread.Start();
    }
    private void SetupClient()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(robot_ip_address);
            IPEndPoint endpoint = new IPEndPoint(localAddr, PORT);
            client = new TcpClient();
            client.Connect(endpoint);
            Debug.Log("Client connected");
            stream = client.GetStream();
        }
        catch (SocketException e)
        {
            Debug.Log(e);
            ClosePreviousConnection();
        }
    }

    private void ClosePreviousConnection()
    {
        if (stream != null)
            {
                stream.Close();
            }
        if (client != null)
        {
            client.Close();
        }
        if (thread != null && thread.IsAlive) thread.Abort();
    }
    public void OnApplicationQuit()
    {
        ClosePreviousConnection();
    }

    public void send_message(byte message_type, int wait_before)
    {
        if (stream == null) return;
        if (wait_before > 0) Thread.Sleep(wait_before*1000);
        stream.Write(new byte[] {message_type}, 0, 1);
        Debug.Log("Wrote command " + message_type.ToString());
    }
}