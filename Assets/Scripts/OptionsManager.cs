using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public Slider volumeSlider; 

    void Start()
    {
        if (volumeSlider != null)
        {

            GameObject audioObject = GameObject.Find(Music.audioObjectName);
            if (audioObject != null)
            {
                AudioSource audioSource = audioObject.GetComponent<AudioSource>();
                if (audioSource != null)
                    volumeSlider.value = audioSource.volume*100;
            }
        }
    }

    public void OnVolumeChanged()
    {
        GameObject audioObject = GameObject.Find(Music.audioObjectName);
        if (audioObject != null)
        {
            AudioSource audioSource = audioObject.GetComponent<AudioSource>();
            if (audioSource != null)
                audioSource.volume = volumeSlider.value/100;
        }
    }
}
