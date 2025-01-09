using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;

public class SlotsManager : MonoBehaviour
{
    [SerializeField] private Transform TransContent;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private SaveSystem saveSystem;

    [HideInInspector] public string currSlotSelected = "";

    private void Start()
    {
        LoadSlots();
    }

    public void LoadSlots()
    {
        foreach (Transform trans in TransContent)
            Destroy(trans.gameObject);

        //grab each save file
        List<SaveData> saveFiles = SaveSystem.GetAllSaveData();

        //for each save file instantiate a saveSlot with their info
        foreach (var saveFile in saveFiles)
        {
            GameObject slotInstance = Instantiate(saveSlotPrefab, TransContent);

            //Change text
            slotInstance.GetComponentInChildren<TextMeshProUGUI>().text = $"Slot: {saveFile.name}\n" +
                                                                $"Money: {Money.FormatMoneyValue(saveFile.playerMoney)}\n" +
                                                                $"Date: {saveFile.saveDate}";

            //iniciate button
            slotInstance.GetComponent<Button>().onClick.AddListener(() => OnClickSlot(saveFile.name));
        }
    }

    public void OnClickSlot(string slotClicked)
    {
        currSlotSelected = slotClicked;
    }
}
