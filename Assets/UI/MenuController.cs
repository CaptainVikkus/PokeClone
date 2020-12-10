using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public Animator menuButton;
    public GameObject mainMenu;
    public PlayerController player;

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
}
