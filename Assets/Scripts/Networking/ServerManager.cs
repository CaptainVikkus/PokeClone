﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using NetworkMessages;
using UnityEngine.Assertions;
using System.Text;
using System;


public struct Player
{
    public PlayerMessage data;
}

public struct Battle
{
    public BattleMessage data;
    public int playerID;
}

public class ServerManager : MonoBehaviour
{
    private NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    public ushort serverPort = 12345;

    public List<PlayerMessage> matchLobby =
        new List<PlayerMessage>();

    public List<Battle> battleLobby =
        new List<Battle>();

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = serverPort;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port " + serverPort);
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        StartCoroutine(Heartbeat());
    }

    System.Collections.IEnumerator Heartbeat()
    {
        while(this.isActiveAndEnabled)
        {
            Debug.Log("Beat");
            for (int i = 0; i < m_Connections.Length; i++)
            {
                Assert.IsTrue(m_Connections[i].IsCreated);

                Debug.Log("Heartbeat sent to:" + m_Connections[i].InternalId);
                var m = new MessageHeader();
                m.type = MessageType.HEARTBEAT;
                SendToClient(JsonUtility.ToJson(m), m_Connections[i]);
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        // CleanUpConnections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // AcceptNewConnections
        NetworkConnection c = m_Driver.Accept();
        while (c != default(NetworkConnection))
        {
            OnConnect(c);

            // Check if there is another new connection
            c = m_Driver.Accept();
        }

        // Read Incoming Messages
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            Assert.IsTrue(m_Connections[i].IsCreated);

            NetworkEvent.Type cmd;
            cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            while (cmd != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    OnConnect(m_Connections[i]);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    OnData(stream, i);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    OnDisconnect(i);
                }

                cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            }
        }

        //Check PlayerMessages for pairs
        if (matchLobby.Count > 1)
        {
            //Build a Match
            bool first = (UnityEngine.Random.value > 0.5f); //random true false
            var p1 = matchLobby[0];
            var p2 = matchLobby[1];
            SendBattleMessage(p2, first, FindConnection(p1.serverID)); //Send Enemy p2 to Player p1
            SendBattleMessage(p1, !first, FindConnection(p2.serverID)); //Send Enemy p1 to Player p2
            matchLobby.RemoveAt(0);//remove player1
            matchLobby.RemoveAt(0);//remove player2
        }
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void OnConnect(NetworkConnection c)
    {
        m_Connections.Add(c);
        Debug.Log("Accepted a connection");
    }

    void OnData(DataStreamReader stream, int i)
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        MessageHeader header = JsonUtility.FromJson<MessageHeader>(recMsg);

        switch (header.type)
        {
            case MessageType.PLAYER_MSG:
                PlayerMessage pMsg = JsonUtility.FromJson<PlayerMessage>(recMsg);
                Debug.Log("Player message received!: " + pMsg.playerName);
                UpdateLobby(pMsg, i);
                break;
            case MessageType.HEARTBEAT:
                Debug.Log("Heartbeat received from: " + m_Connections[i].InternalId);
                break;
            case MessageType.BATTLE_MSG:
                BattleMessage bMsg = JsonUtility.FromJson<BattleMessage>(recMsg);
                Debug.Log("Battle started for: " + bMsg.playerName);
                AddBattle(bMsg, i);
                break;
            case MessageType.MOVE_MSG:
                Debug.Log("Move received");
                SendMove(recMsg, i);
                break;
            case MessageType.DISCONNECT:
                Debug.Log("Disconnection Received");
                OnDisconnect(i);
                break;
            default:
                Debug.Log("SERVER ERROR: Unrecognized message received!");
                break;
        }
    }

    private void UpdateLobby(PlayerMessage pMsg, int i)
    {
        //Check for duplicate (i.e. leaving player)
        foreach (var player in matchLobby)
        {
            if (player.playerName == pMsg.playerName)
            {
                matchLobby.Remove(player);
                Debug.Log("Player Removed!: " + pMsg.playerName);
                return;
            }
        }
        //No Player Found
        Debug.Log("Player Added!:" + pMsg.playerName);
        pMsg.serverID = m_Connections[i].InternalId;
        matchLobby.Add(pMsg);
    }

    private void AddBattle(BattleMessage bMsg, int i)
    {
        //Check for duplicate (i.e. leaving player)
        foreach (var player in battleLobby)
        {
            if (player.data.playerName == bMsg.playerName)
            {
                battleLobby.Remove(player);
                Debug.Log("Player Removed!: " + bMsg.playerName);
                return;
            }
        }
        var battle = new Battle();
        battle.data = bMsg;
        battle.playerID = m_Connections[i].InternalId;
        battleLobby.Add(battle);
    }

    void SendMove(string move, int i)
    {
        string enemy = "";
        //Find Player's Enemy
        foreach (var battle in battleLobby)
        {
            if (battle.playerID == m_Connections[i].InternalId)
            {
                enemy = battle.data.enemyID;
                break;
            }
        }
        if (enemy != "")
        {
            //Send Move to Enemy
            foreach (var battle in battleLobby)
            {
                if (battle.data.playerName == enemy)
                {
                    SendToClient(move, FindConnection(battle.playerID));
                    return;
                }
            }
        }
    }

    void OnDisconnect(int connection)
    {
        //get the internal id
        int id = m_Connections[connection].InternalId;
        if (matchLobby.Count > 0)
        {
            foreach (var player in matchLobby)
            {
                if (player.serverID == id) { matchLobby.Remove(player); }
            }
        }
        //delete the connection
        Debug.Log("Client disconnected from server");
        m_Connections[connection].Disconnect(m_Driver);
        m_Connections[connection] = default(NetworkConnection);
    }

    void SendBattleMessage(PlayerMessage enemy, bool turn, NetworkConnection c)
    {
        Assert.IsTrue(c.IsCreated);

        var bMsg = new BattleMessage();
        bMsg.turn = turn;
        bMsg.enemyID = enemy.connectionID;
        bMsg.playerName = enemy.playerName;
        bMsg.pokemonName = enemy.pokemonName;
        bMsg.hp = enemy.hp;
        bMsg.Lvl = enemy.Lvl;

        string message = JsonUtility.ToJson(bMsg);
        var writer = m_Driver.BeginSend(NetworkPipeline.Null, c);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    void SendToClient(string message, NetworkConnection c)
    {
        var writer = m_Driver.BeginSend(c);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    NetworkConnection FindConnection(int id)
    {
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (m_Connections[i].InternalId == id)
            {
                return m_Connections[i];
            }
        }

        return default(NetworkConnection);
    }

}
