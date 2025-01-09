using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MainCoffeeManager : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;

    [SerializeField] private TextMeshProUGUI namePanelTxt;
    [SerializeField] private TextMeshProUGUI dialoguePanelTxt;

    [SerializeField] private Text skipBtnText;
    [SerializeField] private Text playBtnText;

    [SerializeField] private GameObject tasksOpenMenuBtn;
    [SerializeField] private GameObject tasksCloseMenuBtn;
    [SerializeField] private GameObject tasksMenu;
    [SerializeField] private GameObject taskPrefab;
    private static Vector3 taskPrefabPosition;

    [SerializeField] private GameObject musicOpenMenuBtn;
    [SerializeField] private GameObject musicCloseMenuBtn;
    [SerializeField] private GameObject musicMenu;

    [SerializeField] private GameObject upgOpenMenuBtn;
    [SerializeField] private GameObject upgCloseMenuBtn;
    [SerializeField] private GameObject upgMenu;

    [SerializeField] private GameObject markerGO;
    public static List<Task> activeTasks = new();
    private static List<GameObject> activeTasksGameObjs = new();

    private void Start()
    {
        taskPrefabPosition = taskPrefab.transform.position;
        activeTasksGameObjs = new List<GameObject>();
        UpdateTasks();
        LoadMusicMenu();
        LoadUpgMenu();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SceneManager.LoadScene("SaveLoadData");
        }

        if (Upgrade.musicToUnlockExists && !Upgrade.musicUnlocking)
        {
            Upgrade.musicUnlocking = true;

            //fecha update tab
            ToggleUpgradeMenu();

            //abre music tab
            ToggleMusicMenu();
        }
    }

    public void OnClickSkipBtn()
    {
        Dialogue.skip = !Dialogue.skip;
        if (Dialogue.skip)
            skipBtnText.text = "Stop";
        else
            skipBtnText.text = "Skip";
    }

    public void OnClickPlayBtn()
    {
        Dialogue.skip = !Dialogue.skip;
        if (Dialogue.skip)
        {
            Dialogue.pauseBetweenSkips = -2f;
            playBtnText.text = "Pause";
        }
        else
        {
            Dialogue.pauseBetweenSkips = Dialogue.defaultTimeBetweenSkips;
            playBtnText.text = "Play";
        }
    }

    public static void LoadSecndNpcsDrinks()
    {
        int[] indices = CalculateIndices();
        Array.Sort(indices, (a, b) => b.CompareTo(a));

        foreach (var index in indices)
            Dialogue.InsertAtIndex("(DRINK_NPC)", index);
    }

    public static int[] CalculateIndices()
    {
        if (GameManager.startDayNum < 1 || Dialogue.lines == null || Dialogue.lines.Length == 0)
            throw new ArgumentException("Invalid level number or lines array");

        int linesCount = Dialogue.lines.Length;

        int minIndicesCount = (int)(linesCount / 200.0f * (2f + GameManager.startDayNum));
        int maxIndicesCount = (int)(linesCount / 200.0f * (5f + GameManager.startDayNum));
        int indicesCount = Random.Range(minIndicesCount, maxIndicesCount + 1);
        List<int> indices = new();

        int startBound = (int)(linesCount / 10f);
        int endBound = linesCount - startBound;
        int middleIndex = (int)(linesCount / 2f);

        int possibilities = (int)(linesCount - startBound * 2f);

        int spaceLeft = (int)(possibilities / (indicesCount - 1f));
        int spaceRand = (int)(spaceLeft / indicesCount);

        int firstIndexRand = (spaceLeft > middleIndex - startBound) ? spaceRand : spaceLeft;
        int firstIndex = (int)(middleIndex + Random.Range(-firstIndexRand, firstIndexRand + 1));
        indices.Add(firstIndex);

        while (indices.Count < indicesCount)
        {
            bool valid = true;

            // Check the space between the first index and the startBound
            int maxSpaceStart = startBound;
            int maxSpaceEnd = indices[0];
            int maxSpaceSize = maxSpaceEnd - maxSpaceStart;

            // Find the largest space between indices
            for (int i = 0; i < indices.Count - 1; i++)
            {
                int currentSpaceStart = indices[i];
                int currentSpaceEnd = indices[i + 1];
                int currentSpaceSize = currentSpaceEnd - currentSpaceStart;

                if (currentSpaceSize > maxSpaceSize)
                {
                    maxSpaceStart = currentSpaceStart;
                    maxSpaceSize = currentSpaceSize;
                }
            }

            // Check the space between the last index and the endBound
            int endSpaceStart = indices[indices.Count - 1];
            int endSpaceEnd = endBound;
            int endSpaceSize = endSpaceEnd - endSpaceStart;

            if (endSpaceSize > maxSpaceSize)
            {
                maxSpaceStart = endSpaceStart;
                maxSpaceSize = endSpaceSize;
            }

            int middle = maxSpaceStart + maxSpaceSize / 2;
            int randomOffset = Random.Range(-spaceRand, spaceRand + 1);
            int newIndex = Mathf.Clamp(middle + randomOffset, startBound, endBound - 1);

            bool boundReached = false;
            while (Dialogue.lines[newIndex].Contains(")"))
            {
                if (newIndex == startBound || newIndex == endBound - 1)
                {
                    if (boundReached)
                    {
                        valid = false;
                        indicesCount = indices.Count;
                        break;
                    }
                    boundReached = true;
                }
                if (boundReached)
                    newIndex *= -1;

                if (GameManager.startDayNum == 1)
                    newIndex++;
                else
                    newIndex--;

                if (newIndex < 0)
                    newIndex *= -1;

                newIndex = Mathf.Clamp(newIndex, startBound, endBound - 1);
            }

            // Add the new index to the list
            if (valid)
            {
                indices.Add(newIndex);
                indices.Sort();
            }

        }

        return indices.ToArray();
    }

    public static void ActivateNewTask(float timer, TaskType type)
    {
        activeTasks.Add(new Task(timer, type));
    }

    public void UpdateTasks()
    {
        if (TrashDrag.readyToRemoveTrash)
            CreateNewTask(activeTasksGameObjs.Count, "Take out the trash", TaskType.Trash, TrashManager.taskTimer);
        if (!CleanManager.clean)
            CreateNewTask(activeTasksGameObjs.Count, "Clean Table!", TaskType.Clean, CleanManager.taskTimer);
        if (ScndNPCs.secndClientWaiting)
            foreach (var client in ScndNPCs.secondariesDrinksToServe)
                CreateNewTask(activeTasksGameObjs.Count, $"Client {client.drinkNumberOfClient} Waiting!", TaskType.NPCOrder, Drink.drinkTaskTimer);

        if (activeTasksGameObjs.Count > 0)
            ActivateTaskWarning();
    }

    private void ActivateTaskWarning()
    {
        var openImg = tasksOpenMenuBtn.GetComponent<RawImage>();
        var closeImg = tasksCloseMenuBtn.GetComponent<RawImage>();
        StartCoroutine(BlinkRawImageRed(openImg));
        StartCoroutine(BlinkRawImageRed(closeImg));

        StartCoroutine(MoveRedMarker());
    }
    private IEnumerator MoveRedMarker()
    {
        TextMeshProUGUI marker = markerGO.GetComponent<TextMeshProUGUI>();

        markerGO.SetActive(true);
        marker.alignment = TextAlignmentOptions.Bottom;

        float markerDuration = 2.5f;
        float markerInterval = 0.5f;

        float elapsedTime = 0f;
        while (elapsedTime < markerDuration)
        {
            marker.alignment = (marker.alignment == TextAlignmentOptions.Bottom) ? TextAlignmentOptions.Top : TextAlignmentOptions.Bottom;
            yield return new WaitForSeconds(markerInterval);
            elapsedTime += markerInterval;
        }

        marker.alignment = TextAlignmentOptions.Top;
        markerGO.SetActive(false);
    }

    private IEnumerator BlinkRawImageRed(RawImage rawImage)
    {
        Color originalColor = rawImage.color;
        float blinkDuration = 2.5f;
        float blinkInterval = 0.5f;

        float elapsedTime = 0f;
        while (elapsedTime < blinkDuration)
        {
            rawImage.color = (rawImage.color == Color.red) ? originalColor : Color.red;
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        rawImage.color = originalColor;
    }

    private bool ContainsTaskWithType(TaskType taskType)
    {
        bool containsTask = activeTasks.Any(task => task.type == taskType);
        return containsTask;
    }

    private void CreateNewTask(int taskNum, string taskName, TaskType taskType, float timerSec)
    {
        GameObject taskInstance = Instantiate(taskPrefab, tasksMenu.transform);

        TaskTimerMenu taskTimerMenu = taskInstance.GetComponent<TaskTimerMenu>();
        taskTimerMenu.taskType = taskType;

        Vector3 position = taskInstance.transform.localPosition;
        int marginBetweenTasks = 5;
        position.y -= (taskInstance.GetComponent<RectTransform>().rect.height * taskNum + marginBetweenTasks);
        taskInstance.transform.localPosition = position;

        TextMeshProUGUI taskInstanceText = taskInstance.GetComponentInChildren<TextMeshProUGUI>();
        taskInstanceText.text = taskName + "\n";
        //lidar com "Mesas sujas: 2/3"  !!!
        taskInstanceText.text += "Timer: ";

        TaskTimerMenu taskTimer = taskInstance.GetComponentInChildren<TaskTimerMenu>();
        taskTimer.SaveTaskText(taskInstanceText.text);

        taskTimer.timerIsRunning = true;

        if (!ContainsTaskWithType(taskType))
            ActivateNewTask(timerSec, taskType);
        else if (taskType != TaskType.NPCOrder)
            timerSec = activeTasks.First(task => task.type == taskType).timer;
        else
        {
            //get current number
            Match currMatch = Regex.Match(taskName, @"\d+");
            int currTaskNum = -1;
            if (currMatch.Success)
                currTaskNum = int.Parse(currMatch.Value);

            if (!currMatch.Success || currTaskNum == -1)
                return;

            List<Task> drinkTasks = activeTasks.FindAll(task => task.type == taskType);
            int currIndex = ScndNPCs.secondariesDrinksToServe.FindIndex(d => d.drinkNumberOfClient == currTaskNum);
            bool alreadyExists = currIndex < drinkTasks.Count();

            if (alreadyExists)
                timerSec = drinkTasks[currIndex].timer;
            else
                ActivateNewTask(timerSec, taskType);
        }

        taskTimer.timeRemaining = timerSec;

        activeTasksGameObjs.Add(taskInstance);
    }

    public static void RemoveTask(TaskType taskType)
    {
        if (!activeTasks.Any(task => task.type == taskType))
            return;

        Task taskInfo = activeTasks.FirstOrDefault(task => task.type == taskType);
        if (taskInfo == null)
            return;

        activeTasks.Remove(taskInfo);

        switch (taskType)
        {
            case TaskType.Trash:
                TrashDrag.readyToRemoveTrash = false;
                break;
            case TaskType.Clean:
                CleanManager.clean = true;
                break;
        }
    }

    public static void RemoveTaskFromScene(GameObject taskToRemove, TaskType taskType)
    {
        if (!activeTasksGameObjs.Contains(taskToRemove) || !activeTasks.Any(task => task.type == taskType))
            return;

        RemoveTask(taskType);

        activeTasksGameObjs.Remove(taskToRemove);
        Destroy(taskToRemove);

        foreach (var task in activeTasksGameObjs)
        {
            if (task.transform.position == taskPrefabPosition)
                continue;

            Vector3 position = task.transform.localPosition;
            position.y += (task.GetComponent<RectTransform>().rect.height + 5);
            task.transform.localPosition = position;
        }
        /*at least before update:   !!!
         * atualizar que mesa estão limpas ou etc...,
         * ou criar um enum com os diferentes tipos de task e baseado na que foi feita atualizar,
         * ou foreach lista e aumentar o position.y, mas só aumentar das tasks != da primeira[0], fazer antes de remover esta task
         * como vão ser completadas em outros scenes, aqui apenas lida com a coisa de quando o tempo acaba o que acontece???
        */
    }

    public void ToggleTaskMenu()
    {
        RawImage upgOpenMenuBtnImg = upgOpenMenuBtn.GetComponent<RawImage>();
        upgOpenMenuBtnImg.enabled = !upgOpenMenuBtnImg.IsActive();
        RawImage musicOpenMenuBtnImg = musicOpenMenuBtn.GetComponent<RawImage>();
        musicOpenMenuBtnImg.enabled = !musicOpenMenuBtnImg.IsActive();

        RawImage tasksOpenMenuBtnImg = tasksOpenMenuBtn.GetComponent<RawImage>();
        tasksOpenMenuBtnImg.enabled = !tasksOpenMenuBtnImg.IsActive();

        RawImage tasksMenuImg = tasksMenu.GetComponent<RawImage>();
        tasksMenuImg.enabled = !tasksMenuImg.IsActive();

        RawImage tasksCloseMenuBtnImg = tasksCloseMenuBtn.GetComponent<RawImage>();
        tasksCloseMenuBtnImg.enabled = !tasksCloseMenuBtnImg.IsActive();

        foreach (var task in activeTasksGameObjs)
        {
            Image taskImg = task.GetComponent<Image>();
            taskImg.enabled = !taskImg.IsActive();

            TextMeshProUGUI taskText = task.GetComponentInChildren<TextMeshProUGUI>();
            taskText.enabled = !taskText.IsActive();
        }
    }

    public void ToggleMusicMenu()
    {
        if (Dialogue.isMusicDoneVar == false)
        {
            Dialogue.isMusicDoneVar = true;
            dialogue.LoadMusicTutorial();
        }

        RawImage upgOpenMenuBtnImg = upgOpenMenuBtn.GetComponent<RawImage>();
        upgOpenMenuBtnImg.enabled = !upgOpenMenuBtnImg.IsActive();
        RawImage tasksOpenMenuBtnImg = tasksOpenMenuBtn.GetComponent<RawImage>();
        tasksOpenMenuBtnImg.enabled = !tasksOpenMenuBtnImg.IsActive();

        RawImage musicOpenMenuBtnImg = musicOpenMenuBtn.GetComponent<RawImage>();
        musicOpenMenuBtnImg.enabled = !musicOpenMenuBtnImg.IsActive();

        RawImage musicMenuImg = musicMenu.GetComponent<RawImage>();
        musicMenuImg.enabled = !musicMenuImg.IsActive();

        RawImage musicCloseMenuBtnImg = musicCloseMenuBtn.GetComponent<RawImage>();
        musicCloseMenuBtnImg.enabled = !musicCloseMenuBtnImg.IsActive();

        if (Upgrade.musicUnlocking && !musicMenuImg.IsActive())
        {
            Upgrade.musicToUnlockExists = false;
            Upgrade.musicUnlocking = false;
        }

        foreach (Transform music in musicMenu.transform)
        {
            GameObject musicObject = music.gameObject;

            Image musicImg = musicObject.GetComponent<Image>();
            musicImg.enabled = !musicImg.IsActive();

            TextMeshProUGUI musicText = musicObject.GetComponentInChildren<TextMeshProUGUI>();
            musicText.enabled = !musicText.IsActive();

            Button button = musicObject.GetComponent<Button>();
            if (button == null)
                button = musicObject.AddComponent<Button>();

            button.onClick.AddListener(() => OnMusicClicked(musicObject));
        }
    }

    public void ToggleUpgradeMenu()
    {
        RawImage tasksOpenMenuBtnImg = tasksOpenMenuBtn.GetComponent<RawImage>();
        tasksOpenMenuBtnImg.enabled = !tasksOpenMenuBtnImg.IsActive();
        RawImage musicOpenMenuBtnImg = musicOpenMenuBtn.GetComponent<RawImage>();
        musicOpenMenuBtnImg.enabled = !musicOpenMenuBtnImg.IsActive();

        RawImage upgOpenMenuBtnImg = upgOpenMenuBtn.GetComponent<RawImage>();
        upgOpenMenuBtnImg.enabled = !upgOpenMenuBtnImg.IsActive();

        RawImage upgMenuImg = upgMenu.GetComponent<RawImage>();
        upgMenuImg.enabled = !upgMenuImg.IsActive();

        RawImage upgCloseMenuBtnImg = upgCloseMenuBtn.GetComponent<RawImage>();
        upgCloseMenuBtnImg.enabled = !upgCloseMenuBtnImg.IsActive();

        foreach (Transform upg in upgMenu.transform)
        {
            GameObject upgObject = upg.gameObject;

            Image upgImg = upgObject.GetComponent<Image>();
            upgImg.enabled = !upgImg.IsActive();

            TextMeshProUGUI upgText = upgObject.GetComponentInChildren<TextMeshProUGUI>();
            upgText.enabled = !upgText.IsActive();

            Button button = upgObject.GetComponent<Button>();
            if (button == null)
                button = upgObject.AddComponent<Button>();

            button.onClick.AddListener(() => OnUpgClicked(upgObject));
        }
    }

    void OnUpgClicked(GameObject upg)
    {
        TextMeshProUGUI upgTextComponent = upg.GetComponentInChildren<TextMeshProUGUI>();

        string[] separateUpgText = upgTextComponent.text.Split('\n');
        if (separateUpgText.Length > 0)
        {
            string upgName = separateUpgText[0];
            Upgrade upgFound = Upgrade.FindUpgradeByName(upgName);
            if (upgFound != null)
            {
                upgFound.Buy();
                TextMeshProUGUI menuItemInstanceText = upg.GetComponentInChildren<TextMeshProUGUI>();
                if (upgFound.name.Contains("Music"))
                    menuItemInstanceText.text = upgFound.name + "\n";
                else
                    menuItemInstanceText.text = upgFound.name + " Lvl" + upgFound.level + "\n";

                menuItemInstanceText.text += "Price: " + upgFound.price + "€";
            }
        }
    }

    private void LoadUpgMenu()
    {
        int upgNum = -1;
        int marginBetweenTasks = 5;

        foreach (var upg in Upgrade.upgradesList)
        {
            upgNum++;

            GameObject menuItemInstance = Instantiate(taskPrefab, upgMenu.transform);

            Vector3 position = menuItemInstance.transform.localPosition;
            position.y -= (menuItemInstance.GetComponent<RectTransform>().rect.height * upgNum + marginBetweenTasks);
            menuItemInstance.transform.localPosition = position;

            TextMeshProUGUI menuItemInstanceText = menuItemInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (upg.name == "Unlock Music")
                menuItemInstanceText.text = upg.name + "\n";
            else
                menuItemInstanceText.text = upg.name + " Lvl" + upg.level + "\n";
            menuItemInstanceText.text += "Price: " + upg.price + "$";
        }
    }

    void OnMusicClicked(GameObject music)
    {
        TextMeshProUGUI musicTextComponent = music.GetComponentInChildren<TextMeshProUGUI>();

        string[] separateMusicText = musicTextComponent.text.Split('\n');
        if (separateMusicText.Length > 0)
        {
            string musicName = separateMusicText[0];
            Music musicFound = Music.FindMusicByName(musicName);
            if (musicFound != null)
            {
                if (Upgrade.musicUnlocking && Upgrade.musicToUnlockExists)
                {
                    if (!musicFound.IsDiscovered)
                    {
                        Upgrade.BuyMusicUpg();

                        Music m = Music.musicList.First(m => m.Name == musicFound.Name);
                        musicFound.IsDiscovered = true;

                        TextMeshProUGUI menuItemInstanceText = music.GetComponentInChildren<TextMeshProUGUI>();
                        menuItemInstanceText.text = musicFound.Name + "\n";
                        menuItemInstanceText.text += "Type: " + (musicFound.IsDiscovered ? musicFound.Emotion.ToString() : "Unknown");

                        ToggleMusicMenu();
                    }
                }
                else
                {
                    Music.ChangeMusic(musicFound.AudioClip);
                    Music.currMusic = musicFound;
                }
            }
        }
    }

    private void LoadMusicMenu()
    {
        int musicNum = -1;
        int marginBetweenTasks = 5;

        foreach (var music in Music.musicList)
        {
            musicNum++;

            GameObject menuItemInstance = Instantiate(taskPrefab, musicMenu.transform);

            Vector3 position = menuItemInstance.transform.localPosition;
            position.y -= (menuItemInstance.GetComponent<RectTransform>().rect.height * musicNum + marginBetweenTasks);
            menuItemInstance.transform.localPosition = position;

            TextMeshProUGUI menuItemInstanceText = menuItemInstance.GetComponentInChildren<TextMeshProUGUI>();
            menuItemInstanceText.text = music.Name + "\n";
            menuItemInstanceText.text += "Type: " + (music.IsDiscovered ? music.Emotion.ToString() : "Unknown");
        }
    }

    public void LoadDrinkStationScene()
    {
        if (!Dialogue.isChoosing && !Dialogue.startingNewDay)
        {
            //!!! juntar a função LoadDrinkStationScene e LoadTablesScene, para fazer o -- e recebendo uma string ou enum com o nome das scenes mudar para essa tal
            Dialogue.skip = true;
            Dialogue.pauseBetweenSkips = -2f;

            Dialogue.nameTxtTemp = Dialogue.nameTxt;
            Dialogue.dialogueTxtTemp = Dialogue.dialogueTxt;

            Dialogue.nameTxt = namePanelTxt.text;
            Dialogue.dialogueTxt = dialoguePanelTxt.text;

            SceneManager.LoadScene("DrinkStation");
        }
    }
    public void LoadTablesScene()
    {
        if (!Dialogue.isChoosing && !Dialogue.startingNewDay)
        {
            Dialogue.skip = true;
            Dialogue.pauseBetweenSkips = -2f;

            Dialogue.nameTxtTemp = Dialogue.nameTxt;
            Dialogue.dialogueTxtTemp = Dialogue.dialogueTxt;

            Dialogue.nameTxt = namePanelTxt.text;
            Dialogue.dialogueTxt = dialoguePanelTxt.text;

            SceneManager.LoadScene("Tables");
        }
    }
}

[System.Serializable]
public enum TaskType
{
    None,
    Trash,
    Clean,
    Music,
    NPCOrder
}

[System.Serializable]
public class Task
{
    public float timer;
    public TaskType type;

    public Task(float timer, TaskType type)
    {
        this.timer = timer;
        this.type = type;
    }
}