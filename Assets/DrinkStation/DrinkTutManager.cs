using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrinkTutManager : MonoBehaviour
{
    [SerializeField] private GameObject machine;
    [SerializeField] private GameObject infoBtn;
    [SerializeField] private GameObject backBtn;
    [SerializeField] private GameObject dialoguePanel;

    [SerializeField] private List<Sprite> drinkTutSprites;
    private int currSpriteNum = 0;

    [SerializeField] private Image DrinkTutRawImage;
    private TextMeshProUGUI tutText;

    private readonly List<string> tutLines = new()
        {
            "To make a drink, you’ll need to choose one sweetener/syrup, one base and one topping.",
            "In the top right corner of the machine, you’re able to select what category of what part of the drink you want to " +
            "add. It should be made in a sweetener-base-topping order.",
            "To make a drink, select the desired category and then choose the ingredient by clicking its respective icon.",
            "Then, when you are ready to pour the ingredient onto the cup press the yellow button on the left side of the drink machine.",
            "Repeat the process to the other categories, and then press the green button on the left side of the drink machine to serve it.",
            "If you make a mistake in the process, just press the red button on the left side of the drink machine. Be careful " +
            "because all the already poured ingredients will be going to the trash !!!",
            "As you need more ingredients for your drinks, press the ‘+’ icon to add more to your cart. The ingredient price will be listed on the " +
            "button below, and the quantity of the item you already have in stock is displayed below the ingredient icon.",
            "To buy the ingredients you added to the cart, click on the shopping cart icon on the upper right corner of the coffee machine. " +
            "You’ll see listed the ingredients you added to the cart, their individual price, their total price of the amount added " +
            "to the cart, and the whole ingredient list total displayed.",
            "Just click on the “PURCHASE” button and if you have enough funds, you’ll be able to purchase your goods.",
            "Bonus tip! Press the book icon on the upper right corner of the drink machine to get a better view of what difference each type of coffee " +
            "beans or coffee substitute makes to the drink to better fit the description given by the customer!"
        };

    private void Start()
    {
        tutText = DrinkTutRawImage.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        ToggleOtherGameObjects();
        UpdateTutorial();
    }

    public void OnClickRightArrow()
    {
        if (currSpriteNum < 8)
        {
            currSpriteNum++;
            UpdateTutorial();
        }
        else
        {
            ToggleOtherGameObjects();
            gameObject.SetActive(false);
        }
    }

    public void OnClickLeftArrow()
    {
        if (currSpriteNum > 0)
        {
            currSpriteNum--;
            UpdateTutorial();
        }
    }

    private void UpdateTutorial()
    {
        DrinkTutRawImage.sprite = drinkTutSprites[currSpriteNum];
        tutText.text = tutLines[currSpriteNum];
    }

    private void ToggleOtherGameObjects()
    {
        machine.SetActive(!machine.activeSelf);
        infoBtn.SetActive(!infoBtn.activeSelf);
        backBtn.SetActive(!backBtn.activeSelf);
        dialoguePanel.SetActive(!dialoguePanel.activeSelf);
    }
}
