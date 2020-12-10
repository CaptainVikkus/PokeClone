using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenOverlayManager : MonoBehaviour
{
    [SerializeField] Animator overlayAnimator;

    private ScreenOverlayManager() { }
    private static ScreenOverlayManager instance;
    public static ScreenOverlayManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScreenOverlayManager>();
            }
            return instance;
        }

        private set { }
    }
    // Start is called before the first frame update
    void Start()
    {
        ScreenOverlayManager[] screenOverlayManagers = FindObjectsOfType<ScreenOverlayManager>();
        foreach (ScreenOverlayManager mgr in screenOverlayManagers)
        {
            if (mgr != Instance)
            {
                Destroy(mgr.gameObject);
            }
        }

        DontDestroyOnLoad(transform.root.gameObject);

        //subscribe to encounter events
        GameController.Instance.onEnterEncounter.AddListener(OnEnterCombat);
        GameController.Instance.onExitEncounter.AddListener(OnExitCombat);

        SceneManager.sceneLoaded += OnEnterNewScene;
        SceneManager.sceneUnloaded += OnExitScene;
    }

    void OnEnterCombat()
    {
        overlayAnimator.Play("FadeToBlack");
    }

    void OnExitCombat()
    {
        overlayAnimator.Play("FadeToBlack");
    }

    void OnEnterNewScene(Scene newScene, LoadSceneMode mode)
    {
        overlayAnimator.Play("FadeFromBlack");
    }

    void OnExitScene(Scene scene)
    {
        //overlayAnimator.Play("FadeToBlack");
    }

    public void FadeIn()
    {
        overlayAnimator.Play("FadeFromBlack");
    }
    public void FadeOut()
    {
        overlayAnimator.Play("FadeToBlack");
    }
}
