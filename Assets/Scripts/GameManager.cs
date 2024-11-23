using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int startDayNum = -1;

    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FileReader.ReadDrinkFile();

        FileReader.ReadMusicFile();

        FileReader.ReadFeedbackFile();

        Music.ChangeMusic(Music.musicList[0].AudioClip);

        LoadCharactersSprites();
    }

    public static void LoadCharactersSprites()
    {
        foreach (var character in Dialogue.characters)
        {
            if (character.sprite != null)
                continue;
            Texture newCharacterTex = Resources.Load<Texture>("Characters/" + character.name);
            character.sprite = newCharacterTex;
        }
    }
}
