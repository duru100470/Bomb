using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType
{
    LobbyBGM,
    GameSceneBGM,
    ButtonClick,
    Jump,
    Explosion,
    Length
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource buttonSource;
    [SerializeField] private List<AudioSource> audioSources;
    Dictionary<AudioType, AudioClip> audioDictionary = new Dictionary<AudioType, AudioClip>();

    [SerializeField] private float initMasterVolume; 
    [SerializeField] private float masterVolume = .5f;
    public float MasterVolume => masterVolume;
    [SerializeField] private float initBGMVolume;
    [SerializeField] private float bgmVolume =.5f;
    public float BGMVolume => bgmVolume;
    [SerializeField] private float initVFXVolume;
    [SerializeField] private float vfxVolume = .5f;
    public float VFXVolume => vfxVolume;

    private void Awake()
    {
        if(Instance != null) Destroy(this.gameObject);
        Instance = this;

        for (int i = 0; i < (int)AudioType.Length; i++)
        {
            audioDictionary.Add((AudioType)i, Resources.Load<AudioClip>($"Audio/{((AudioType)i).ToString()}"));
        }

        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);

        masterVolume = initMasterVolume;
        bgmVolume = initBGMVolume;
        vfxVolume = initVFXVolume;

        VolumeRenew();
    }

    public void AddAudioSource(AudioSource source)
    {
        Debug.Log(source.name);
        audioSources.Add(source);
    }

    public void PlayAudio(AudioType type, int idx)
    {
        AudioSource source = audioSources[idx];
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

    public int SourceIdx(AudioSource source)
    {
        return audioSources.IndexOf(source);
    }

    public void VolumeRenew()
    {
        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetVFXVolume(vfxVolume);
    }

    public void ResetAudioSource()
    {
        audioSources.Clear();
    }

    public void RemoveAudioSource(AudioSource source)
    {
        if(audioSources.Contains(source))
        {
            audioSources.Remove(source);
        }
    }

}