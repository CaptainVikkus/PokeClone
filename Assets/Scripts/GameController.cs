using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

public enum GameState { FreeRoam, Battle }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    public UnityEvent onEnterEncounter;
    public UnityEvent onExitEncounter;

    GameState state;

    private void Start()
    {
        
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    void StartBattle()
    {
        onEnterEncounter.Invoke();
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        battleSystem.StartBattle();
    }

    void EndBattle(bool won)
    {
        onExitEncounter.Invoke();
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
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
