using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LoginBehaviour : MonoBehaviour
{
    private TMP_InputField LoginFieldText;
    private bool getting;

    void Start()
    {
        LoginFieldText = GetComponent<TMP_InputField>();
        if (LoginFieldText != null)
        {
            //LoginFieldText.text = "Hello";
            LoginFieldText.Select();
        }
        PokemonBase _base = PokemonBase.ReadBaseMonString("Squirtle");
        SaveSystem.startPokemon = new Pokemon(_base, 5);
    }

    // Check the Login Field for a valid username
    public void CheckCredentials()
    {
        // do not accept nothing
        if (LoginFieldText.text != "")
        {
            Debug.Log("Username Received: " + LoginFieldText.textComponent.GetParsedText());
            //Load Player and Pokemon if available
            SaveSystem.LoadPlayer(LoginFieldText.text);
            SceneManager.LoadScene("Town");
        }
    }

    public void CheckNetworkCredentials()
    {
        if (LoginFieldText.text != "")
        {
            Debug.Log("Username Received: " + LoginFieldText.textComponent.GetParsedText());
            LoadNetworkPlayer(LoginFieldText.text);
        }
    }

        public void PickStarter(string pokeName)
    {
        PokemonBase _base = PokemonBase.ReadBaseMonString(pokeName);
        SaveSystem.startPokemon = new Pokemon(_base, 5);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadNetworkPlayer(string player)
    {
        string uri = "https://d8nwq2rco8.execute-api.us-east-2.amazonaws.com/default/loadPlayerData/?Player="
            + player;
        UnityWebRequest quest = UnityWebRequest.Get(uri);
        if (!getting) 
        {
            getting = true;//enforce only one GET running
            StartCoroutine(GetRequest(quest, player));
        }
    }

    private IEnumerator GetRequest(UnityWebRequest quest, string player)
    {
        SaveSystem.currentPlayer = player;
        ///Wait for request to finish
        yield return quest.SendWebRequest();

        if (quest.isNetworkError) { Debug.Log("Failed to load Server"); }
        else
        {
            Debug.Log(quest.downloadHandler.text);
            if (quest.downloadHandler.text != SaveSystem.currentPlayer)
            { ///Existing Player Found
                var pokeSave = Pokemon.ReadPokemonLambda(quest.downloadHandler.text);
                SaveSystem.startPokemon = pokeSave;
            }
        }
        PlayerController.pokemon = SaveSystem.startPokemon;
        getting = false;//GET finished
        SceneManager.LoadScene("Town");
    }
}
