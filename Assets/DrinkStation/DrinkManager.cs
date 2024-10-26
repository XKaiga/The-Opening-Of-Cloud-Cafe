using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DrinkManager : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private GameObject flavours;
    [SerializeField] private GameObject flavoursInfo;

    private Drink drink = new Drink();
    private int indexClientsDrinks = 0;
    public Drink[] clientsDrinks = { new Drink("tea", "hotMilk", "honey") };
    public static bool clientWaiting = false;

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

            flavours.SetActive(!flavours.activeSelf);
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
            if (flavourClicked && clientWaiting)
            {
                AddFlavour(colliderParent.name.ToLower(), colliderName);
                if (drink.IsReady())
                    Serve(drink, clientsDrinks[indexClientsDrinks]);
                return;
            }
        }
    }

    private void AddFlavour(string flavourType, string flavourName)
    {
        switch (flavourType)
        {
            case "base":
                if (drink.baseFlavour == "")
                    drink.baseFlavour = flavourName;
                break;
            case "toppings":
                if (drink.topFlavour == "")
                    drink.topFlavour = flavourName;
                break;
            case "syrups":
                if (drink.syrupFlavour == "")
                    drink.syrupFlavour = flavourName;
                break;
        }
    }

    private void Serve(Drink drinkServing, Drink correctOrder)
    {
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
        Money.ReceiveTip(gainXPoints, false);

        indexClientsDrinks++;
        clientWaiting = false;
        drink = new Drink();

        Dialogue.lineIndex++;

        int num = Random.Range(1, 2);
        CleanManager.clean = num == 1;
    }
    private void Update()
    {
        Debug.Log("client Waiting: " + clientWaiting);
    }
}

public class Drink
{
    public string baseFlavour;
    public string topFlavour;
    public string syrupFlavour;
    public string client;

    public Drink(string baseFlavour = "", string topFlavour = "", string syrupFlavour = "", string client = null)
    {
        this.baseFlavour = baseFlavour;
        this.topFlavour = topFlavour;
        this.syrupFlavour = syrupFlavour;
        this.client = client;
    }

    public bool IsReady() => syrupFlavour != "" && topFlavour != "" && baseFlavour != "";

    public int CompareDrinks(Drink other)
    {
        int differences = 0;

        if (this.baseFlavour.ToLower() != other.baseFlavour.ToLower())
            differences++;

        if (this.topFlavour.ToLower() != other.topFlavour.ToLower())
            differences++;

        if (this.syrupFlavour.ToLower() != other.syrupFlavour.ToLower())
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