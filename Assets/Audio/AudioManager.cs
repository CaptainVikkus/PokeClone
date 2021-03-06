﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClip[] musicTracks;

    public enum Track
    {
        Title,
        Town,
        Battle,
        Victory,
        Explore
    }

    private AudioManager() { }
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
            }
            return instance;
        }

        private set { }
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioManager[] audioManagers = FindObjectsOfType<AudioManager>();
        foreach (AudioManager mgr in audioManagers)
        {
            if (mgr != Instance)
            {
                Destroy(mgr.gameObject);
            }
        }

        DontDestroyOnLoad(transform.root);
    }

    /// <summary>
    /// switch to selected track
    /// </summary>
    /// <param name="trackID"></param>
    public void PlayTrack(Track trackID)
    {
        audioSource.clip = musicTracks[(int)trackID];
        audioSource.Play();
    }

    public void FadeTrack(Track trackID)
    {
        audioSource.volume = 0;
        PlayTrack(trackID);
        StartCoroutine(RaiseVolume(3.0f));
    }

    IEnumerator RaiseVolume(float transitionTime)
    {
        float timer = 0.0f;
        while(timer < transitionTime)
        {
            timer += Time.deltaTime;
            float normTime = timer / transitionTime;
            audioSource.volume = Mathf.SmoothStep(0, 1, normTime);
            yield return new WaitForEndOfFrame();
        }
    }
}

