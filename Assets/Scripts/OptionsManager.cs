using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider vfxVolSlider;

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
                audioSource.volume = volumeSlider.value / 100;
        }
    }

    public void OnClickBackBtn()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void OnVfxVolChanged()
    {
        Music.vfxVolume = vfxVolSlider.value / 100;
    }
}
