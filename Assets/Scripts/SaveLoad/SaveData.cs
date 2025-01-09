using System;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    //Save Info
    public string name;
    public string saveDate;

    //Money class
    public float playerScore;
    public float playerMoney;
    public List<Upgrade> upgrades = new();
    public float tipModifier;

    //TrashManager class
    public int trashMaxQty;
    public int currTrashQty;
    public int taskTimerTrash;

    //TrashDrag class
    public bool readyToRemoveTrash;

    //CleanManager class
    public float widthMultiplier;
    public bool clean;
    public int taskTimerClean;

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
