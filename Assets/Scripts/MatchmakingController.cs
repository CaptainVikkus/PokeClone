using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using NetworkMessages;
using Unity.Collections;
using System.Text;

public class MatchmakingController : MonoBehaviour
{
    [SerializeField] GameObject glow;

    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIP;
    public ushort serverPort;
    [HideInInspector] public static string opponentIP;
    [HideInInspector] public static ushort opponentPort;

    public string localID;

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.Parse(serverIP, serverPort);
        m_Connection = m_Driver.Connect(endpoint);
    }

    void AddToLobby()
    {

    }

    void OnConnect()
    {
        Debug.Log("We are now connected to the server");
    }

    void OnDisconnect()
    {
        Debug.Log("Client got disconnected from server");
        m_Connection = default(NetworkConnection);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void OnData(DataStreamReader stream)
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        MessageHeader header = JsonUtility.FromJson<MessageHeader>(recMsg);

        switch (header.type)
        {
            case MessageType.PLAYER_MSG:
                break;
            case MessageType.BATTLE_MSG:
                break;
            default:
                Debug.Log("Unrecognized message received!");
                break;
        }
    }

    void SendToServer(string message)
    {
        var writer = m_Driver.BeginSend(m_Connection);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    //** Glow and Trigger Matchmaking On**//
    private void OnTriggerEnter2D(Collider2D collision)
    {
        glow.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        glow.SetActive(false);
    }
    //** /Glow and Trigger **//

    //Called once every frame
    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        cmd = m_Connection.PopEvent(m_Driver, out stream);
        while (cmd != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                OnConnect();
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnData(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                OnDisconnect();
            }

            cmd = m_Connection.PopEvent(m_Driver, out stream);
        }
    }

}
