using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using NetworkMessages;
using Unity.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;

public class MatchmakingController : MonoBehaviour
{
    [SerializeField] GameObject glow;

    // Start is called before the first frame update
    void Start()
    {
    }

    void AddToLobby()
    {
        NetworkManager.Instance.AddToLobby();
    }

    //** Glow and Trigger Matchmaking On**//
    private void OnTriggerEnter2D(Collider2D collision)
    {
        glow.SetActive(true);
        AddToLobby();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        glow.SetActive(false);
        AddToLobby(); // Second time should remove player
    }
    //** /Glow and Trigger **//
}
