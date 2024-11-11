using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DrinkManager2 : MonoBehaviour
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
            if (flavourClicked && (mainDrinksToServe.Count != 0 || secondariesDrinksToServe.Count != 0) && !drinkServing.IsReady())
            {
                AddFlavour(colliderName);
                if (drinkServing.IsReady()) //!!! eliminar este if por completo
                {
                    Drink correctOrder = mainDrinks[0];

                    Serve(drinkServing, correctOrder, true);
                }
                return;
            }
        }
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

    private void Serve(Drink drinkServing, Drink correctOrder, bool servingMainNPCs)
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
        //Money.ReceiveTip(gainXPoints, false);

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
        Drink drink = mainDrinksToServe[0];//!!! enquanto não temos a campainha vai assim
        mainDrinks.Remove(drink);
        mainDrinksToServe.Remove(drink);

        drinkServing = new Drink();

        if (mainDrinksToServe.Count == 0)
            Dialogue.lineIndex++;

        int num = UnityEngine.Random.Range(1, 2);
        CleanManager.clean = num == 1;
    }

    public static Drink FindOrder(string name, int drinkNumber, List<Drink> listToFindFrom)
    {
        Drink drink = listToFindFrom.FirstOrDefault(d => d.client.ToLower() == name.ToLower() && d.drinkNumberOfClient == drinkNumber);
        return drink;
    }
}