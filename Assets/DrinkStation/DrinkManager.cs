using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DrinkManager : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private GameObject flavours;
    [SerializeField] private GameObject flavoursInfo;

    private static Drink drinkServing = new();

    public static List<Drink> mainDrinks = new() { };
    public static List<Drink> mainDrinksToServe = new();
    public static bool mainClientWaiting => mainDrinksToServe.Count != 0;

    public static List<Drink> secondariesDrinks = new() { }; //!!!: strings with the text to write and to convert to drinks? or drinks and general text/s ?
    public static List<Drink> secondariesDrinksToServe = new();

    public TextMeshProUGUI tipText;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        flavours.SetActive(false);
        flavoursInfo.SetActive(false);
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(mainCam.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;

        Collider2D collider = rayHit.collider;
        string colliderName = collider.name.ToLower();

        if (colliderName.Contains("machine"))
        {
            if (colliderName.Contains("backbtn"))
            {
                SceneManager.LoadScene("Dialogue");
                return;
            }

            TougleFlavours();

            return;
        }

        if (colliderName.Contains("nextbtn") || colliderName.Contains("infobackbtn"))
        {
            flavours.SetActive(!flavours.activeSelf);
            flavoursInfo.SetActive(!flavoursInfo.activeSelf);
            return;
        }

        var colliderParent = collider.transform.parent;
        if (colliderParent != null && colliderParent.parent != null)
        {
            var flavourClicked = colliderParent.parent.name.ToLower().Contains("flavours");
            if (flavourClicked && (mainDrinksToServe.Count != 0 || secondariesDrinksToServe.Count != 0) && !drinkServing.IsReady())
            {
                AddFlavour(colliderName);
                if (drinkServing.IsReady()) //!!! eliminar este if por completo
                {
                    Serve(drinkServing, true);

                    TougleFlavours();
                }
                return;
            }
        }

        /*
        click in campainha && drinkServing.IsReady()
            play sound effect
            string client = choosse who to serve from a list //example: client = "Sara3", when clicking in Sara or Client2 when clicking client #6, (6th client but drink = drink2)
            
            string name = new string(client.Where(char.IsLetter).ToArray());
            string numberString = new string(client.Where(char.IsDigit).ToArray());
            int drinkNumber = int.TryParse(numberString, out int num) ? num : 0; 
            if (drinkNumber == 0)
                return;

            bool clientIsSecondary = name.toLower().contains("client");
            if (clientIsSecondary)
                Serve(drinkServing, FindOrder(name, drinkNumber, secondariesDrinksToServe), false)
            else
                Serve(drinkServing, FindOrder(name, drinkNumber, mainDrinksToServe), true)
        */
    }

    private void TougleFlavours()
    {
        flavours.SetActive(!flavours.activeSelf);

        var tipTextParentGO = tipText.gameObject.transform.parent.gameObject;
        tipTextParentGO.SetActive(!flavours.activeSelf);
        if (tipText.text == "")
            tipTextParentGO.SetActive(false);
    }

    public static void AddFlavour(string flavour, Drink drink = null)
    {
        var flavourDetermined = DetermineFlavour(flavour);

        if (flavourDetermined == null)
            return;

        if (flavourDetermined is BaseFlavour baseFlavour && drinkServing.baseFlavour == BaseFlavour.None)
            if (drink != null)
                drink.baseFlavour = baseFlavour;
            else
                drinkServing.baseFlavour = baseFlavour;

        else if (flavourDetermined is TopFlavour topFlavour && drinkServing.topFlavour == TopFlavour.None)
            if (drink != null)
                drink.topFlavour = topFlavour;
            else
                drinkServing.topFlavour = topFlavour;

        else if (flavourDetermined is SyrupFlavour syrupFlavour && drinkServing.syrupFlavour == SyrupFlavour.None)
            if (drink != null)
                drink.syrupFlavour = syrupFlavour;
            else
                drinkServing.syrupFlavour = syrupFlavour;
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

    private void Serve(Drink drinkServing, bool servingMainNPCs)
    {
        //play sound maquina
        AudioClip soundEffect = Resources.Load<AudioClip>("SoundEffects/" + "sfx_coffe_machine");
        AudioSource.PlayClipAtPoint(soundEffect, Vector3.zero, Music.vfxVolume);

        Drink correctOrder = mainDrinksToServe[0];//!!! enquanto não temos a campainha vai assim

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

        //Feedback feedback = Feedback.feedbacksList.Find(f => f.clientName == correctOrder.client && f.reactionType == feedbackType);
        //Dialogue.InsertAtIndex(feedback.reactionsTxt[rndFeedback], Dialogue.lineIndex + 3);

        if (!flavours.activeSelf)
            tipText.gameObject.transform.parent.gameObject.SetActive(true);

        Money.ReceiveTip(gainXPoints, false, tipText);


        //if (servingMainNPCs)
        //{
        //    mainDrinksToServe.Remove(correctOrder);
        //    mainDrinks.Remove(correctOrder);
        //}
        //else
        //{
        //    secondariesDrinksToServe.Remove(correctOrder);
        //    secondariesDrinks.Remove(correctOrder);
        //}
        mainDrinks.Remove(correctOrder);
        mainDrinksToServe.Remove(correctOrder);

        drinkServing.baseFlavour = BaseFlavour.None;
        drinkServing.topFlavour = TopFlavour.None;
        drinkServing.syrupFlavour = SyrupFlavour.None;

        if (mainDrinksToServe.Count == 0)
            Dialogue.lineIndex++;

        int num = UnityEngine.Random.Range(1, 3);
        CleanManager.clean = num == 1;
        MainCoffeeManager.activeTasks.Add(new(CleanManager.taskTimer, TaskType.Clean));
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