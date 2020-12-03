using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] AudioManager audioManager;
    [SerializeField] ScreenOverlayManager overlayManager;

    public Vector3 targetPlayerLocation = Vector3.zero;
    //public Vector3 prevPlayerLocation;

    public UnityEvent onEnterEncounter;
    public UnityEvent onExitEncounter;

    GameState state;

    private GameController() { }
    private static GameController instance;
    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameController>();
            }
            return instance;
        }

        private set { }
    }

    private void Start()
    {
        GameController[] gameControllers = FindObjectsOfType<GameController>();
        foreach (GameController mgr in gameControllers)
        {
            if (mgr != Instance)
            {
                Destroy(mgr.gameObject);
            }
        }
        DontDestroyOnLoad(transform.root);

        SetManagers();

        audioManager.FadeTrack(AudioManager.Track.Town);
    }

    private void OnLevelWasLoaded(int level)
    {
        SetManagers();

        if (playerController != null & targetPlayerLocation != Vector3.zero)
        {
            playerController.gameObject.transform.position = targetPlayerLocation;
            targetPlayerLocation = Vector3.zero;
        }
    }

    void SetManagers()
    {
        overlayManager = ScreenOverlayManager.Instance;
        audioManager = AudioManager.Instance;

        battleSystem = FindObjectOfType<BattleSystem>();
        if (battleSystem != null) 
        {
            Debug.Log("Battle Loaded");
            battleSystem.OnBattleOver += EndBattle;
            battleSystem.StartBattle();
        }

        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null) 
        {
            Debug.Log("Player Loaded");
            playerController.OnEncountered += StartBattle;
        }
    }


    void StartBattle()
    {
        targetPlayerLocation = playerController.gameObject.transform.position;
        onEnterEncounter.Invoke();
        state = GameState.Battle;
        audioManager.FadeTrack(AudioManager.Track.Battle);
        SceneManager.LoadScene("BattleSystem");
    }

    void EndBattle(bool won)
    {
        onExitEncounter.Invoke();
        state = GameState.FreeRoam;
        audioManager.FadeTrack(AudioManager.Track.Town);
        SceneManager.LoadScene("Town");
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
