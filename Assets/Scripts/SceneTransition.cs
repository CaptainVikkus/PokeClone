using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string targetScene;
    public AudioManager.Track newSong;

    // Update is called once per frame
    public void LoadScene()
    {
        ScreenOverlayManager.Instance.FadeOut();
        AudioManager.Instance.FadeTrack(newSong);
        SceneManager.LoadScene(targetScene);
    }
}
