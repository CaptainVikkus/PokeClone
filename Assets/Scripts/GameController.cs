using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Multiplayer, Dialog }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] NetworkBattleSystem multiplayerSystem;
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        //Find Managers
        SetManagers();

        audioManager.FadeTrack(AudioManager.Track.Town);

        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode load)
    {
        SetManagers();

        if (playerController != null)
        {
            if (targetPlayerLocation != Vector3.zero)
            {
                playerController.gameObject.transform.position = targetPlayerLocation;
                targetPlayerLocation = Vector3.zero;
            }
            state = GameState.FreeRoam;
        }
        else if (battleSystem != null)
        {
            state = GameState.Battle;
        }
        else if (multiplayerSystem != null)
        {
            state = GameState.Multiplayer;
        }
        else
        {
            Debug.Log("No Player or Battle Found");
            state = GameState.Dialog;
        }
    }

    void SetManagers()
    {
        overlayManager = ScreenOverlayManager.Instance;
        audioManager = AudioManager.Instance;

        battleSystem = FindObjectOfType<BattleSystem>();
        if (battleSystem != null) 
        {
            Debug.Log("Battle Mode");
            battleSystem.OnBattleOver += EndBattle;
            battleSystem.StartBattle();
        }

        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null) 
        {
            Debug.Log("Adventure Mode");
            playerController.OnEncountered += StartBattle;
        }

        multiplayerSystem = FindObjectOfType<NetworkBattleSystem>();
        if (multiplayerSystem != null)
        {
            Debug.Log("Multiplayer Mode");
            multiplayerSystem.OnBattleOver += EndBattle;
            multiplayerSystem.StartBattle();
        }
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
            case (GameState.Multiplayer):
                multiplayerSystem.HandleUpdate();
                break;
            case (GameState.Dialog):
                break;
        };
    }
    void StartBattle()
    {
        targetPlayerLocation = playerController.gameObject.transform.position;
        onEnterEncounter.Invoke();
        state = GameState.Dialog;
        audioManager.FadeTrack(AudioManager.Track.Battle);
        StartCoroutine(EnterEncounter());
    }

    void EndBattle(bool won)
    {
        onExitEncounter.Invoke();
        state = GameState.FreeRoam;
        audioManager.FadeTrack(AudioManager.Track.Town);
        SceneManager.LoadScene("Town");
    }

    IEnumerator EnterEncounter()
    {
        yield return new WaitForSeconds(2.0f);
        state = GameState.Battle;
        SceneManager.LoadScene("BattleSystem");
    }

    public void StartMultiplayerBattle()
    {
        onEnterEncounter.Invoke();
        state = GameState.Dialog;
        audioManager.FadeTrack(AudioManager.Track.Battle);
        StartCoroutine(EnterEncounterTrainer());
    }

    IEnumerator EnterEncounterTrainer()
    {
        yield return new WaitForSeconds(2.0f);
        state = GameState.Multiplayer;
        SceneManager.LoadScene("NetworkBattleSystem");
    }
}
