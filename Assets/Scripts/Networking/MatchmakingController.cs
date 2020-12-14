using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using NetworkMessages;
using Unity.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;

public class MatchmakingController : MonoBehaviour
{
    [SerializeField] GameObject glow;

    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIP;
    public ushort serverPort;
    [HideInInspector] public static string opponentIP;
    [HideInInspector] public static ushort opponentPort;
    private string myIP;

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.Parse(serverIP, serverPort);
        m_Connection = m_Driver.Connect(endpoint);

        StartCoroutine(GetIPAddress());
    }

    void AddToLobby()
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

    void OnConnect()
    {
        Debug.Log("We are now connected to the server");
        StartCoroutine(Heartbeat());
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

    void OnDisconnect()
    {
        Debug.Log("Client got disconnected from server");
        m_Connection = default(NetworkConnection);
    }

    public void OnDestroy()
    {
        m_Connection.Disconnect(m_Driver);
        OnDisconnect();
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
            case MessageType.HEARTBEAT:
                Debug.Log("Heartbeat");
                break;
            case MessageType.BATTLE_MSG:
                var bMsg = JsonUtility.FromJson<BattleMessage>(recMsg);
                BattleData.SetBattleData(bMsg);
                GameController.Instance.StartMultiplayerBattle();
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
        AddToLobby();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        glow.SetActive(false);
        AddToLobby(); // Second time should remove player
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
}
