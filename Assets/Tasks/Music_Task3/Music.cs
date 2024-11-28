using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Music
{
    public const string audioObjectName = "Audio Source"; 

    public static Music currMusic = new();
    public static List<Music> musicList = new();

    public static int vfxVolume = 50;

    public string Name { get; set; }
    public EmotionEnum Emotion { get; set; }
    public bool IsDiscovered { get; set; }

    public AudioClip AudioClip { get; set; }

    public Music(string name = "", EmotionEnum emotion = EmotionEnum.None, bool discovered = false, AudioClip audioClip = null)
    {
        Name = name;
        Emotion = emotion;
        IsDiscovered = discovered;
        AudioClip = audioClip;
    }

    public static List<Character> WhoHatesMusic()
    {
        List<Character> charsHateCurrMusic = new();
        foreach (var character in Dialogue.characters.Where(c => c.active))
        {
            if (character.currentEmotion != EmotionEnum.None && character.currentEmotion != currMusic.Emotion)
                charsHateCurrMusic.Add(character);
        }

        return charsHateCurrMusic;
    }

    public static object DetermineEmotion(string emoName)
    {
        if (Enum.TryParse(emoName, true, out EmotionEnum emotion))
            return emotion;

        return null;
    }

    public static Music FindMusicByName(string musicName) => musicList.First(m => musicName.ToLower().Contains(m.Name.ToLower()));
    
    public static void ChangeMusic(AudioClip newAudioClip)
    {
        if (Music.musicList.Count <= 0 || newAudioClip.name == currMusic.Name)
            return;

        GameObject audioObject = GameObject.Find(audioObjectName);
        if (audioObject != null)
        {
            AudioSource audioSource = audioObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = newAudioClip;
                audioSource.Play();
                currMusic = musicList.First(m => m.AudioClip == newAudioClip);
            }
        }
    }
}

[System.Serializable]
public enum EmotionEnum
{
    None,
    Fear,
    Disgust,
    Happy,
    Sad,
    Angry,
    Tense,
    Vulnerable
}
