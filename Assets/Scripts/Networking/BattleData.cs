using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleData
{
    public static string enemyID = "-1";
    public static string playerName;
    public static string pokemonName;
    public static int Lvl;
    public static int hp;
    public static BattleState turn;

    public static void SetBattleData(NetworkMessages.BattleMessage bMsg)
    {
        enemyID = bMsg.enemyID;
        playerName = bMsg.playerName;
        pokemonName = bMsg.pokemonName;
        Lvl = bMsg.Lvl;
        hp = bMsg.hp;
        turn = bMsg.turn;
    }
}
