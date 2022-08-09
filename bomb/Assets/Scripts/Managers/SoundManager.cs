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
    [SerializeField] private AudioSource buttonSource;
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>();
    Dictionary<AudioType, AudioClip> audioDictionary = new Dictionary<AudioType, AudioClip>();
    public bool bgmPlaying = false;

    [SerializeField] private float masterVolume = .5f;
    public float MasterVolume => masterVolume;
    [SerializeField] private float bgmVolume =.5f;
    public float BGMVolume => bgmVolume;
    [SerializeField] private float vfxVolume = .5f;
    public float VFXVolume => vfxVolume;

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

    public void SetCamSource(AudioSource[] source)
    {
        bgmAudioSource = source[0];
        buttonSource = source[1];
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

    public void PlayUISound(AudioType type)
    {
        AudioSource source = buttonSource;
        source.loop = false;
        source.clip = audioDictionary[type];
        source.Play();
    }

    public void SetMasterVolume(float vol)
    {
        foreach(var source in audioSources)
        {
            source.volume = vol * vfxVolume;
        }
        bgmAudioSource.volume = vol * bgmVolume;
        buttonSource.volume = vol * masterVolume;
        masterVolume = vol;
    }

    public void SetBGMVolume(float vol)
    {
        bgmAudioSource.volume = vol * masterVolume;
        bgmVolume = vol;
    }

    public void SetVFXVolume(float vol)
    {
        foreach(var source in audioSources)
        {
            source.volume = vol * masterVolume;
        }
        buttonSource.volume = vol * masterVolume;
        vfxVolume = vol;
    }
}