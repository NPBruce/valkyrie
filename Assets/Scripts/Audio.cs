using UnityEngine;
using System.Collections;

public class Audio : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.transform.parent = Game.Get().cc.gameObject.transform;
    }

    public void Music(string fileName)
    {
        StartCoroutine(LoadMusic(fileName));
    }

    public IEnumerator LoadMusic(string fileName)
    {
        WWW file = new WWW(@"file://" + fileName);
        yield return file;
        audioSource.clip = file.audioClip;
        audioSource.Play();
    }
}
