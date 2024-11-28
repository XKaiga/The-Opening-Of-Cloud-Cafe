using System;
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

        LoadIngredientsInfo();
    }

    private void LoadIngredientsInfo()
    {
        List<String> ingredientsNameList = new() { "Caramel", "Sugar", "Strawberry", "Honey", "Vanilla", "Chocolate", //syrup
                                                    "Robusta", "Excelsa", "Tea", "Abisca", "Liberica", "Decaf", //base
                                                    "Water", "ColdMilk", "VIC", "Foam", "HotMilk", "Chantilly"}; //toppings

        for (int i = 0; i < ingredientsNameList.Count; i++)
        {
            string ingredientName = ingredientsNameList[i];
            
            IngredientType ingredientType;
            if (i < 6)
                ingredientType = IngredientType.Syrup;
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
            Texture newCharacterTex = Resources.Load<Texture>("Characters/" + character.name);
            character.sprite = newCharacterTex;
        }
    }
}
