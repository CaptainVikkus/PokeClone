using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MenuController : MonoBehaviour
{
    public Animator menuButton;
    public GameObject mainMenu;
    public PlayerController player;
    public Image pokeSprite;
    public TextMeshProUGUI lvl;
    public RectTransform health;
    public RectTransform exp;
    private bool getting;

    public void UpdateUI()
    {
        //Set Pokemon sprite to current pokemon
        pokeSprite.sprite = PlayerController.pokemon.Base.FrontSprite;
        float calcHp = PlayerController.pokemon.HP / (float)PlayerController.pokemon.MaxHp;
        //Set healthbar to current pokemon health
        health.localScale = new Vector3(calcHp, 1.0f);
        float calcExp = PlayerController.pokemon.exp / (float)PlayerController.pokemon.exp2NextLevel;
        exp.localScale = new Vector3(calcExp, 1.0f);
        //Set level to pokemone level
        lvl.SetText(PlayerController.pokemon.Level.ToString());
    }

    public void ToggleMenu()
    {
        menuButton.SetBool("Open", !menuButton.GetBool("Open"));
        mainMenu.SetActive(!mainMenu.activeSelf);
    }

    public void SaveGame()
    {
        if (player != null)
        {
            SaveSystem.SavePlayer(player);
            //Debug.Log("Player Saved " + PlayerController.pokemon.Base.name);
        }
        else
        {
            Debug.LogError("PlayerController not Assigned");
        }
    }

    public void SaveGameOnline()
    {
        if (player != null)
        {
            //Build Url
            var pokeSave = Pokemon.CreatePokemonLambda(PlayerController.pokemon);
            string uri = "https://d8nwq2rco8.execute-api.us-east-2.amazonaws.com/default/savePlayerData/"
                + "?Player=" + SaveSystem.currentPlayer
                + "&Pokemon=" + pokeSave.Pokemon
                + "&Health=" + pokeSave.Health
                + "&Level=" + pokeSave.Level;
            UnityWebRequest quest = UnityWebRequest.Get(uri);
            if (!getting)
            {
                getting = true;//enforce only one GET running
                StartCoroutine(SendSave(quest));
            }

            SaveSystem.SavePlayer(player);
            //Debug.Log("Player Saved " + PlayerController.pokemon.Base.name);
        }
        else
        {
            Debug.LogError("PlayerController not Assigned");
        }
    }

    IEnumerator SendSave(UnityWebRequest quest)
    {
        yield return quest.SendWebRequest();

        if (quest.isNetworkError) { Debug.Log("Failed to load Server"); }
        else
        {
            Debug.Log(quest.downloadHandler.text);

        }
    }
}
