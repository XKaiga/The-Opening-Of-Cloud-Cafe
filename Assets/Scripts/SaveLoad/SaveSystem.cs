using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private TMP_InputField slotNameTxt;

    private void Update()
    {
        if (GameManager.startDayNum != -1 && Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene("Dialogue");
    }

    public void OnClickSaveBtn()
    {
        if (GameManager.startDayNum == -1)
            return;

        string slotName = slotNameTxt.text;
        string selectedSlotName = slotsManager.currSlotSelected;
        if (slotName == "" && selectedSlotName == "")
            return;

        if (selectedSlotName != "")
            DeleteSaveSlot(selectedSlotName);

        if (slotName == "")
            slotName = selectedSlotName;
        
        SaveData saveData = GatherSaveData(slotName);
        SaveDataToSlot(slotName, saveData);
        slotsManager.LoadSlots();
    }

    public void OnClickLoadBtn()
    {
        if (slotsManager == null || slotsManager.currSlotSelected == "")
            return;

        SaveData saveData = LoadDataFromSlot(slotsManager.currSlotSelected);
        if (saveData != null)
        {
            LoadSaveData(saveData);
            SceneManager.LoadScene("Dialogue");
        }
    }

    public void OnClickMainMenuBtn()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public static SaveData GatherSaveData(string slotName)
    {
        SaveData saveData = new();

        //get save info
        saveData.name = slotName;
        saveData.saveDate = SetDateFormat(DateTime.Now);

        saveData.playerScore = Money.playerScore;
        saveData.playerMoney = Money.playerMoney;
        saveData.upgrades = Upgrade.upgradesList;

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
        saveData.secondariesDrinksToServe = ScndNPCs.secondariesDrinksToServe;

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
        Upgrade.upgradesList = saveData.upgrades;

        // Load trash data
        TrashManager.currTrashQty = saveData.currTrashQty;
        TrashDrag.readyToRemoveTrash = saveData.readyToRemoveTrash;

        // Load cleaning status
        CleanManager.clean = saveData.clean;

        // Load current music name
        Music foundCurrMusic = Music.FindMusicByName(saveData.currMusicName);
        Music music = foundCurrMusic ?? Music.musicList[0];
        Music.ChangeMusic(music.AudioClip);

        // Load active tasks
        MainCoffeeManager.activeTasks = saveData.activeTasks;

        // Load starting day number
        GameManager.startDayNum = saveData.startDayNum;

        // Load drinks to serve
        DrinkManager.mainDrinksToServe = saveData.mainDrinksToServe;
        ScndNPCs.secondariesDrinksToServe = saveData.secondariesDrinksToServe;

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
            new(1, 7, 10, "Bigger Bin"), new(1, 13, 0.1f, "Larger Cloth"),
            new(1, 4, 3, "Extended Timer"), new(1, 10, 0.05f, "Tip Boost"),
            new(1,10,0,"Unlock Music")
        };
        Upgrade.upgradesList = upgrades;

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
        ScndNPCs.secondariesDrinksToServe = new();

        // Load characters data
        Dialogue.characters = new() {
        new Character(name: "ALYIA", patience: 2, hateMusicTxt: "\"Can you put another music plz, i don't really vibe with this one.\""),
        new Character(name: "RONNIE", patience: 4,  hateMusicTxt: "\"I hate this music, can you put another.\""),
        new Character(name : "JASPER", patience: 3,  hateMusicTxt: "\"Ca...c..can you change the music plz.\""),
        new Character(name : "AMARA", patience : 5, hateMusicTxt: "\"With a song like that it would be better to have zero music.\"")
        };
        GameManager.LoadCharactersSprites();

        // Load the current line index
        Dialogue.lineIndex = -1;
    }

    private static string GetSaveFilePath(string slotName)
    {
        return Application.persistentDataPath + "/SaveSlot" + slotName + ".json";
    }

    public static void SaveDataToSlot(string slotName, SaveData data)
    {
        string path = GetSaveFilePath(slotName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved at " + data.saveDate + " to slot " + slotName + " at path: " + path);
    }

    public static SaveData LoadDataFromSlot(string slotName)
    {
        string path = GetSaveFilePath(slotName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }
        Debug.LogWarning("Save file not found");
        return null;
    }

    public static bool SaveSlotExists(string slotName)
    {
        return File.Exists(GetSaveFilePath(slotName));
    }

    public static void DeleteSaveSlot(string slotName)
    {
        string path = GetSaveFilePath(slotName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save in slot " + slotName);
        }
    }

    public static List<SaveData> GetAllSaveData()
    {
        List<SaveData> allSaveData = new List<SaveData>();
        string saveDirectory = Application.persistentDataPath;

        // Get all files matching the "SaveSlot*.json" pattern
        string[] files = Directory.GetFiles(saveDirectory, "SaveSlot*.json");

        foreach (string file in files)
        {
            try
            {
                // Read the file content
                string json = File.ReadAllText(file);

                // Deserialize the JSON into a SaveData object
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // Add the deserialized object to the list
                allSaveData.Add(data);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load save data from file: " + file + "\nError: " + e.Message);
            }
        }

        return allSaveData;
    }

    public static bool LoadLatestSaveSlot()
    {
        string latestSlot = GetLatestSaveSlotName();
        if (latestSlot != "")
        {
            SaveData saveData = LoadDataFromSlot(latestSlot);
            if (saveData != null)
            {
                LoadSaveData(saveData);
                return true;
            }
        }
        return false;
    }

    public static string GetLatestSaveSlotName()
    {
        string saveDirectory = Application.persistentDataPath;
        string latestSlotName = null;
        DateTime latestDate = DateTime.MinValue;

        // Get all files matching the "SaveSlot*.json" pattern
        string[] files = Directory.GetFiles(saveDirectory, "SaveSlot*.json");

        foreach (string file in files)
        {
            try
            {
                // Read the file content
                string json = File.ReadAllText(file);

                // Deserialize the JSON into a SaveData object
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // Try to parse the saveDate into a DateTime
                DateTime parsedDate = GetSaveDate(data.saveDate);

                // Check if this save has the latest date
                if (parsedDate >= latestDate)
                {
                    latestDate = parsedDate;
                    latestSlotName = data.name;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load save data from file: " + file + "\nError: " + e.Message);
            }
        }

        if (latestSlotName == null)
        {
            Debug.LogWarning("No save slots found.");
        }

        return latestSlotName;
    }

    // Convert DateTime to string for saving
    public static string SetDateFormat(DateTime date) => date.ToString("dd/MM/yyyy");

    // Convert string back to DateTime when loading
    public static DateTime GetSaveDate(string dateString)
    {
        return DateTime.TryParseExact(dateString, "dd/MM/yyyy",
                                      System.Globalization.CultureInfo.InvariantCulture,
                                      System.Globalization.DateTimeStyles.None,
                                      out DateTime date) ? date : DateTime.MinValue;
    }
}
