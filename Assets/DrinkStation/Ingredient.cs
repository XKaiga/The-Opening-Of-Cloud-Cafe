using System.Collections.Generic;
using UnityEngine;

public class Ingredient
{
    public static List<Ingredient> ingredientsList = new();

    public string name;
    public IngredientType ingrdType;
    public int currQty; 
    public int maxQty; 
    public float price;

    public Ingredient(string name = "", IngredientType ingrdType = IngredientType.None, int currQty = 3, int maxQty = 7, float price = 5)
    {
        this.name = name;
        this.ingrdType = ingrdType;
        this.currQty = currQty;
        this.maxQty = maxQty;
        this.price = price;
    }

    public static Sprite FindIngrdSprite(string ingrdType, string ingrdName)
    {
        Sprite ingrdSprite = Resources.Load<Sprite>("drinkMachine/Ingredients/" + ingrdType.ToLower() + "/" + ingrdName.ToLower());
        return ingrdSprite;
    }
}

public enum IngredientType
{
    None,
    Syrups,
    Base,
    Toppings
}
