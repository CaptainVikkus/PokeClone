using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public Animator menuButton;
    public GameObject mainMenu;

    public void ToggleMenu()
    {
        menuButton.SetBool("Open", !menuButton.GetBool("Open"));
        mainMenu.SetActive(!mainMenu.activeSelf);
    }
}
