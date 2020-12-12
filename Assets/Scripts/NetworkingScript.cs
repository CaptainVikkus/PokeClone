using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkMessages
{
   public enum MessageType
    {
        PLAYER_MSG,
        BATTLE_MSG,
        MOVE_MSG
    }

    [System.Serializable]
    public class MessageHeader
    {
        public MessageType type;
    }

    [System.Serializable]
    public class PlayerMessage : MessageHeader
    {
        string connectionID;
        string playerName;
        string pokemonName;
        int Lvl;
        int hp;
        public PlayerMessage() // Constructor
        {
            type = MessageType.PLAYER_MSG;
            connectionID = "-1"; // Tester for a invalid message
        }
    }

    [System.Serializable]
    public class BattleMessage : MessageHeader
    {
        string enemyID;
        string playerName;
        string pokemonName;
        int Lvl;
        int hp;
        BattleState turn;
        public BattleMessage() // Constructor
        {
            type = MessageType.BATTLE_MSG;
            enemyID = "-1"; // Tester for an invalid message
        }
    }

    [System.Serializable]
    public class MoveMessage : MessageHeader
    {
        bool hit;
        string MoveName;
        public MoveMessage() // Constructor
        {
            type = MessageType.MOVE_MSG;
        }
    }


}
