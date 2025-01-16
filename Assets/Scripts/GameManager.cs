using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static Effects;

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

        FileReader.ReadDrinkFile();

        FileReader.ReadMusicFile();

        FileReader.ReadFeedbackFile();

        FileReader.ReadFinalsFile();

        if (Music.currMusic.AudioClip == null)
            Music.ChangeMusic(Music.musicList[0].AudioClip);

        LoadCharactersSprites();

        LoadIngredientsInfo();
    }
    private void LoadIngredientsInfo()
    {
        List<String> ingredientsNameList = new() { "Caramel", "Sugar", "Strawberry", "Honey", "Vanilla", "Chocolate", //syrup
                                                    "Robusta", "Excelsa", "Tea", "Abisca", "Liberica", "Decaf", //base
                                                    "Water", "VIC", "Foam", "HotMilk", "Chantilly", "ColdMilk"}; //toppings

        for (int i = 0; i < ingredientsNameList.Count; i++)
        {
            string ingredientName = ingredientsNameList[i];

            IngredientType ingredientType;
            if (i < 6)
                ingredientType = IngredientType.Syrups;
            else if (i < 12)
                ingredientType = IngredientType.Base;
            else
                ingredientType = IngredientType.Toppings;

            Ingredient.ingredientsList.Add(new(ingredientName, ingredientType));
        }
    }

    public static void LoadCharactersSprites()
    {
        foreach (var character in Dialogue.characters)
        {
            if (character.sprite != null)
                continue;
            Texture newCharacterTex = Resources.Load<Texture>("Characters/" + character.name + "_Default");
            character.sprite = newCharacterTex;
        }
    }

    public static string GetEmoStringForSprite(Character character, EmotionEnum previousEmotion)
    {
        string emoString = "_Default";

        switch (character.currentEmotion)
        {
            case EmotionEnum.Disgust:
                if (previousEmotion != EmotionEnum.Disgust)
                    emoString = "_Worried";
                else
                    emoString = "_Suprised";
                break;
            case EmotionEnum.Happy:
                emoString = "_Smile";
                break;
            case EmotionEnum.Sad:
            case EmotionEnum.Fear:
                emoString = "_Worried";
                break;
            case EmotionEnum.Angry:
                emoString = "_Anger";
                break;
        }

        return emoString;
    }

    public static EmotionEnum GetEmotionForEffect(EffectType effectType)
    {
        EmotionEnum charEmotion = EmotionEnum.None;

        switch (effectType)
        {
            case EffectType.Pride:
            case EffectType.Joy:
            case EffectType.Laughter:
                charEmotion = EmotionEnum.Happy;
                break;
            case EffectType.Angry:
                charEmotion = EmotionEnum.Angry;
                break;
        }

        return charEmotion;
    }

    public static void UpdateCharacterSprite(Character character, string emoString = "_Default", float delayToDefaultSec = 4.0f)
    {
        if (character == null || character.sprite == null)
            return;

        if (emoString == "_Suprised")
            delayToDefaultSec *= 0.4f;
        else if (emoString == "")
            emoString = "_Default";

        Texture newCharacterTex = Resources.Load<Texture>("Characters/" + character.name + emoString);
        if (newCharacterTex == null)
            newCharacterTex = Resources.Load<Texture>("Characters/" + character.name + "_Default");
        character.sprite = newCharacterTex;

        Dialogue dialogueManager = GameObject.FindFirstObjectByType<Dialogue>();
        if (dialogueManager != null)
            dialogueManager.UpdateCharacterNewSprite(character);

        if (delayToDefaultSec > 0)
        {
            // Start the coroutine with a delay
            MonoBehaviour monoBehaviour = GameObject.FindObjectOfType<MonoBehaviour>();
            if (monoBehaviour != null)
                monoBehaviour.StartCoroutine(RestartCharacterSpriteWithDelay(character, dialogueManager, delayToDefaultSec));
        }
    }

    private static IEnumerator RestartCharacterSpriteWithDelay(Character character, Dialogue dialogueManager, float delay)
    {
        yield return new WaitForSeconds(delay);
        RestartCharacterSprite(character, dialogueManager);
    }

    private static void RestartCharacterSprite(Character character, Dialogue dialogueManager = null)
    {
        if (character == null) return;

        Texture defCharacterTex = Resources.Load<Texture>("Characters/" + character.name + "_Default");
        character.sprite = defCharacterTex;

        if (dialogueManager == null)
            dialogueManager = GameObject.FindFirstObjectByType<Dialogue>();

        if (dialogueManager != null)
            dialogueManager.UpdateCharacterNewSprite(character);

        Debug.Log("Back To Normal");
    }
}
