using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;

[System.Serializable]
public class SaveData
{
    //Save Info
    public DateTime saveDate;

    //Money class
    public float playerScore;
    public float playerMoney;
    public List<Upgrade> upgrades = new();

    //TrashManager class
    public int currTrashQty;

    //TrashDrag class
    public bool readyToRemoveTrash;

    //CleanManager class
    public bool clean;

    //Music class
    public string currMusicName;

    //MainCoffeeManager class
    public List<Task> activeTasks = new();

    //GameManager class
    public int startDayNum;

    //DrinkManager class
    public List<Drink> mainDrinksToServe = new();
    public List<Drink> secondariesDrinksToServe = new();

    //Dialogue class
    public List<Character> characters = new();
    public int lineIndex;
}
