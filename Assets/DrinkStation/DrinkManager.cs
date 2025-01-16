using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DrinkManager : MonoBehaviour
{
    private Camera mainCam;
    [SerializeField] private TextMeshProUGUI namePanelTxt;
    [SerializeField] private TextMeshProUGUI dialoguePanelTxt;

    [SerializeField] private Dialogue dialogueManager;

    [SerializeField] private Timer drinkTimer;

    [SerializeField] private GameObject GODrinkMachine;
    [SerializeField] private GameObject GODrinkScreen;

    [SerializeField] private GameObject GOCup;
    [SerializeField] private GameObject GOSyrups;
    [SerializeField] private GameObject GOBase;
    [SerializeField] private GameObject GOToppings;

    [SerializeField] private GameObject flavoursInfo;
    [SerializeField] private GameObject cartPanel;

    [SerializeField] private GameObject GOQuantitiesParent;
    [SerializeField] private GameObject GOPricesParent;
    private IngredientType currTabType = IngredientType.Syrups;

    private static Drink drinkServing = new();

    public static List<Drink> mainDrinks = new() { };
    public static List<Drink> mainDrinksToServe = new();
    public static bool mainClientWaiting => mainDrinksToServe.Count != 0;

    public TextMeshProUGUI tipText;
    private bool tipShowing = false;

    private void Awake()
    {
        mainCam = Camera.main;

        if (ScndNPCs.secndClientWaiting)
        {
            float drinkStartTime = MainCoffeeManager.activeTasks.Where(t => t.type == TaskType.NPCOrder) // Filter tasks of type NPCOrder
                                                                .Select(t => t.timer)                   // Select the timer values
                                                                .DefaultIfEmpty(Drink.drinkTaskTimer)         // Provide a default value if no tasks are found
                                                                .Min();                                 // Get the smallest timer value
            drinkTimer.taskType = TaskType.NPCOrder;
            drinkTimer.StartTimer(drinkStartTime);
        }
    }

    [SerializeField] private GameObject tutManager;
    public static bool isDrinkTutorialDone = false;
    private static bool isDrinkTutorialSeen = false;
    private void Start()
    {
        TrashDrink();

        flavoursInfo.SetActive(false);

        GOCup.SetActive(true);

        UpdateIngredientsInfo();
    }

    private void Update()
    {
        if (ScndNPCs.secndClientWaiting && !drinkTimer.timerIsRunning)
        {
            MainCoffeeManager.RemoveTask(TaskType.NPCOrder);
            ScndNPCs.secondariesDrinksToServe.Remove(ScndNPCs.secondariesDrinksToServe[0]);

            StartCoroutine(StartAnotherDrinkTimer());
        }

        if (DrinkManager.isDrinkTutorialDone && !isDrinkTutorialSeen)
        {
            isDrinkTutorialSeen = true;

            tutManager.SetActive(true);
        }
    }

    private IEnumerator StartAnotherDrinkTimer()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3);

        // Does after waiting
        if (ScndNPCs.secndClientWaiting)
        {
            float drinkStartTime = MainCoffeeManager.activeTasks.Where(t => t.type == TaskType.NPCOrder) // Filter tasks of type NPCOrder
                                                                .Select(t => t.timer)                   // Select the timer values
                                                                .DefaultIfEmpty(Drink.drinkTaskTimer)         // Provide a default value if no tasks are found
                                                                .Min();                                 // Get the smallest timer value
            drinkTimer.taskType = TaskType.NPCOrder;
            drinkTimer.StartTimer(drinkStartTime);
        }
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
                if (!Dialogue.isChoosing)
                {
                    StartChangingScene();

                    Dialogue.nameTxtTemp = Dialogue.nameTxt;
                    Dialogue.dialogueTxtTemp = Dialogue.dialogueTxt;

                    Dialogue.nameTxt = namePanelTxt.text;
                    Dialogue.dialogueTxt = dialoguePanelTxt.text;

                    UpdateDrinkTimerOnExit();

                    SceneManager.LoadScene("Dialogue");
                }
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
                    currTabType = IngredientType.Syrups;
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
                ToggleInfoPanel();
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
                bool anyoneWaiting = mainClientWaiting || ScndNPCs.secndClientWaiting;
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
                    CartManager.ingredientsToBuy.Add(ingredientsFromCurrType[posNumber]);
                }
            }
            else
            {
                cartPanel.SetActive(true);
                GODrinkMachine.SetActive(false);
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
                    Sprite flavourSprite = Ingredient.FindIngrdSprite(colliderParent.name.ToLower(), colliderName.ToLower());
                    GODrinkScreen.GetComponent<SpriteRenderer>().sprite = flavourSprite;
                    return;
                }
            }
        }
    }

    public void ToggleInfoPanel()
    {
        GODrinkMachine.SetActive(!GODrinkMachine.activeSelf);
        flavoursInfo.SetActive(!flavoursInfo.activeSelf);

        if (tipShowing)
            tipText.gameObject.transform.parent.gameObject.SetActive(!flavoursInfo.activeSelf);
    }

    public void StartChangingScene()
    {
        if (ScndNPCs.secndClientWaiting)
        {
            Task drinkTaskFound = MainCoffeeManager.activeTasks.Find(task => drinkTimer.gameObject.name.ToLower().Contains(task.type.ToString().ToLower()));
            Text drinkTimerTxt = drinkTimer.gameObject.GetComponentInChildren<Text>();
            if (drinkTaskFound != null)
                if (drinkTimerTxt.isActiveAndEnabled)
                    drinkTaskFound.timer = float.Parse(drinkTimerTxt.text);
                else
                    Debug.Log("drink task error");
        }
    }

    public void UpdateDrinkTimerOnExit()
    {
        if (ScndNPCs.secndClientWaiting)
        {
            Task drinkTaskFound = MainCoffeeManager.activeTasks.Find(task => drinkTimer.gameObject.name.ToLower().Contains(task.type.ToString().ToLower()));
            Text drinkTimerTxt = drinkTimer.gameObject.GetComponentInChildren<Text>();
            if (drinkTaskFound != null)
                if (drinkTimerTxt.isActiveAndEnabled)
                    drinkTaskFound.timer = float.Parse(drinkTimerTxt.text);
                else
                    Debug.Log("drink task error");
        }
    }

    public void UpdateIngredientsInfo()
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

    private void TrashDrink()
    {
        Drink emptyDrink = new Drink();
        int qtyIngrdToTrash = emptyDrink.CompareDrinks(drinkServing);
        if (qtyIngrdToTrash == 0)
            return;

        if (!FirstDrink())
        {
            int numberOfTrash = qtyIngrdToTrash switch
            {
                1 => new[] { 3, 4, 5 }[Random.Range(0, 3)],
                2 => new[] { 6, 7, 8 }[Random.Range(0, 3)],
                3 => new[] { 9, 10, 11 }[Random.Range(0, 3)],
                _ => throw new ArgumentOutOfRangeException(nameof(qtyIngrdToTrash), "qtyIngrdToTrash must be between 1 and 3.")
            };
            TrashManager.FillTrash(numberOfTrash);
        }


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

    private bool FirstDrink()
    {
        if (ScndNPCs.secndClientWaiting)
            return false;

        return (mainClientWaiting && mainDrinksToServe[0].client.ToUpper() == "ALYIA");
    }

    private void Serve()
    {
        //play sound maquina
        AudioClip soundEffect = Resources.Load<AudioClip>("SoundEffects/" + "sfx_coffe_machine");
        AudioSource.PlayClipAtPoint(soundEffect, Vector3.zero, Music.vfxVolume);

        Drink correctOrder;
        if (ScndNPCs.secndClientWaiting)
            correctOrder = ScndNPCs.secondariesDrinksToServe[0];
        else
            correctOrder = mainDrinksToServe[0];

        int rndFeedback = UnityEngine.Random.Range(0, 3);
        FeedbackType feedbackType = FeedbackType.None;
        int differences = drinkServing.CompareDrinks(correctOrder);
        int gainXPoints = 0;
        switch (differences)
        {
            case 0:
                gainXPoints = 250;
                feedbackType = FeedbackType.Good;
                //play nice effect
                AudioClip soundEffect1 = Resources.Load<AudioClip>("SoundEffects/" + "sfx_good_drink");
                AudioSource.PlayClipAtPoint(soundEffect1, Vector3.zero, Music.vfxVolume);
                break;
            case 1:
                gainXPoints = 166;
                feedbackType = FeedbackType.Average;
                //play avg effect
                AudioClip soundEffect2 = Resources.Load<AudioClip>("SoundEffects/" + "sfx_average_drink");
                AudioSource.PlayClipAtPoint(soundEffect2, Vector3.zero, Music.vfxVolume);
                break;
            case 2:
                gainXPoints = 83;
                feedbackType = FeedbackType.Average;
                AudioClip soundEffect3 = Resources.Load<AudioClip>("SoundEffects/" + "sfx_average_drink");
                AudioSource.PlayClipAtPoint(soundEffect3, Vector3.zero, Music.vfxVolume);
                break;
            case 3:
                feedbackType = FeedbackType.Bad;
                //play bad effect
                AudioClip soundEffect4 = Resources.Load<AudioClip>("SoundEffects/" + "sfx_bad_drink");
                AudioSource.PlayClipAtPoint(soundEffect4, Vector3.zero, Music.vfxVolume);
                break;
        }

        Money.playerScore += gainXPoints;

        Feedback feedback = Feedback.feedbacksList.Find(f => f.clientName == correctOrder.client && f.reactionType == feedbackType);
        if (feedback != null)
            Dialogue.InsertAtIndex(feedback.reactionsTxt[rndFeedback], Dialogue.lineIndex + 3);

        tipText.gameObject.transform.parent.gameObject.SetActive(!flavoursInfo.activeSelf);
        tipShowing = !flavoursInfo.activeSelf;

        float drinkBaseCost = drinkServing.GetDrinkCost();
        Money.ReceiveTip(gainXPoints, ScndNPCs.secndClientWaiting, tipText, drinkBaseCost);

        //saving the bool because the values used to determine the bool will be modified
        bool firstDrink = FirstDrink();

        if (ScndNPCs.secndClientWaiting)
        {
            if (drinkTimer.timerIsRunning)
            {
                drinkTimer.StopTimer(drinkTimer.timeRemaining);

                ScndNPCs.secondariesDrinksToServe.Remove(correctOrder);
                //ScndNPCs.secondariesDrinks.Remove(correctOrder);

                Money.AddTaskScore();
            }
        }
        else if (mainClientWaiting)
        {
            mainDrinksToServe.Remove(correctOrder);
            mainDrinks.Remove(correctOrder);
        }

        drinkServing = new Drink();

        if (!firstDrink)
        {
            //if (ScndNPCs.secndClientWaiting) //!!!não sei se faz sentido, acho que não
            //    Dialogue.lineIndex++;

            int num = UnityEngine.Random.Range(0, 3); //1 in 3 chance to be clean
            CleanManager.clean = num == 010;
            if (!CleanManager.clean)
                MainCoffeeManager.activeTasks.Add(new(!TableManager.isCleanTutDone ? 99 : CleanManager.taskTimer, TaskType.Clean));
        }
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
    public const float drinkTaskTimer = 40f;

    public BaseFlavour baseFlavour;
    public TopFlavour topFlavour;
    public SyrupFlavour syrupFlavour;
    public string client;
    public int drinkNumberOfClient;
    public int dayOfTheDrink;

    public string scndOrder;

    public Drink(BaseFlavour baseFlavour = BaseFlavour.None, TopFlavour topFlavour = TopFlavour.None, SyrupFlavour syrupFlavour = SyrupFlavour.None,
                string client = null, int drinkNumberOfClient = 0, int dayOfTheDrink = 0, string scndOrder = null)
    {
        this.baseFlavour = baseFlavour;
        this.topFlavour = topFlavour;
        this.syrupFlavour = syrupFlavour;
        this.client = client;
        this.drinkNumberOfClient = drinkNumberOfClient;
        this.dayOfTheDrink = dayOfTheDrink;
        this.scndOrder = scndOrder;
    }

    public bool IsReady() => syrupFlavour != SyrupFlavour.None && topFlavour != TopFlavour.None && baseFlavour != BaseFlavour.None;

    public float GetDrinkCost()
    {
        float taxesCost = 0.2f;

        Ingredient baseIngrd = Ingredient.ingredientsList.Find(ingrd => ingrd.name == this.baseFlavour.ToString());
        float baseCost = baseIngrd != null ? baseIngrd.price : 0;

        Ingredient topIngrd = Ingredient.ingredientsList.Find(ingrd => ingrd.name == this.topFlavour.ToString());
        float topCost = topIngrd != null ? topIngrd.price : 0;

        Ingredient syrupIngrd = Ingredient.ingredientsList.Find(ingrd => ingrd.name == this.syrupFlavour.ToString());
        float syrupCost = syrupIngrd != null ? syrupIngrd.price : 0;

        float endDrinkCost = (syrupCost + baseCost + topCost) * (1f + taxesCost);
        return endDrinkCost;
    }

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