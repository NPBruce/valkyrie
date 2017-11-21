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
    public List<AudioClip> previousMusic;
    public bool loop = true;

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
        previousMusic = music;

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

    public void Music(string fileName)
    {
        List<string> toPlay = new List<string>();
        toPlay.Add(fileName);
        Music(toPlay);
    }

    public void Music(List<string> fileNames, bool alwaysLoop = true)
    {
        StartCoroutine(PlayMusic(fileNames, alwaysLoop));
    }

    public void PlayTrait(string trait)
    {
        List<string> files = new List<string>();
        foreach (AudioData ad in Game.Get().cd.audio.Values)
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

    public IEnumerator PlayMusic(List<string> fileNames, bool alwaysLoop = true)
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
            var file = new WWW(new System.Uri(fileName).AbsoluteUri);
            yield return file;
            newMusic.Add(file.GetAudioClip());
        }
        music = newMusic;
        if (newMusic.Count > 1 || alwaysLoop)
        {
            previousMusic = music;
        }
        musicIndex = 0;
        fetchingMusic = false;
        if (audioSource.isPlaying) fadeOut = true;
    }

    public IEnumerator PlayEffect(string fileName)
    {
        var file = new WWW(new System.Uri(fileName).AbsoluteUri);
        yield return file;
        if (file.error != null)
        {
            ValkyrieDebug.Log("Warning: Unable to load audio: " + fileName + " Error: " + file.error);
        }
        else
        {
            audioSourceEffect.PlayOneShot(file.GetAudioClip(), effectVolume);
        }
    }

    public void UpdateMusic()
    {
        if (music != previousMusic && loop)
        {
            loop = false;
        }
        else
        {
            loop = true;
            music = previousMusic;
        }
        if (music.Count == 0)
        {
            audioSource.Stop();
            return;
        }
        if (musicIndex >= music.Count)
        {
            musicIndex = 0;
        }
        audioSource.clip = music[musicIndex];
        audioSource.Play();
        // Set next music
        musicIndex++;
    }
}
