using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioMenager : MonoBehaviour
{
    private AudioSource systemSource;
    private List<AudioSource> activeSources;
    
    #region Singleton
    public static AudioMenager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            systemSource = GetComponent<AudioSource>();
            activeSources = new List<AudioSource>();
        }

        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    /*
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
*/   
    #region AudioController2D

    public void Play(AudioClip clip)
    {
        systemSource.Stop();
        systemSource.clip = clip;
        systemSource.Play();
    }

    public void Stop()
    {
        systemSource.Stop();
    }

    public void Pause()
    {
        systemSource.Pause();
    }

    public void Resume()
    {
        systemSource.UnPause();
    }
    
    
    public void PlayOneShot(AudioClip clip)
    {
        systemSource.PlayOneShot(clip);
    }
    
    
    
    #endregion
    
    #region AudioController3D

    public void Play(AudioClip clip, AudioSource source)
    {
        if (!activeSources.Contains(source))
        {
            activeSources.Add(source);
        }
        systemSource.Stop();
        systemSource.clip = clip;
        systemSource.Play();
    }

    public void Stop(AudioSource source)
    {
        if (activeSources.Contains(source))
        {
            activeSources.Remove(source);
        }
        source.Stop();
    }

    public void Pause(AudioSource source)
    {
        source.Pause();
    }

    public void Resume(AudioSource source)
    {
        source.UnPause();
    }
    
    
    public void PlayOneShot(AudioClip clip, AudioSource source)
    {
        source.PlayOneShot(clip);
    }
    
    
    
    #endregion
    
}
