using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginBehaviour : MonoBehaviour
{
    private TMP_InputField LoginFieldText;

    void Start()
    {
        LoginFieldText = GetComponent<TMP_InputField>();
        if (LoginFieldText != null)
        {
            //LoginFieldText.text = "Hello";
            LoginFieldText.Select();
        }
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

    public void PickStarter(string pokeName)
    {
        PokemonBase _base = PokemonBase.ReadBaseMonString(pokeName);
        SaveSystem.startPokemon = new Pokemon(_base, 5);
    }
}
