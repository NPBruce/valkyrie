using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ValkyrieTools;

public class Audio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource audioSourceEffect;
    public GameObject effectsObject;
    public AudioClip eventClip;
    public List<AudioClip> music;
    public bool fadeOut = false;
    public bool fetchingMusic = false;
    public int musicIndex = 0;
    public float effectVolume;
    public float musicVolume;
    public List<AudioClip> defaultQuestMusic;

    void Start()
    {
        Game game = Game.Get();
        audioSource = gameObject.AddComponent<AudioSource>();
        string vSet = game.config.data.Get("UserConfig", "music");
        float.TryParse(vSet, out musicVolume);
        if (vSet.Length == 0) musicVolume = 1;
        audioSource.volume = musicVolume;

        gameObject.transform.SetParent(game.cc.gameObject.transform);
        music = new List<AudioClip>();
        defaultQuestMusic = null;

        effectsObject = new GameObject("audioeffects");
        effectsObject.transform.SetParent(game.cc.gameObject.transform);
        audioSourceEffect = effectsObject.AddComponent<AudioSource>();
        vSet = game.config.data.Get("UserConfig", "effects");
        float.TryParse(vSet, out effectVolume);
        if (vSet.Length == 0) effectVolume = 1;
    }

    private void FixedUpdate()
    {
        if (fadeOut)
        {
            audioSource.volume -= 0.01f;
            if (audioSource.volume <= 0)
            {
                audioSource.volume = 0;
                fadeOut = false;
                audioSource.Stop();
            }
        }
        if (!audioSource.isPlaying)
        {
            audioSource.volume = musicVolume;
            UpdateMusic();
        }
    }

    public void StopMusic()
    {
        StartCoroutine(PlayMusic(new List<string>(), false));
    }
    
    public void PlayDefaultQuestMusic(List<string> fileNames)
    {
        StartCoroutine(PlayMusic(fileNames, true));
    }

    public void PlayMusic(List<string> fileNames)
    {
        StartCoroutine(PlayMusic(fileNames, false));
    }

    public void PlayTrait(string trait)
    {
        List<string> files = new List<string>();
        foreach (AudioData ad in Game.Get().cd.Values<AudioData>())
        {
            if (ad.ContainsTrait(trait))
            {
                files.Add(ad.file);
            }
        }
        if (files.Count > 0) StartCoroutine(PlayEffect(files[Random.Range(0, files.Count)]));
    }

    public void Play(string file)
    {
        if (file.Length > 0) StartCoroutine(PlayEffect(file));
    }

    public void PlayTest()
    {
        AudioClip test = (AudioClip)Resources.Load("test");
        audioSourceEffect.PlayOneShot(test, effectVolume);
    }

    private IEnumerator PlayMusic(List<string> fileNames, bool isDefaultQuestMusic)
    {
        while (fetchingMusic)
        {
            yield return null;
        }
        fetchingMusic = true;
        List<AudioClip> newMusic = new List<AudioClip>();
        foreach (string s in fileNames)
        {
            string fileName = s;
            using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(new System.Uri(fileName).AbsoluteUri, AudioType.UNKNOWN))
            {
                yield return uwr.SendWebRequest();
                if (!uwr.isNetworkError && !uwr.isHttpError)
                {
                    newMusic.Add(UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(uwr));
                }
            }
        }
        music = newMusic;
        if (isDefaultQuestMusic)
        {
            defaultQuestMusic = music;
        }

        musicIndex = 0;
        fetchingMusic = false;
        if (audioSource.isPlaying) fadeOut = true;
    }

    public IEnumerator PlayEffect(string fileName)
    {
        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(new System.Uri(fileName).AbsoluteUri, AudioType.UNKNOWN))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                ValkyrieDebug.Log("Warning: Unable to load audio: " + fileName + " Error: " + uwr.error);
            }
            else
            {
                audioSourceEffect.PlayOneShot(UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(uwr), effectVolume);
            }
        }
    }

    public void UpdateMusic()
    {
        if (music.Count == 0)
            return;

        // if previous music has ended, play or restart default quest music
        if (musicIndex >= music.Count)
        {
            musicIndex = 0;
            if (defaultQuestMusic != null && defaultQuestMusic.Count > 0)
            { 
                music = defaultQuestMusic;
            }
        }

        audioSource.clip = music[musicIndex];
        audioSource.Play();
        // Set next music

        musicIndex++;
    }

    public void StopAudioEffect()
    {
        if(audioSourceEffect!=null)
            audioSourceEffect.Stop();
    }
}
