using UnityEngine;
using UnityEngine.UI;
using TeleopReachy;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class RecordingServer : MonoBehaviour
{
    private const byte START_RECORDING = 1;
    private const byte STOP_RECORDING = 0;
    private const byte CLOSE_CONNECTION = 5;
    private const byte DUMMY = 0xFF;
    private const byte RECORDING_START = 1;
    private const byte RECORDING_END = 2;
    private const byte NO_REQUEST = 3;
    private const byte ALLOW_REQUEST = 4;
    private const int PORT = 50056;

    private TcpClient client;
    private NetworkStream stream;
    private Thread thread;
    [SerializeField] private string robot_ip_address = "192.168.68.60";

    [SerializeField] private HandsTracker hand_tracker;
    private HandController controller;
    private bool was_pressed = false;
    private volatile bool active_recording = false;
    private volatile bool can_record = false;
    private volatile bool display_indicator = false;
    [SerializeField] private Image indicator_image;
    [SerializeField] private Image no_recording_image;
    private bool listen = true;

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
            }
            else if (can_record)
            {
                send_message(START_RECORDING, 0);
                Debug.Log("Starting Recording");
            }
        }
        was_pressed = controller.is_record_button_pressed;
        indicator_image.enabled = display_indicator;
        no_recording_image.enabled = !can_record && !active_recording;
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
            byte[] buffer = new byte[8];
            while (listen)
            {
                int received = stream.Read(buffer, 0, 2);
                if (received <= 0)
                {
                    listen = false;
                    continue;
                }
                for (int i = 0; i < received; i++) {
                    byte command = buffer[i];
                    switch (command)
                    {
                        case NO_REQUEST:
                            can_record = false;
                            break;
                        
                        case ALLOW_REQUEST:
                            can_record = true;
                            break;
                        
                        case RECORDING_START:
                            display_indicator = true;
                            active_recording = true;
                            break;
                        
                        case RECORDING_END:
                            display_indicator = false;
                            active_recording = false;
                            break;
                        
                        case CLOSE_CONNECTION:
                            listen = false;
                            break;
                        
                        default:
                            break;
                    }
                }
            }
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
                send_message(CLOSE_CONNECTION, 0);
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