using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character
{
    public bool active;

    public string name;

    public EmotionEnum currentEmotion;
    
    public int favoredLevel;

    public string hateMusicTxt;

    public Texture sprite;

    public Character(bool active = false, string name = "", EmotionEnum currEmotion = EmotionEnum.None, int favored = 0, string hateMusicTxt = null, Texture sprite = null)
    {
        this.active = active;
        this.name = name;
        this.currentEmotion = currEmotion;
        this.favoredLevel = favored;
        this.hateMusicTxt = hateMusicTxt;
        this.sprite = sprite;
    }
    public Character(Character character)
    {
        this.active = character.active;
        this.name = character.name;
        this.currentEmotion = character.currentEmotion;
        this.favoredLevel = character.favoredLevel;
        this.hateMusicTxt = character.hateMusicTxt;
        this.sprite = character.sprite;
    }

    public static Character DetermineCharacter(string characterName)
    {
        foreach (var character in Dialogue.characters)
            if (character.name.ToLower() == characterName.ToLower())
                return character;
        
        return null;
    }
}
