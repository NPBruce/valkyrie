using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Audio : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip eventClip;
    public List<AudioClip> music;
    public bool fadeOut = false;
    public int musicIndex = 0;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.transform.parent = Game.Get().cc.gameObject.transform;
        music = new List<AudioClip>();
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

    public void Play(string file)
    {
        if (file.Length > 0) StartCoroutine(PlayEffect(file));
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
        music = new List<AudioClip>();
        WWW file = new WWW(@"file://" + fileName);
        yield return file;
        audioSource.PlayOneShot(file.audioClip);
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
        audioSource.volume = 1;
        audioSource.clip = music[musicIndex];
        audioSource.Play();
        // Set next music
        musicIndex++;
    }
}
