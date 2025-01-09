using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Money
{
    public static float playerScore = 0;
    public static float playerMoney = 0;

    public static float tipModifier = 0f;

    public static void ReceiveTip(int drinkScore, bool npcSecundario, TextMeshProUGUI tipText, float drinkBaseCost = 0f)
    {
        float tip = npcSecundario ? drinkScore * 0.05f + drinkScore * tipModifier : drinkScore * (0.1f + tipModifier);

        float totalMoneyEarned = (float)Math.Round(drinkBaseCost + tip, 2);
        playerMoney += totalMoneyEarned;
        
        tipText.text = totalMoneyEarned + "€";
    }

    public static void AddTaskScore()
    {
        playerScore += 10;
    }

    //private int GetTasksScore(int tarefasCumpridas, int totalTarefas)
    //{
    //    int score = (int)(tarefasCumpridas * 100) / totalTarefas;
    //    return score;
    //}

    public static void UpdatedMoneyText(Text textComponent)
    {
        if (textComponent == null)
            return;

        textComponent.text = FormatMoneyValue(playerMoney);
    }

    public static string FormatMoneyValue(float money)
    {
        // Round to two decimal places
        float rounded = Mathf.Round(money * 100) / 100;

        // Check if it's an integer
        if (Mathf.Approximately(rounded, Mathf.Floor(rounded)))
            return rounded.ToString("F0") + " €"; // No decimal places if it's a whole number
        else
            return rounded.ToString("F2") + " €"; // Two decimal places otherwise
    }
}

[System.Serializable]
public class Upgrade
{
    public static List<Upgrade> upgradesList = new() {
        new(1, 7, 10, "Bigger Bin"), new(1, 13, 0.1f, "Larger Cloth"),
        new(1, 4, 3, "Extended Timer"), new(1, 10, 0.1f, "Tip Boost"),
        new(1,10,0,"Unlock Music")
    };

    public static bool musicToUnlockExists = false;
    public static bool musicUnlocking = false;

    public int level;
    public float price;
    public float valueAdded;
    public string name;

    public Upgrade(int level = 1, float price = 0, float valueAdded = 0, string name = "")
    {
        this.level = level;
        this.price = price;
        this.valueAdded = valueAdded;
        this.name = name;
    }

    public static Upgrade FindUpgradeByName(string upgName) => upgradesList.First(upg => upgName.ToLower().Contains(upg.name.ToLower()));

    public bool Buy()
    {
        if (Money.playerMoney >= this.price && this.price > 0)
        {
            bool upgDone = ExecuteUpg();
            if (!upgDone)
                return false;

            this.level++;
            Money.playerMoney -= this.price;

            return true;
        }
        return false;
    }

    public static bool BuyMusicUpg()
    {
        musicToUnlockExists = false;
        musicUnlocking = false;

        Upgrade musicUpg = Upgrade.FindUpgradeByName("Unlock Music");

        Money.playerMoney -= musicUpg.price;

        musicUpg.level++;

        return true;
    }

    private bool ExecuteUpg()
    {
        switch (this.name)
        {
            case "Bigger Bin":
                TrashManager.trashMaxQty += (int)this.valueAdded;
                break;

            case "Larger Cloth":
                CleanManager.widthMultiplier += this.valueAdded;
                break;

            case "Extended Timer":
                TrashManager.taskTimer += (int)this.valueAdded;
                CleanManager.taskTimer += (int)this.valueAdded;
                break;

            case "Tip Boost":
                Money.tipModifier += this.valueAdded;
                break;

            case "Unlock Music":
                musicToUnlockExists = true;
                return false; //skips bying the upg, because this upg is only bought when a music is chosen, see BuyMusicUpg()

            default:
                return false;
        }

        return true;
    }
}
