using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCallerScript : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] clips;

    public int soundID;

    public void PlayCurrentSound()
    {
        audioSource.clip = clips[soundID];
        audioSource.Play();
    }
    public void PlayCurrentSoundOneShot()
    {
        audioSource.PlayOneShot(clips[soundID]);
    }

    public void PlaySound(int soundID)
    {
        audioSource.clip = clips[soundID];
        audioSource.Play();
    }
    public void PlaySoundOneShot(int soundID)
    {
        audioSource.PlayOneShot(clips[soundID]);
    }
}
