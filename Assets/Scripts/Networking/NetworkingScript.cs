using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkMessages
{
   public enum MessageType
    {
        PLAYER_MSG,
        BATTLE_MSG,
        MOVE_MSG,
        HEARTBEAT
    }

    [System.Serializable]
    public class MessageHeader
    {
        public MessageType type;
    }

    [System.Serializable]
    public class PlayerMessage : MessageHeader
    {
        public string connectionID;
        public int serverID;
        public string playerName;
        public string pokemonName;
        public int Lvl;
        public int hp;
        public PlayerMessage() // Constructor
        {
            type = MessageType.PLAYER_MSG;
            connectionID = "-1"; // Tester for a invalid message
        }
    }

    [System.Serializable]
    public class BattleMessage : MessageHeader
    {
        public string enemyID;
        public string playerName;
        public string pokemonName;
        public int Lvl;
        public int hp;
        public bool turn;
        public BattleMessage() // Constructor
        {
            type = MessageType.BATTLE_MSG;
            enemyID = "-1"; // Tester for an invalid message
        }
    }

    [System.Serializable]
    public class MoveMessage : MessageHeader
    {
        public bool hit;
        public string MoveName;
        public MoveMessage() // Constructor
        {
            type = MessageType.MOVE_MSG;
        }
    }
}
