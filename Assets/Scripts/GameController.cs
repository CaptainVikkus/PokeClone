using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public enum GameState { FreeRoam, Battle }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] AudioManager audioSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        audioSystem.FadeTrack(AudioManager.Track.Town);
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        battleSystem.StartBattle();
        audioSystem.FadeTrack(AudioManager.Track.Battle);
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        audioSystem.FadeTrack(AudioManager.Track.Town);
    }

    private void Update()
    {
        switch (state)
        {
            case (GameState.FreeRoam):
                playerController.HandleUpdate();
            break;
            case (GameState.Battle):
                battleSystem.HandleUpdate();
            break;
            
        };
    }
}
