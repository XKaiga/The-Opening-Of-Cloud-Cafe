using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField slotNumberTxt;

    private void Update()
    {
        if (GameManager.startDayNum != -1 && Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene("Dialogue");
    }

    public void OnClickSaveBtn()
    {
        if (GameManager.startDayNum == -1)
            return;

        bool isValid = GetSlotNumberInput(out int slotNumber);

        if (isValid)
        {
            SaveData saveData = GatherSaveData();
            SaveDataToSlot(slotNumber, saveData);
        }
    }
    public void OnClickLoadBtn()
    {
        bool isValid = GetSlotNumberInput(out int slotNumber);

        if (isValid)
        {
            SaveData saveData = LoadDataFromSlot(slotNumber);
            if (saveData != null)
            {
                LoadSaveData(saveData);
                SceneManager.LoadScene("Dialogue");
            }
        }
    }

    public void OnClickNewGameBtn() { SceneManager.LoadScene("Tutorial"); }

    private bool GetSlotNumberInput(out int result)
    {
        result = -1;

        bool isInt = int.TryParse(slotNumberTxt.text.Trim(), out int slotNumber);
        if (!isInt || slotNumber < 0)
            return false;
        result = slotNumber;
        return true;
    }

    public static SaveData GatherSaveData()
    {
        SaveData saveData = new();

        //get save info
        saveData.saveDate = DateTime.Now;

        saveData.playerScore = Money.playerScore;
        saveData.playerMoney = Money.playerMoney;
        saveData.upgrades = Money.upgrades;

        // Collect trash data
        saveData.currTrashQty = TrashManager.currTrashQty;
        saveData.readyToRemoveTrash = TrashDrag.readyToRemoveTrash;

        // Collect cleaning status
        saveData.clean = CleanManager.clean;

        // Collect current music name
        saveData.currMusicName = Music.currMusic.Name;

        // Collect active tasks
        saveData.activeTasks = MainCoffeeManager.activeTasks;

        // Save starting day number
        saveData.startDayNum = GameManager.startDayNum;

        // Collect drinks to serve
        saveData.mainDrinksToServe = DrinkManager.mainDrinksToServe;
        saveData.secondariesDrinksToServe = DrinkManager.secondariesDrinksToServe;

        // Collect characters data
        List<Character> saveCharactersData = Dialogue.characters
            .Select(character => new Character(character))  
            .ToList();
        saveData.characters = saveCharactersData;
        foreach (var character in saveData.characters)
            character.sprite = null;

        // Store the current line index
        saveData.lineIndex = Dialogue.isChoosing ? Dialogue.lineIndex - 1 : Dialogue.lineIndex;

        return saveData;
    }

    public static void LoadSaveData(SaveData saveData)
    {
        Money.playerScore = saveData.playerScore;
        Money.playerMoney = saveData.playerMoney;
        Money.upgrades = saveData.upgrades;

        // Load trash data
        TrashManager.currTrashQty = saveData.currTrashQty;
        TrashDrag.readyToRemoveTrash = saveData.readyToRemoveTrash;

        // Load cleaning status
        CleanManager.clean = saveData.clean;

        // Load current music name
        Music foundCurrMusic = Music.FindMusicByName(saveData.currMusicName);
        Music.currMusic = foundCurrMusic != null ? foundCurrMusic : null;
        Music.ChangeMusic(Music.musicList[0].AudioClip);

        // Load active tasks
        MainCoffeeManager.activeTasks = saveData.activeTasks;

        // Load starting day number
        GameManager.startDayNum = saveData.startDayNum;

        // Load drinks to serve
        DrinkManager.mainDrinksToServe = saveData.mainDrinksToServe;
        DrinkManager.secondariesDrinksToServe = saveData.secondariesDrinksToServe;

        // Load characters data
        Dialogue.characters = saveData.characters;
        GameManager.LoadCharactersSprites();

        // Load the current line index
        Dialogue.lineIndex = saveData.lineIndex;
    }

    public static void LoadNewGameData()
    {
        Money.playerScore = 0;
        Money.playerMoney = 0;

        List<Upgrade> upgrades = new() {
        new(1, 7, 10, "Bigger Trash Bin"), new(1, 13, 0.1f, "Larger Cloth"),
        new(1, 4, 3, "Extended Task Time"), new(1, 10, 0.05f, "Tip Boost")
        };
        Money.upgrades = upgrades;

        // Load trash data
        TrashManager.currTrashQty = 0;
        TrashDrag.readyToRemoveTrash = false;

        // Load cleaning status
        CleanManager.clean = true;

        // Load current music name
        Music.ChangeMusic(Music.musicList[0].AudioClip);

        // Load active tasks
        MainCoffeeManager.activeTasks = new();

        // Load starting day number
        GameManager.startDayNum = 0;

        // Load drinks to serve
        DrinkManager.mainDrinksToServe = new();
        DrinkManager.secondariesDrinksToServe = new();

        // Load characters data
        Dialogue.characters = new() {
        new Character(name: "ALYIA", hateMusicTxt: "\"Can you put another music plz, i don't really vibe with this one.\""),
        new Character(name: "RONNIE", hateMusicTxt: "\"I hate this music, can you put another.\""),
        new Character(name : "JASPER", hateMusicTxt: "\"Ca...c..can you change the music plz.\""),
        new Character(name : "AMARA", hateMusicTxt: "\"With a song like that it would be better to have zero music.\"")
        };
        GameManager.LoadCharactersSprites();

        // Load the current line index
        Dialogue.lineIndex = -1;
    }

    private static string GetSaveFilePath(int slotNumber)
    {
        return Application.persistentDataPath + "/SaveSlot" + slotNumber + ".json";
    }

    public static void SaveDataToSlot(int slotNumber, SaveData data)
    {
        string path = GetSaveFilePath(slotNumber);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved to slot " + slotNumber + " at path: " + path);
    }

    public static SaveData LoadDataFromSlot(int slotNumber)
    {
        string path = GetSaveFilePath(slotNumber);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"Game loaded from slot {slotNumber}");
            return data;
        }
        Debug.LogWarning("Save file not found");
        return null;
    }

    public static bool SaveSlotExists(int slotNumber)
    {
        return File.Exists(GetSaveFilePath(slotNumber));
    }

    public static void DeleteSaveSlot(int slotNumber)
    {
        string path = GetSaveFilePath(slotNumber);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save in slot " + slotNumber);
        }
    }
}
