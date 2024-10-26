using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Money
{
    public static int playerScore = 0;
    public static float playerMoney = 0;

    private static Upgrade trashUpg = new Upgrade(7,10,"trash");
    private static Upgrade cleanUpg = new Upgrade(13, 0.1f, "clean");

    public static void ReceiveTip(int drinkScore, bool npcSecundario)
    {
        playerMoney = npcSecundario? drinkScore * 0.05f : drinkScore * 0.1f;
    }
    public static void ReceiveTip(bool npcSecundario)
    {
        playerMoney = npcSecundario ? playerScore * 0.05f : playerScore * 0.1f;
    }

    public static void AddTaskScore()
    {
        if (Timer.timerIsRunning)
            playerScore += 10;
    }

    //private int GetTasksScore(int tarefasCumpridas, int totalTarefas)
    //{
    //    int score = (int)(tarefasCumpridas * 100) / totalTarefas;
    //    return score;
    //}
}


public class Upgrade
{
    public float price;
    public float valueAdded;
    public string name;

    public Upgrade(float price = 0, float valueAdded = 0, string name = "")
    {
        this.price = price;
        this.valueAdded = valueAdded;
        this.name = name;
    }

    public bool Buy()
    {
        if (Money.playerMoney >= this.price && this.price > 0)
        {
            Money.playerMoney -= this.price;

            ExecuteUpg();
            return true;
        }
        return false;
    }

    private bool ExecuteUpg()
    {
        switch (this.name)
        {
            case "trash":
                TrashManager.trashMaxQty += (int)this.valueAdded;
                break;
            case "clean":
                CleanManager.widthMultiplier += this.valueAdded;
                break;
        }
        return false;
    }
}
