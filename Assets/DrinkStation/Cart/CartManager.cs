using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CartManager : MonoBehaviour
{
    [SerializeField] private DrinkManager drinkManager;
    [SerializeField] private GameObject GODrinkMachine;
    [SerializeField] private GameObject GOQuantitiesParent;
    [SerializeField] private GameObject GOPricesParent;

    [SerializeField] private Transform contentPanel;

    [SerializeField] private GameObject ingrdItemPrefab;

    [SerializeField] private TextMeshProUGUI playerMoneyTxt;
    [SerializeField] private TextMeshProUGUI cartMoneyTxt;

    public static List<Ingredient> ingredientsToBuy = new();

    private void OnEnable()
    {
        LoadCart();
        UpdateMoneyText();
        UpdateCartMoneyText();
    }

    private void LoadCart()
    {
        ClearCart(false);

        List<Ingredient> ingrdsChecked = new();
        foreach (Ingredient ingrd in ingredientsToBuy)
        {
            if (ingrdsChecked.Contains(ingrd))
                continue;

            GameObject ingrdInstance = Instantiate(ingrdItemPrefab, contentPanel);

            //change quantity showing
            int qtyBuying = ingredientsToBuy.Count(ingrdToBuy => ingrdToBuy == ingrd);
            ingrdInstance.GetComponentInChildren<TextMeshProUGUI>().text = qtyBuying.ToString();

            //change logo
            Transform GOLogoBg = ingrdInstance.transform.Find("LogoBg");
            if (GOLogoBg != null)
            {
                Transform GOLogo = GOLogoBg.Find("Logo");
                if (GOLogo != null)
                    if (GOLogo.TryGetComponent<Image>(out var image))
                        image.sprite = Ingredient.FindIngrdSprite(ingrd.ingrdType.ToString(), ingrd.name);
            }

            //iniciate buttons
            Transform GOPlus = ingrdInstance.transform.Find("Plus");
            if (GOPlus != null)
                GOPlus.GetComponent<Button>().onClick.AddListener(() => Add(ingrd));

            Transform GOMinus = ingrdInstance.transform.Find("Minus");
            if (GOMinus != null)
                GOMinus.GetComponent<Button>().onClick.AddListener(() => Subtract(ingrd));

            ingrdsChecked.Add(ingrd);
        }
    }

    public void BuyIngredients()
    {
        if (ingredientsToBuy.Count <= 0)
            return; //vfx lista pisca vermelho

        List<Ingredient> ingrdsChecked = new();
        List<int> qtyBuyingIngrdsChecked = new();
        foreach (var ingrd in ingredientsToBuy)
        {
            if (!ingrdsChecked.Contains(ingrd))
            {
                int qtyBuying = ingredientsToBuy.Count(ingrdToBuy => ingrdToBuy == ingrd);
                if (ingrd.currQty + qtyBuying > ingrd.maxQty)
                {
                    return; //vfx ingredient pisca vermelho
                }

                ingrdsChecked.Add(ingrd);
                qtyBuyingIngrdsChecked.Add(qtyBuying);
            }
        }

        float totalCost = 0f;
        foreach (var ingrd in ingrdsChecked)
        {
            int qtyBuying = qtyBuyingIngrdsChecked[ingrdsChecked.IndexOf(ingrd)];
            totalCost += qtyBuying * ingrd.price;

            if (Money.playerMoney < totalCost)
                return; //!!!vfx: red warning in the buy btn
        }

        Money.playerMoney -= totalCost;
        UpdateMoneyText();

        foreach (var ingrd in ingrdsChecked)
        {
            int qtyBuying = qtyBuyingIngrdsChecked[ingrdsChecked.IndexOf(ingrd)];
            ingrd.currQty += qtyBuying;
        }

        drinkManager.UpdateIngredientsInfo();

        ClearCart(true);
    }

    private void UpdateMoneyText()
    {
        playerMoneyTxt.text = Money.playerMoney.ToString();
    }

    private void UpdateCartMoneyText()
    {
        if (cartMoneyTxt == null)
            return;

        float totalCost = 0f;
        List<Ingredient> ingrdsChecked = new();
        foreach (var ingrd in ingredientsToBuy)
        {
            if (!ingrdsChecked.Contains(ingrd))
            {
                int qtyBuying = ingredientsToBuy.Count(ingrdToBuy => ingrdToBuy == ingrd);

                totalCost += qtyBuying * ingrd.price;

                ingrdsChecked.Add(ingrd);
            }
        }

        cartMoneyTxt.text = totalCost.ToString();
    }

    private void Add(Ingredient ingrd)
    {
        ingredientsToBuy.Add(ingrd);
        UpdateIngrdInCart(ingrd);
    }

    private void Subtract(Ingredient ingrd)
    {
        ingredientsToBuy.Remove(ingrd);
        UpdateIngrdInCart(ingrd);
    }

    private void UpdateIngrdInCart(Ingredient ingrd)
    {
        foreach (Transform trans in contentPanel)
        {
            Transform GOLogoBg = trans.transform.Find("LogoBg");
            if (GOLogoBg != null)
            {
                Transform GOLogo = GOLogoBg.Find("Logo");
                if (GOLogo != null)
                    if (GOLogo.TryGetComponent<Image>(out var image))
                        if (image.sprite.name.ToLower() == ingrd.name.ToLower())
                        {
                            int qtyBuying = ingredientsToBuy.Count(ingrdToBuy => ingrdToBuy == ingrd);
                            trans.GetComponentInChildren<TextMeshProUGUI>().text = qtyBuying.ToString();

                            if (qtyBuying == 0)
                                Destroy(trans.gameObject);
                        }
            }
        }
        UpdateCartMoneyText();
    }

    public void ClearCart(bool clearList)
    {
        foreach (Transform trans in contentPanel)
            Destroy(trans.gameObject);

        if (clearList)
            ingredientsToBuy.Clear();

        UpdateCartMoneyText();
    }

    public void CloseCart()
    {
        GODrinkMachine.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
