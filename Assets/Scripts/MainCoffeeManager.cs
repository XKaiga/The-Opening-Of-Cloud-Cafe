using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainCoffeeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI namePanelTxt;
    [SerializeField] private TextMeshProUGUI dialoguePanelTxt;

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

    public static List<Task> activeTasks = new();
    private static List<GameObject> activeTasksGameObjs = new();

    private const float resetSpawnNPCTimerMin = 150;
    private const float resetSpawnNPCTimerMax = 300;
    private const float npcTaskTimerMin = 25;
    private const float npcTaskTimerMax = 40;
    private static float spawnNPCTimerSec;
    private static bool npcExists = false;
    private static bool npcSpawnTimerStarted = false;

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
        if (Input.GetKeyUp(KeyCode.Space))
            Dialogue.skip = !Dialogue.skip;
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

        //UpdateShowNpcTimer();
    }

    private void UpdateShowNpcTimer()
    {
        if (!npcExists)
        {
            if (!npcSpawnTimerStarted)
            {
                spawnNPCTimerSec = Random.Range(resetSpawnNPCTimerMin, resetSpawnNPCTimerMax);
                npcSpawnTimerStarted = true;
            }
            else
            {
                if (spawnNPCTimerSec > 0)
                    spawnNPCTimerSec -= Time.deltaTime;
                else
                {
                    //!!!todo spawn npc
                    npcExists = true;
                    npcSpawnTimerStarted = false;
                }
            }
        }
    }

    private void UpdateTasks()
    {
        if (TrashDrag.readyToRemoveTrash)
            CreateNewTask(activeTasksGameObjs.Count, "Take out the trash", TaskType.Trash, TrashManager.taskTimer);
        if (!CleanManager.clean)
            CreateNewTask(activeTasksGameObjs.Count, "Clean Table!", TaskType.Clean, CleanManager.taskTimer);

        var openImg = tasksOpenMenuBtn.GetComponent<RawImage>();
        var closeImg = tasksCloseMenuBtn.GetComponent<RawImage>();
        if (activeTasksGameObjs.Count > 0)
        {
            openImg.color = Color.red;
            closeImg.color = Color.red;
        }
        else
        {
            openImg.color = Color.white;
            closeImg.color = Color.white;
        }
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
            activeTasks.Add(new Task(timerSec, taskType));
        else
            timerSec = activeTasks.First(task => task.type == taskType).timer;

        taskTimer.timeRemaining = timerSec;

        activeTasksGameObjs.Add(taskInstance);
    }

    public static void RemoveTask(TaskType taskType)
    {
        if (!activeTasks.Any(task => task.type == taskType))
            return;

        Task taskInfo = activeTasks.Find(task => task.type == taskType);
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
            Dialogue.LoadMusicTutorial();
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