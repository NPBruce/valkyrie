using UnityEngine;
using System.Collections;

public class Audio : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Music(string fileName)
    {
        StartCoroutine(LoadMusic(fileName));
    }

    public IEnumerator LoadMusic(string fileName)
    {
        WWW file = new WWW(fileName);
        yield return file;
        audioSource.clip = file.audioClip;
        //audioSource.Play();
    }
}
