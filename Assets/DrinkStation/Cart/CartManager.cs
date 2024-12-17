using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

    public static List<Ingredient> ingredientsToBuy = new();

    private void OnEnable()
    {
        LoadCart();
        UpdateMoneyText();
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
            Transform GOLogo = ingrdInstance.transform.Find("Logo");
            if (GOLogo != null)
                if (GOLogo.TryGetComponent<Image>(out var image))
                    image.sprite = Ingredient.FindIngrdSprite(ingrd.ingrdType.ToString(), ingrd.name);

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
        Transform GOMoney = transform.Find("MoneyBg");
        GOMoney.GetComponentInChildren<Text>().text = Money.playerMoney.ToString();
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
            Transform GOLogo = trans.Find("Logo");
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

    public void ClearCart(bool clearList)
    {
        if (ingredientsToBuy.Count != -1)
        {
            foreach (Transform trans in contentPanel)
                Destroy(trans.gameObject);

            if (clearList)
                ingredientsToBuy.Clear();
        }
        else
            CloseCart();
    }

    public void CloseCart()
    {
        GODrinkMachine.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
