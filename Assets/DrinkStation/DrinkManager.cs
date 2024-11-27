using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEditor.Progress;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class DrinkManager : MonoBehaviour
{
    private Camera mainCam;
    [SerializeField] private TextMeshProUGUI namePanelTxt;
    [SerializeField] private TextMeshProUGUI dialoguePanelTxt;

    [SerializeField] private Dialogue dialogueManager;

    [SerializeField] private GameObject GODrinkMachine;
    [SerializeField] private GameObject GODrinkScreen;

    [SerializeField] private GameObject GOCup;
    [SerializeField] private GameObject GOSyrups;
    [SerializeField] private GameObject GOBase;
    [SerializeField] private GameObject GOToppings;
    [SerializeField] private GameObject flavoursInfo;

    [SerializeField] private GameObject GOQuantitiesParent;
    [SerializeField] private GameObject GOPricesParent;
    private IngredientType currTabType = IngredientType.Syrup;

    private static Drink drinkServing = new();

    public static List<Drink> mainDrinks = new() { };
    public static List<Drink> mainDrinksToServe = new();
    public static bool mainClientWaiting => mainDrinksToServe.Count != 0;

    public static List<Drink> secondariesDrinks = new() { }; //!!!: strings with the text to write and to convert to drinks? or drinks and general text/s ?
    public static List<Drink> secondariesDrinksToServe = new();
    public static bool secndClientWaiting => secondariesDrinksToServe.Count != 0;

    public TextMeshProUGUI tipText;
    private bool tipShowing = false;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        TrashDrink();

        flavoursInfo.SetActive(false);

        GOCup.SetActive(true);

        UpdateIngredientsInfo();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(mainCam.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;

        Collider2D collider = rayHit.collider;
        string colliderName = collider.name.ToLower();

        if (colliderName.Contains("btn"))
        {
            if (colliderName.Contains("back"))
            {
                Dialogue.pauseBetweenSkips = 0.2f;
                Dialogue.skip = false;
                Dialogue.nameTxt = namePanelTxt.text;
                Dialogue.dialogueTxt = dialoguePanelTxt.text;
                if (!Dialogue.isChoosing)
                    SceneManager.LoadScene("Dialogue");
            }

            else if (colliderName.Contains("tab"))
            {
                if (colliderName.Contains(currTabType.ToString().ToLower()))
                    return;

                GOBase.SetActive(false);
                GOSyrups.SetActive(false);
                GOToppings.SetActive(false);

                if (colliderName.Contains("syrups"))
                {
                    GOSyrups.SetActive(true);
                    currTabType = IngredientType.Syrup;
                }
                else if (colliderName.Contains("base"))
                {
                    GOBase.SetActive(true);
                    currTabType = IngredientType.Base;
                }
                else
                {
                    GOToppings.SetActive(true);
                    currTabType = IngredientType.Toppings;
                }

                UpdateIngredientsInfo();
            }

            else if (colliderName.Contains("info"))
            {
                GODrinkMachine.SetActive(!GODrinkMachine.activeSelf);
                flavoursInfo.SetActive(!flavoursInfo.activeSelf);

                if (tipShowing)
                    tipText.gameObject.transform.parent.gameObject.SetActive(!flavoursInfo.activeSelf);
            }

            else if (colliderName.Contains("pour"))
            {
                if (GODrinkScreen.GetComponent<SpriteRenderer>().sprite != null && !drinkServing.IsReady())
                {
                    string flavourName = GODrinkScreen.GetComponent<SpriteRenderer>().sprite.name.ToLower();
                    bool flavourAdded = AddFlavour(flavourName);

                    if (flavourAdded)
                    {
                        GODrinkScreen.GetComponent<SpriteRenderer>().sprite = null;
                        UpdateIngredientsInfo();
                    }
                }
            }

            else if (colliderName.Contains("serve"))
            {
                bool anyoneWaiting = mainClientWaiting || secndClientWaiting;
                if (anyoneWaiting && drinkServing.IsReady())
                {
                    Serve();
                    RemoveDrink();
                }
            }

            else if (colliderName.Contains("remove"))
                TrashDrink();

            return;
        }

        if (colliderName.Contains("cart"))
        {
            if (colliderName.Contains("ingredient"))
            {
                bool posNumberParsed = int.TryParse(colliderName[0].ToString(), out int posNumber);
                if (!posNumberParsed)
                    return;
                posNumber--;

                List<Ingredient> ingredientsFromCurrType = Ingredient.ingredientsList.FindAll(ingrd => ingrd.ingrdType == currTabType);
                if (ingredientsFromCurrType.Count > 0)
                {
                    //!!!vfx: animation of ingredient going to the cart
                    Money.ingredientsToBuy.Add(ingredientsFromCurrType[posNumber]);
                }
            }
            else
            {
                if (Money.ingredientsToBuy.Count <= 0)
                    return;

                float totalCost = 0f;
                List<Ingredient> ingrdsChecked = new();
                List<int> qtyBuyingIngrdsChecked = new();
                foreach (var ingrd in Money.ingredientsToBuy)
                {
                    if (!ingrdsChecked.Contains(ingrd))
                    {
                        int qtyBuying = Money.ingredientsToBuy.Count(ingrdToBuy => ingrdToBuy == ingrd);
                        if (ingrd.currQty + qtyBuying > ingrd.maxQty)
                            continue;

                        totalCost += ingrd.price;
                        ingrdsChecked.Add(ingrd);
                        qtyBuyingIngrdsChecked.Add(qtyBuying);
                    }
                }

                if (Money.playerMoney < totalCost)
                {
                    //!!!vfx: red warning in the cart btn
                    return;
                }
                Money.playerMoney -= totalCost;

                foreach (var ingrd in ingrdsChecked)
                {
                    int qtyBuying = qtyBuyingIngrdsChecked[ingrdsChecked.IndexOf(ingrd)];
                    ingrd.currQty += qtyBuying;
                }

                UpdateIngredientsInfo();

                Money.ingredientsToBuy.Clear();
            }
            return;
        }

        var colliderParent = collider.transform.parent;
        if (colliderParent != null)
        {
            var flavourBtn = colliderParent.parent;
            if (flavourBtn != null && flavourBtn.parent != null)
            {
                bool flavourClicked = flavourBtn.parent.name.ToLower().Contains("flavours");
                if (flavourClicked && !drinkServing.IsReady())
                {
                    Sprite flavourSprite = Resources.Load<Sprite>("drinkMachine/Ingredients/" + colliderParent.name.ToLower() + "/" + colliderName.ToLower());
                    GODrinkScreen.GetComponent<SpriteRenderer>().sprite = flavourSprite;
                    return;
                }
            }
        }
    }

    private void TrashDrink()
    {
        Drink emptyDrink = new Drink();
        int qtyIngrdToTrash = emptyDrink.CompareDrinks(drinkServing);
        if (qtyIngrdToTrash == 0)
            return;
        int numberOfTrash = qtyIngrdToTrash switch
        {
            1 => new[] { 3, 4, 5 }[Random.Range(0, 3)],
            2 => new[] { 6, 7, 8 }[Random.Range(0, 3)],
            3 => new[] { 9, 10, 11 }[Random.Range(0, 3)],
            _ => throw new ArgumentOutOfRangeException(nameof(qtyIngrdToTrash), "qtyIngrdToTrash must be between 1 and 3.")
        };
        TrashManager.FillTrash(numberOfTrash);

        RemoveDrink();
    }

    private void RemoveDrink()
    {
        drinkServing = new Drink();

        GameObject GOCupSyrp = GameObject.Find("cup" + "Syrups");
        Sprite spriteCup = Resources.Load<Sprite>("drinkMachine/cup/cupSprite");
        GOCupSyrp.GetComponent<SpriteRenderer>().sprite = spriteCup;

        GameObject GOCupBase = GameObject.Find("cup" + "Base");
        GOCupBase.GetComponent<SpriteRenderer>().sprite = null;
        GameObject GOCupTopp = GameObject.Find("cup" + "Toppings");
        GOCupTopp.GetComponent<SpriteRenderer>().sprite = null;
    }

    private void UpdateIngredientsInfo()
    {
        List<Ingredient> ingredientsFromCurrType = Ingredient.ingredientsList.FindAll(ingrd => ingrd.ingrdType == currTabType);

        int i = 0;
        foreach (Transform TransQuantitie in GOQuantitiesParent.transform)
        {
            GameObject GOQuantitie = TransQuantitie.gameObject;
            TextMeshPro QtyText = GOQuantitie.GetComponent<TextMeshPro>();
            QtyText.text = ingredientsFromCurrType[i].currQty + "/" + ingredientsFromCurrType[i].maxQty;
            i++;
        }

        i = 0;
        foreach (Transform TransPrices in GOPricesParent.transform)
        {
            GameObject GOPrice = TransPrices.gameObject;
            TextMeshPro PriceText = GOPrice.GetComponent<TextMeshPro>();
            PriceText.text = ingredientsFromCurrType[i].price + "€";
            i++;
        }
    }

    public static bool AddFlavour(string flavour, Drink drink = null)
    {
        bool flavourAdded = false;

        var flavourDetermined = DetermineFlavour(flavour);

        if (flavourDetermined == null)
            return false;

        string flavourType = "";
        if (flavourDetermined is BaseFlavour baseFlavour && drinkServing.baseFlavour == BaseFlavour.None)
        {
            flavourType = "Base";
            if (drink != null)
                drink.baseFlavour = baseFlavour;
            else
            {
                var ingrdFound = Ingredient.ingredientsList.Find(i => i.name.ToLower().Contains(baseFlavour.ToString().ToLower()) && i.currQty > 0);
                if (ingrdFound != null)
                {
                    ingrdFound.currQty--;
                    drinkServing.baseFlavour = baseFlavour;
                    flavourAdded = true;
                }
                else
                    return false;
            }
        }

        else if (flavourDetermined is TopFlavour topFlavour && drinkServing.topFlavour == TopFlavour.None)
        {
            flavourType = "Toppings";
            if (drink != null)
                drink.topFlavour = topFlavour;
            else
            {
                var ingrdFound = Ingredient.ingredientsList.Find(i => i.name.ToLower().Contains(topFlavour.ToString().ToLower()) && i.currQty > 0);
                if (ingrdFound != null)
                {
                    ingrdFound.currQty--;
                    drinkServing.topFlavour = topFlavour;
                    flavourAdded = true;
                }
                else
                    return false;
            }
        }

        else if (flavourDetermined is SyrupFlavour syrupFlavour && drinkServing.syrupFlavour == SyrupFlavour.None)
        {
            flavourType = "Syrups";
            if (drink != null)
                drink.syrupFlavour = syrupFlavour;
            else
            {
                var ingrdFound = Ingredient.ingredientsList.Find(i => i.name.ToLower().Contains(syrupFlavour.ToString().ToLower()) && i.currQty > 0);
                if (ingrdFound != null)
                {
                    ingrdFound.currQty--;
                    drinkServing.syrupFlavour = syrupFlavour;
                    flavourAdded = true;
                }
                else
                    return false;
            }
        }

        if (drink == null && flavourType != "")
        {
            GameObject GOCupFlavour = GameObject.Find("cup" + flavourType);

            flavour = flavour.ToLower();

            string spriteCupFlavourPath = "drinkMachine/cup/" + flavourType + "/";

            bool topping = flavourType.Contains("Top");
            bool topNeedsBase = false;
            if (topping)
            {
                if (drinkServing.baseFlavour == BaseFlavour.None)
                    return false;

                if (flavour == "water")
                {
                    topNeedsBase = true;
                    spriteCupFlavourPath += "Water/water_";
                }
                else if (flavour.Contains("milk"))
                {
                    topNeedsBase = true;
                    if (flavour.Contains("hot"))
                        spriteCupFlavourPath += "HotMilk/hotMilk_";
                    else
                        spriteCupFlavourPath += "ColdMilk/coldMilk_";
                }
            }

            Sprite spriteCupFlavour = Resources.Load<Sprite>(spriteCupFlavourPath + (topping && topNeedsBase ? drinkServing.baseFlavour.ToString().ToLower() : flavour));

            GOCupFlavour.GetComponent<SpriteRenderer>().sprite = spriteCupFlavour;
        }

        return flavourAdded;
    }

    public static object DetermineFlavour(string flavourName)
    {
        if (Enum.TryParse(flavourName, true, out BaseFlavour baseFlavour) && baseFlavour != BaseFlavour.None)
            return baseFlavour;

        if (Enum.TryParse(flavourName, true, out TopFlavour topFlavour) && topFlavour != TopFlavour.None)
            return topFlavour;

        if (Enum.TryParse(flavourName, true, out SyrupFlavour syrupFlavour) && syrupFlavour != SyrupFlavour.None)
            return syrupFlavour;

        return null;
    }

    private void Serve()
    {
        Drink correctOrder;
        if (secndClientWaiting)
            correctOrder = secondariesDrinksToServe[0];
        else
            correctOrder = mainDrinksToServe[0];

        int differences = drinkServing.CompareDrinks(correctOrder);
        int gainXPoints = 0;
        switch (differences)
        {
            case 0:
                gainXPoints = 250;
                break;
            case 1:
                gainXPoints = 166;
                break;
            case 2:
                gainXPoints = 83;
                break;
        }

        Money.playerScore += gainXPoints;

        tipText.gameObject.transform.parent.gameObject.SetActive(true);
        tipShowing = true;
        Money.ReceiveTip(gainXPoints, secndClientWaiting, tipText);

        if (secndClientWaiting)
        {
            secondariesDrinksToServe.Remove(correctOrder);
            secondariesDrinks.Remove(correctOrder);
        }
        else
        {
            mainDrinksToServe.Remove(correctOrder);
            mainDrinks.Remove(correctOrder);
        }

        drinkServing = new Drink();

        if (secndClientWaiting)
            Dialogue.lineIndex++;

        int num = UnityEngine.Random.Range(0, 3); //1 in 3 chance
        CleanManager.clean = num == 0;
    }

    public static Drink FindOrder(string name, int drinkNumber, List<Drink> listToFindFrom)
    {
        Drink drink = listToFindFrom.FirstOrDefault(d => d.client.ToLower() == name.ToLower() && d.drinkNumberOfClient == drinkNumber);
        return drink;
    }
}

[System.Serializable]
public class Drink
{
    public BaseFlavour baseFlavour;
    public TopFlavour topFlavour;
    public SyrupFlavour syrupFlavour;
    public string client;
    public int drinkNumberOfClient;
    public int dayOfTheDrink;

    public Drink(BaseFlavour baseFlavour = BaseFlavour.None, TopFlavour topFlavour = TopFlavour.None, SyrupFlavour syrupFlavour = SyrupFlavour.None, string client = null, int drinkNumberOfClient = 0, int dayOfTheDrink = 0)
    {
        this.baseFlavour = baseFlavour;
        this.topFlavour = topFlavour;
        this.syrupFlavour = syrupFlavour;
        this.client = client;
        this.drinkNumberOfClient = drinkNumberOfClient;
        this.dayOfTheDrink = dayOfTheDrink;
    }

    public bool IsReady() => syrupFlavour != SyrupFlavour.None && topFlavour != TopFlavour.None && baseFlavour != BaseFlavour.None;

    public int CompareDrinks(Drink other)
    {
        int differences = 0;

        if (this.baseFlavour != other.baseFlavour)
            differences++;

        if (this.topFlavour != other.topFlavour)
            differences++;

        if (this.syrupFlavour != other.syrupFlavour)
            differences++;

        return differences;
    }

    public static bool operator ==(Drink d1, Drink d2)
    {
        if (ReferenceEquals(d1, d2))
            return true;

        if ((object)d1 == null || (object)d2 == null)
            return false;

        return d1.baseFlavour == d2.baseFlavour && d1.topFlavour == d2.topFlavour && d1.syrupFlavour == d2.syrupFlavour;
    }

    public static bool operator !=(Drink d1, Drink d2) => !(d1 == d2);

    public override bool Equals(object obj) => this == (Drink)obj;

    public override int GetHashCode() => baseFlavour.GetHashCode() ^ topFlavour.GetHashCode() ^ syrupFlavour.GetHashCode();
}

public enum BaseFlavour
{
    None,
    Abisca,
    Robusta,
    Liberica,
    Excelsa,
    Decaf,
    Tea
}
public enum TopFlavour
{
    None,
    Water,
    VIC,
    Foam,
    HotMilk,
    Chantilly,
    ColdMilk
}
public enum SyrupFlavour
{
    None,
    Vanilla,
    Strawberry,
    Sugar,
    Caramel,
    Chocolate,
    Honey
}


public class Feedback
{
    public string clientName;
    public ReactionType reactionType;
    public List<string> reactionsTxt;

    public Feedback(string clientName, ReactionType reactionType, List<string> reactionsTxt)
    {
        this.clientName = clientName;
        this.reactionType = reactionType;
        this.reactionsTxt = reactionsTxt;
    }
}

public enum ReactionType
{
    Good,
    Average,
    Bad
}