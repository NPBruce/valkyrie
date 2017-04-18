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
    public int musicIndex = 0;
    public float effectVolume;
    public float musicVolume;

    void Start()
    {
        Game game = Game.Get();
        audioSource = gameObject.AddComponent<AudioSource>();
        string vSet = game.config.data.Get("UserConfig", "music");
        float.TryParse(vSet, out musicVolume);
        if (vSet.Length == 0) musicVolume = 1;
        audioSource.volume = musicVolume;

        gameObject.transform.parent = game.cc.gameObject.transform;
        music = new List<AudioClip>();

        effectsObject = new GameObject("audioeffects");
        effectsObject.transform.parent = game.cc.gameObject.transform;
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

    public void Music(List<string> fileNames)
    {
        StartCoroutine(PlayMusic(fileNames));
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

    public IEnumerator PlayMusic(List<string> fileNames)
    {
        music = new List<AudioClip>();
        foreach (string s in fileNames)
        {
            WWW file = new WWW(@"file://" + s);
            yield return file;
            music.Add(file.audioClip);
        }
        musicIndex = 0;
        if (audioSource.isPlaying) fadeOut = true;
    }

    public IEnumerator PlayEffect(string fileName)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            fileName = "/" + fileName;
        }
        WWW file = new WWW(@"file://" + fileName);
        yield return file;
        if (file.error != null)
        {
            ValkyrieDebug.Log("Warning: Unable to load audio: " + fileName + " Error: " + file.error);
        }
        else
        {
            audioSourceEffect.PlayOneShot(file.audioClip, effectVolume);
        }
    }

    public void UpdateMusic()
    {
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
