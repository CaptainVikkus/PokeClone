using NetworkMessages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIP;
    public ushort serverPort;
    private string myIP;

    private MoveMessage move = null;

    private NetworkManager() { }
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkManager>();
            }
            return instance;
        }

        private set { }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.Parse(serverIP, serverPort);
        m_Connection = m_Driver.Connect(endpoint);

        StartCoroutine(GetIPAddress());
    }

    IEnumerator GetIPAddress()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://checkip.dyndns.org");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string result = www.downloadHandler.text;

            // This results in a string similar to this: <html><head><title>Current IP Check</title></head><body>Current IP Address: 123.123.123.123</body></html>
            // where 123.123.123.123 is your external IP Address.
            //  Debug.Log("" + result);

            string[] a = result.Split(':'); // Split into two substrings -> one before : and one after. 
            string a2 = a[1].Substring(1);  // Get the substring after the :
            string[] a3 = a2.Split('<');    // Now split to the first HTML tag after the IP address.
            string a4 = a3[0];              // Get the substring before the tag.

            Debug.Log("External IP Address = " + a4);
            myIP = a4;
        }
    }

    private IEnumerator Heartbeat()
    {
        while (this.isActiveAndEnabled)
        {
            var m = new MessageHeader();
            m.type = MessageType.HEARTBEAT;
            SendToServer(JsonUtility.ToJson(m));
            yield return new WaitForSeconds(1);
        }
    }

    public void AddToLobby()
    {
        //Build PlayerMsg
        PlayerMessage pMsg = new PlayerMessage();
        pMsg.connectionID = myIP;
        pMsg.playerName = SaveSystem.currentPlayer;
        pMsg.pokemonName = PlayerController.pokemon.Base.name;
        pMsg.hp = PlayerController.pokemon.HP;
        pMsg.Lvl = PlayerController.pokemon.Level;

        SendToServer(JsonUtility.ToJson(pMsg));
    }

    public bool hasMove()
    {
        return move != null;
    }

    public MoveMessage GetMove()
    {
        var _move = move;
        move = null;
        return _move;
    }
    void SendToServer(string message)
    {
        var writer = m_Driver.BeginSend(m_Connection);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    public void SendMove(Move move, bool hit)
    {
        var movemsg = new MoveMessage();
        movemsg.MoveName = move.Base.name;
        movemsg.hit = hit;

        SendToServer(JsonUtility.ToJson(movemsg));
    }

    public void SendBattle(BattleMessage battle)
    {
        SendToServer(JsonUtility.ToJson(battle));
    }

    void OnData(DataStreamReader stream)
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        MessageHeader header = JsonUtility.FromJson<MessageHeader>(recMsg);

        switch (header.type)
        {
            case MessageType.HEARTBEAT:
                Debug.Log("Heartbeat");
                break;
            case MessageType.BATTLE_MSG:
                var bMsg = JsonUtility.FromJson<BattleMessage>(recMsg);
                BattleData.SetBattleData(bMsg);
                var msg = new MessageHeader();
                GameController.Instance.StartMultiplayerBattle();
                break;
            case MessageType.MOVE_MSG: //Move Received
                var mMsg = JsonUtility.FromJson<MoveMessage>(recMsg);
                move = mMsg;
                Debug.Log("Move " + mMsg.MoveName + " Received");
                break;
            default:
                Debug.Log("Unrecognized message received!");
                break;
        }
    }

    void OnConnect()
    {
        Debug.Log("We are now connected to the server");
        StartCoroutine(Heartbeat());
    }

    void OnDisconnect()
    {
        Debug.Log("Client got disconnected from server");
        m_Connection.Disconnect(m_Driver);
        StopAllCoroutines();
        m_Connection = default(NetworkConnection);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

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
