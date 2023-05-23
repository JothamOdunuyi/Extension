using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    AudioSource audioSource;
    GameObject player;
    void Start()
    {
        player = GameObject.Find("First Person Player");
    }

    public void PlaySound(string soundName)
    {
        audioSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        if (audioClips.Any(audio => audio.ToString().Contains(soundName)))
        {
            foreach (AudioClip audio in audioClips)
            {
                if (audio.name == soundName)
                {
                    audioSource.clip = audio;
                    break;
                }
            }
        }
        audioSource.Play();
        StartCoroutine(audioDestroy(audioSource));
    }

    IEnumerator audioDestroy(AudioSource audioSourceToDestroy)
    {
        while (audioSourceToDestroy.isPlaying)
        {
            yield return new WaitForSeconds(.1f);
        }
        Destroy(audioSourceToDestroy);
    }
}