using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum AudioType
{
    LobbyBGM,
    GameSceneBGM,
    ButtonClick,
    Jump,
    Length
}

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>();
    Dictionary<AudioType, AudioClip> audioDictionary = new Dictionary<AudioType, AudioClip>();
    public bool bgmPlaying = false;

    [SerializeField] private float MasterVolume = .5f;
    [SerializeField] private float BGMVolume =.5f;
    [SerializeField] private float VFXVolume = .5f;

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < (int)AudioType.Length; i++)
        {
            audioDictionary.Add((AudioType)i, Resources.Load<AudioClip>($"Audio/{((AudioType)i).ToString()}"));
        }

        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AddAudioSource(AudioSource source)
    {
        audioSources.Add(source);
    }

    public void AddBGMSource(AudioSource source)
    {
        bgmAudioSource = source;
    }

    public void PlayAudio(AudioType type, int sourceIndex)
    {
        AudioSource source = audioSources[sourceIndex];
        source.loop = false;
        source.clip = audioDictionary[type];
        source.Play();
    }

    public void PlayBGM(AudioType type)
    { 
        Debug.Log(type);
        AudioSource source = bgmAudioSource;
        source.loop = true;
        source.clip = audioDictionary[type];
        source.Play();
    }

    public void SetMasterVolume(float vol)
    {
        foreach(var source in audioSources)
        {
            source.volume = vol * VFXVolume;
        }
        bgmAudioSource.volume = vol * BGMVolume;
        MasterVolume = vol;
    }

    public void SetBGMVolume(float vol)
    {
        bgmAudioSource.volume = vol * MasterVolume;
        BGMVolume = vol;
    }

    public void SetVFXVolume(float vol)
    {
        foreach(var source in audioSources)
        {
            source.volume = vol * MasterVolume;
        }
        VFXVolume = vol;
    }
}