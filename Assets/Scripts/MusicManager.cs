using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    private MusicManager()
    { }

    static MusicManager instance;

    public MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = this;
            }
            return instance;
        }

        private set { }
    }

    [SerializeField]
    AudioMixer masterMixer;
    [SerializeField]
    AudioSource musicSource;
    [SerializeField]
    AudioClip[] musicTrackes;

    [SerializeField]
    float volumeMax = 0.0f;

    [SerializeField]
    float volumeMin = -0.0f;

    public enum Track
    {
        OVERWORLD,
        BATTLE
    }
    // Start is called before the first frame update
    void Start()
    {
        MusicManager[] others = FindObjectsOfType<MusicManager>();
        foreach (MusicManager mgr in others)
        {
            if (mgr != instance)
            {
                //Destroy(mgr.gameObject);
            }
        }
        DontDestroyOnLoad(transform.root.gameObject);

        FindObjectOfType<GameController>().onEnterEncounter.AddListener(EnterEncounterHandler);
        FindObjectOfType<GameController>().onExitEncounter.AddListener(ExitEncounterHandler);
    }


    private void EnterEncounterHandler()
    {
        FadeInTrack(Track.BATTLE);
    }
    private void ExitEncounterHandler()
    {
        FadeInTrack(Track.OVERWORLD);
    }

    public void PlayTrack(Track trackID)
    {
        musicSource.clip = musicTrackes[(int)trackID];
        musicSource.Play();
    }

    public void FadeInTrack(Track trackID)
    {
        musicSource.volume = 0;
        PlayTrack(trackID);
        StartCoroutine(RaiseVolume(3.0f));
    }

    IEnumerator RaiseVolume(float transitionTime)
    {
        float timer = 0.0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;

            float normalized = timer / transitionTime;

            musicSource.volume = Mathf.SmoothStep(0, 1, normalized);
            yield return new WaitForEndOfFrame();
        }
    }

    public void SetMusicVolume(float normalizedVolume)
    {
        float dbVal = Mathf.Lerp(volumeMin, volumeMax, normalizedVolume);
        masterMixer.SetFloat("Music Volume", dbVal);
    }
}

