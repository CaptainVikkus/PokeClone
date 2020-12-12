using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static string currentPlayer;
    public static Pokemon startPokemon;

    ///Loads the current pokemon in PlayerController into the current players save
    public static void SavePlayer(PlayerController controller)
    {
        ///Build Json string from pokemon
        startPokemon = PlayerController.pokemon;
        string strMon = Pokemon.CreatePokemonString(startPokemon);
        ///Write File
        if (!Directory.Exists("Saves/")) { Directory.CreateDirectory("Saves/"); }
        string path = "Saves/" + currentPlayer + ".txt";
        //string path = currentPlayer + ".txt";
        File.WriteAllText(path, strMon);
    }

    ///Set current Player and load pokemon if Player exists
    public static void LoadPlayer(string player)
    {
        currentPlayer = player;
        string path = "Saves/" + currentPlayer + ".txt";
        ///string path = currentPlayer + ".txt";
        ///Check if Player exists
        if (File.Exists(path))
        {
            ///Build pokemon from Json string
            string strMon = File.ReadAllText(path);
            startPokemon = Pokemon.ReadPokemonString(strMon);
            //Debug.Log("Player " + player + " Loaded " + startPokemon.Base.name);
        }
        ///Player Not Found
        else
        {
        }
        ///Set Player's Pokemon to either saved or starter
        PlayerController.pokemon = startPokemon;
    }
}

[System.Serializable]
public class SavePokemon
{
    public string pokemonBaseName;
    public int lvl;
    public int hp;
}