using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using static Effects;

public class Dialogue : MonoBehaviour
{
    private EndGameManager endGameManager;
    private RawImage EndGameImage;

    [SerializeField] private List<RawImage> CharacterSpaces;

    [SerializeField] private GameObject options;
    [SerializeField] private Button optionButton1;
    [SerializeField] private Button optionButton2;
    [SerializeField] private Button optionButton3;
    [SerializeField] private TextMeshProUGUI optionText1;
    [SerializeField] private TextMeshProUGUI optionText2;
    [SerializeField] private TextMeshProUGUI optionText3;
    [SerializeField] private List<GameObject> effectsPrefabs;
    private static List<DialogueOption> dialogueOptions = new();
    private static int optionIndex = 0;
    private static int responseIndex = 0;
    public static bool isChoosing = false;
    public static bool charAnswering = false;
    private static int backToDialogueIndex = -1;


    public static bool isMusicDoneVar = false;
    public static bool isTrashTutDoneVar = false;

    public static List<Character> characters = new() {
        new Character(name: "ETHAN"),
        new Character(name: "ALYIA", patience: 2, hateMusicTxt: "\"Can you put another music plz, i don't really vibe with this one.\""),
        new Character(name: "RONNIE", patience: 4,  hateMusicTxt: "\"I hate this music, can you put another.\""),
        new Character(name : "JASPER", patience: 3,  hateMusicTxt: "\"Ca...c..can you change the music plz.\""),
        new Character(name : "AMARA", patience : 5, hateMusicTxt: "\"With a song like that it would be better to have zero music.\"")
    };
    private static List<string> charsPosData = new() { "", "", "" };

    [SerializeField] private float textSpeed;
    [SerializeField] private TextMeshProUGUI namePanelTxt;
    public static string lineName;
    public static string nameTxt = string.Empty;
    public static string nameTxtTemp = string.Empty;

    [SerializeField] private TextMeshProUGUI dialoguePanelTxt;
    public static string lineDialogue;
    public static string dialogueTxt = string.Empty;
    public static string dialogueTxtTemp = string.Empty;

    private string filePath;
    private static int fileDayNumRead = -1;
    public static string[] lines;
    public static int lineIndex = -1;

    public static bool startingNewDay = false;
    private static int dialoguePauseSecs = 3;

    private const float resetTimerToHateMusic = 100;
    private static float timerToHateMusicSec = resetTimerToHateMusic;
    public static bool onTutorial = false;

    private void Awake()
    {
        EndGameManager endManager = FindObjectOfType<EndGameManager>();
        if (endGameManager != null)
        {
            endGameManager = endManager;
            EndGameImage = endManager.image;
            Debug.Log("Success");
        }
        else
            Debug.Log("Failed");
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Dialogue")
        {
            skip = false;
            pauseBetweenSkips = 0.2f;
        }

        UpdateCharacters();

        if (fileDayNumRead != GameManager.startDayNum)
        {
            filePath = Application.dataPath + "/Dialogue/DayFiles/day" + GameManager.startDayNum;
            if (GameManager.startDayNum == 3)
            {
                string charName = GetMostFavoredCharacter() ?? "RONNIE";
                filePath += charName.ToUpper();
            }
            filePath += ".txt";

            lines = System.IO.File.ReadAllLines(filePath);

            if (GameManager.startDayNum > 0)
                MainCoffeeManager.LoadSecndNpcsDrinks();

            fileDayNumRead = GameManager.startDayNum;
        }

        if (startingNewDay)
            return;

        InicializeButtons();

        namePanelTxt.text = nameTxt;
        dialoguePanelTxt.text = dialogueTxt;

        nameTxt = nameTxtTemp;
        dialogueTxt = dialogueTxtTemp;

        nameTxtTemp = string.Empty;
        dialogueTxtTemp = string.Empty;

        if (lineIndex > -1)
            lineIndex--;
        NextLine();
    }

    private void InicializeButtons()
    {
        optionButton1.onClick.AddListener(() => ChooseOption(1));
        optionButton2.onClick.AddListener(() => ChooseOption(2));
        optionButton3.onClick.AddListener(() => ChooseOption(3));

        options.SetActive(isChoosing);

        if (isChoosing)
        {
            this.GetComponent<RawImage>().enabled = false;
            namePanelTxt.text = "";
            dialoguePanelTxt.text = "";
            ShowOptions();
        }
    }

    public static bool skip = false;
    public const float defaultTimeBetweenSkips = 0.2f;
    public static float pauseBetweenSkips = defaultTimeBetweenSkips;
    public bool waitingForDialogue = false;
    void Update()
    {
        if (!startingNewDay)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != "Dialogue" && sceneName != "DrinkStation")
            {
                skip = !onTutorial;
                pauseBetweenSkips = onTutorial ? 0.2f : -2f;
            }

            if (!isChoosing && !charAnswering && !onTutorial)
                UpdateHateTimer();

            if (skip || Input.GetKeyUp(KeyCode.Space))
            {
                EventSystem.current.SetSelectedGameObject(null); // Clear focus on UI elements
                OnClickDialogue();
            }

            ManagePlayerInputs();

            if (TrashDrag.readyToRemoveTrash && !isTrashTutDoneVar)
            {
                isTrashTutDoneVar = true;
                LoadTrashTutorial();
            }
            if (!TableManager.isCleanTutDone && TableManager.doCleanTut)
            {
                TableManager.isCleanTutDone = true;
                LoadTableTutorial();
            }
        }
    }

    private void HandleDialogueSceneInput()
    {
        MainCoffeeManager mainCoffeeManager = FindObjectOfType<MainCoffeeManager>();
        if (mainCoffeeManager == null)
            return;

        if (Input.GetKeyUp(KeyCode.D))
        {
            mainCoffeeManager.LoadDrinkStationScene();
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            mainCoffeeManager.LoadTablesScene();
        }
        else if (Input.GetKeyUp(KeyCode.M) || Input.GetKeyUp(KeyCode.S))
        {
            mainCoffeeManager.ToggleMusicMenu();
        }
        else if (Input.GetKeyUp(KeyCode.U))
        {
            mainCoffeeManager.ToggleUpgradeMenu();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            mainCoffeeManager.ToggleTaskMenu();
        }
    }
    private void HandleDrinkStationInput()
    {
        DrinkManager drinkManager = FindObjectOfType<DrinkManager>();
        if (drinkManager == null)
            return;

        if (Input.GetKeyUp(KeyCode.B))
        {
            if (!isChoosing)
            {
                drinkManager.StartChangingScene();
                LoadDialogueScene();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyUp(KeyCode.I))
        {
            drinkManager.ToggleInfoPanel();
        }
    }
    private void ManagePlayerInputs()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        switch (activeSceneName)
        {
            case "Dialogue":
                HandleDialogueSceneInput();
                break;

            case "DrinkStation":
                HandleDrinkStationInput();
                break;
        }

    }

    public void LoadDialogueScene()
    {
        pauseBetweenSkips = defaultTimeBetweenSkips;
        skip = false;

        nameTxtTemp = nameTxt;
        dialogueTxtTemp = dialogueTxt;

        nameTxt = namePanelTxt.text;
        dialogueTxt = dialoguePanelTxt.text;

        SceneManager.LoadScene("Dialogue");
    }

    private void UpdateHateTimer()
    {
        if (timerToHateMusicSec > 0)
            timerToHateMusicSec -= Time.deltaTime;
        else
        {
            List<Character> charsHateCurrMusic = Music.WhoHatesMusic();
            if (charsHateCurrMusic.Count != 0)
            {
                bool hateDone = false;
                while (!hateDone)
                {
                    Character rndChar = charsHateCurrMusic[Random.Range(0, charsHateCurrMusic.Count())];

                    if (!lines[lineIndex + 1].Contains(rndChar.hateMusicTxt))
                    {
                        if (isMusicDoneVar == false)
                        {
                            isMusicDoneVar = true;
                            LoadMusicTutorial();
                        }

                        InsertAtIndex(rndChar.name + ": " + rndChar.hateMusicTxt, lineIndex + 1);
                        rndChar.Patience--;
                        Money.playerScore -= (5 - rndChar.Patience) * 5 + 5;
                        hateDone = true;
                    }
                    else
                        break;
                }
            }
            timerToHateMusicSec = resetTimerToHateMusic;
        }
    }

    public void OnClickDialogue()
    {
        if (!startingNewDay && !waitingForDialogue)
        {
            waitingForDialogue = true;
            StartCoroutine(WaitXSeconds(skip ? pauseBetweenSkips : 0, () =>
            {
                waitingForDialogue = false;

                if (!isChoosing && !DrinkManager.mainClientWaiting && !startingNewDay)
                {
                    this.GetComponent<RawImage>().enabled = true;

                    if (dialoguePanelTxt.text == lineDialogue || lineDialogue == null)
                    {
                        NextLine();
                    }
                    else
                    {
                        StopAllCoroutines();
                        namePanelTxt.text = lineName;
                        nameTxt = string.Empty;
                        dialoguePanelTxt.text = lineDialogue;
                        dialogueTxt = string.Empty;
                    }
                }
            }));
        }
    }

    public static void InsertAtIndex(string newElement, int index)
    {
        List<string> linesList = new List<string>(lines);

        linesList.Insert(index, newElement);

        lines = linesList.ToArray();
    }

    public void UpdateCharacterNewSprite(Character character)
    {
        if (CharacterSpaces[0] == null || character == null)
            return;

        LoadCharactersSpaces();

        if (character.active)
        {
            RawImage currCharacterSpace = GetCharacterSpace(character);
            if (currCharacterSpace != null)
            {
                currCharacterSpace.texture = character.sprite;
                currCharacterSpace.gameObject.SetActive(true);
            }
        }
    }

    private RawImage GetCharacterSpace(Character character)
    {
        RawImage currCharacterSpace = null;

        foreach (var space in CharacterSpaces)
        {
            Text textData = space.GetComponentInChildren<Text>(true);
            if (textData != null && textData.text == character.name)
            {
                currCharacterSpace = space;
                break;
            }
        }

        return currCharacterSpace;
    }
    private void LoadCharactersSpaces()
    {
        for (int i = 0; i < charsPosData.Count; i++)
            CharacterSpaces[i].GetComponentInChildren<Text>(true).text = charsPosData[i];
    }

    private void UpdateCharacters()
    {
        if (CharacterSpaces[0] == null)
            return;

        LoadCharactersSpaces();

        foreach (var character in characters)
        {
            bool toShowCharacter = character.active;

            RawImage currCharacterSpace = GetCharacterSpace(character);

            if (toShowCharacter && currCharacterSpace != null && currCharacterSpace.texture == null)
            {
                currCharacterSpace.texture = character.sprite;
                currCharacterSpace.gameObject.SetActive(true);
            }

            else if (currCharacterSpace == null && toShowCharacter)
            {
                RawImage spaceEmpty = CharacterSpaces.FirstOrDefault(space => space.texture == null);
                if (spaceEmpty != null)
                {
                    spaceEmpty.texture = character.sprite;
                    Text textData = spaceEmpty.GetComponentInChildren<Text>(true);
                    textData.text = character.name;
                    if (!charsPosData.Contains(character.name) && charsPosData.Contains(""))
                        charsPosData[charsPosData.IndexOf("")] = character.name;
                    //charsPosData.Add(character.name);
                    spaceEmpty.gameObject.SetActive(true);
                }
            }
            else if (currCharacterSpace != null && !toShowCharacter)
            {
                currCharacterSpace.texture = null;
                Text textData = currCharacterSpace.GetComponentInChildren<Text>(true);
                textData.text = "";
                if (charsPosData.Contains(character.name))
                    charsPosData[charsPosData.IndexOf(character.name)] = ""; //charsPosData.Remove(character.name);
                currCharacterSpace.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator WaitXSeconds(float seconds = -1, Action callback = null)
    {
        if (seconds == -1)
            seconds = dialoguePauseSecs;
        else if (seconds == -2)
            seconds = EstimateSpeakingTimeSecs(lines[lineIndex]);

        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    public float EstimateSpeakingTimeSecs(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        string currTxt = namePanelTxt.text + ": " + dialoguePanelTxt.text;
        int charCount = input.Length - currTxt.Length;
        if (charCount <= 0)
            return 0;

        float charsPerSecond = 17.0f;
        return charCount / charsPerSecond;
    }

    private IEnumerator TypeLine(TextMeshProUGUI panelTxt, string line, bool isNamePanel)
    {
        panelTxt.gameObject.SetActive(true);

        string[] textFormats = { "</i>" };
        bool hasItalic = line.Contains("</i>");

        string currentTxt;
        int startIndex = line.Contains(panelTxt.text) ? panelTxt.text.Length : 0;
        for (int i = startIndex; i < line.Length; i++)
        {
            currentTxt = isNamePanel ? (nameTxt = line.Substring(0, i + 1)) : (dialogueTxt = line.Substring(0, i + 1));
            panelTxt.text = currentTxt;

            if (hasItalic)
            {
                for (int j = 0; j < textFormats.Length; j++)
                {
                    if (textFormats[j].Contains(line[i]))
                    {
                        while (textFormats[j].Contains(line[i + 1]))
                        {
                            i++;
                            currentTxt = isNamePanel ? (nameTxt = line.Substring(0, i + 1)) : (dialogueTxt = line.Substring(0, i + 1));
                            panelTxt.text = currentTxt;
                            if (line[i] == '>')
                                break;
                        }
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(textSpeed);
        }

        if (isNamePanel)
            nameTxt = string.Empty;
        else
            dialogueTxt = string.Empty;
    }

    private void NextLine()
    {
        if (isChoosing)
        {
            ShowOptions();
            return;
        }

        //acabou o texto: caixa desaparece
        if (lineIndex >= lines.Length - 1)
        {
            gameObject.SetActive(false);
            return;
        }

        string line = "";
        if (charAnswering && responseIndex < dialogueOptions[optionIndex].Responses.Count)
        {
            string lineShowing = namePanelTxt.text.ToLower() + ": " + dialoguePanelTxt.text.ToLower();
            if (responseIndex == 0 || lineShowing == dialogueOptions[optionIndex].Responses[responseIndex - 1].ToLower())
            {
                line = dialogueOptions[optionIndex].Responses[responseIndex];
                responseIndex++;

                if (responseIndex == dialogueOptions[optionIndex].Responses.Count)
                {
                    lineIndex = backToDialogueIndex;
                    backToDialogueIndex = -1;
                    responseIndex = 0;
                    dialogueOptions.Clear();
                    charAnswering = false;
                }
            }
            else
                line = dialogueOptions[optionIndex].Responses[responseIndex - 1];
        }
        else
            lineIndex++;

        if (dialoguePanelTxt.text != lineDialogue)
        {
            namePanelTxt.text = nameTxt;
            dialoguePanelTxt.text = dialogueTxt;
        }

        IdentifyAndExecuteTypeOfText(line);
    }

    private void IdentifyAndExecuteTypeOfText(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            line = lines[lineIndex];
            line = FormatItalic(line);
        }

        onTutorial = line.Contains("Tutorial");

        string typeOfText = GetTypeOfText(line);
        switch (typeOfText)
        {
            case "conversation":
                string patternName = @"^(.*?): ";
                string patternDialogue = @": (.*)";

                Match matchName = Regex.Match(line, patternName);
                Match matchDialogue = Regex.Match(line, patternDialogue);

                if (matchName.Success)
                    lineName = matchName.Groups[1].Value;

                if (matchDialogue.Success)
                    lineDialogue = matchDialogue.Groups[1].Value;

                StartCoroutine(TypeLine(namePanelTxt, lineName ?? "", true));
                StartCoroutine(TypeLine(dialoguePanelTxt, lineDialogue ?? "", false));
                break;

            case "options":
                this.GetComponent<RawImage>().enabled = false;
                namePanelTxt.text = "";
                dialoguePanelTxt.text = "";

                dialogueOptions.Clear();
                isChoosing = true;
                optionIndex = 0;
                ReadOptions();

                break;

            case "drink":
                if (DrinkManager.isDrinkTutorialDone == false)
                {
                    DrinkManager.isDrinkTutorialDone = true;

                    LoadDrinkTutorial();
                    return;
                }

                List<String> drinkTexts = new() { line };
                int indexDrinks = 1;
                while (lineIndex + indexDrinks < lines.Length && GetTypeOfText(lines[lineIndex + indexDrinks]) == "drink")
                {
                    drinkTexts.Add(lines[lineIndex + indexDrinks]);
                    indexDrinks++;
                }
                lineIndex += indexDrinks - 1;

                foreach (string drinkText in drinkTexts)
                {
                    bool isMainDrink = ExtractClientNameAndDrinkNumber(drinkText, out string name, out int drinkNumber);
                    if (isMainDrink)
                    {
                        this.GetComponent<RawImage>().enabled = false;
                        namePanelTxt.text = "";
                        dialoguePanelTxt.text = "";

                        Drink mainDrink = DrinkManager.FindOrder(name, drinkNumber, DrinkManager.mainDrinks);

                        if (mainDrink != null && !DrinkManager.mainDrinksToServe.Contains(mainDrink))
                            DrinkManager.mainDrinksToServe.Add(mainDrink);
                        continue;
                    }

                    //int randomDrinkIndex = Random.Range(0, ScndNPCs.secondariesDrinks.Count);
                    //Drink secDrink = ScndNPCs.secondariesDrinks[randomDrinkIndex];
                    //if (!ScndNPCs.secondariesDrinksToServe.Contains(secDrink))
                    //    ScndNPCs.secondariesDrinksToServe.Add(secDrink)
                    if (!MainCoffeeManager.activeTasks.Any(task => task.type == TaskType.NPCOrder)) //!!! temp until having more than 1 npc works
                    {
                        ScndNPCs.secondariesDrinksToServe.Add(ScndNPCs.GenerateRandomScndDrink());
                        MainCoffeeManager.activeTasks.Add(new(Drink.drinkTaskTimer, TaskType.NPCOrder));
                    }
                }

                MainCoffeeManager dialogueManager = FindObjectOfType<MainCoffeeManager>();
                if (dialogueManager != null)
                    dialogueManager.UpdateTasks();

                if (charAnswering)
                    responseIndex += drinkTexts.Count();

                if (!DrinkManager.mainClientWaiting)
                    NextLine();

                break;

            case "emo": //(EMO_Name_Emotion)
                string[] emoParts = TrimSplitDialogueCode(line);

                var musicEmoDetermined = Music.DetermineEmotion(emoParts[2]);
                if (musicEmoDetermined == null)
                    break;

                if (musicEmoDetermined is EmotionEnum emotion)
                {
                    Character characterPartEmo = Character.DetermineCharacter(emoParts[1]);

                    EmotionEnum previousEmotion = characterPartEmo.currentEmotion;

                    characterPartEmo.currentEmotion = emotion;

                    if (SceneManager.GetActiveScene().name == "Dialogue")
                    {
                        string emoString = GameManager.GetEmoStringForSprite(characterPartEmo, previousEmotion);

                        GameManager.UpdateCharacterSprite(characterPartEmo, emoString);
                    }
                }
                NextLine();
                break;

            case "show": //(Hide_Name)
            case "hide": //(Show_Name)
                string[] ShowHideParts = TrimSplitDialogueCode(line);

                Character character = Character.DetermineCharacter(ShowHideParts[1]);
                if (character == null)
                    break;

                character.active = ShowHideParts[0].ToLower().Contains("show");
                if (character.active)
                {
                    //play andar sound
                    AudioClip soundEffect = Resources.Load<AudioClip>("SoundEffects/" + "sfx_andar");
                    AudioSource.PlayClipAtPoint(soundEffect, Vector3.zero, Music.vfxVolume);
                }

                UpdateCharacters();

                NextLine();
                break;

            case "endingDay":
                startingNewDay = true;

                if (SceneManager.GetActiveScene().name != "Dialogue")
                    SceneManager.LoadScene("Dialogue");

                EndGameManager.DeactivateAllButtons();

                EndGameImage.enabled = true;

                skip = false;
                GameManager.startDayNum++;
                lineIndex = -1;

                StartCoroutine(HandleNewDay());

                break;


            case "effect":

                Debug.Log("reconheceu a palavra effect");

                if (SceneManager.GetActiveScene().name != "Dialogue")
                    break;

                Debug.Log("reconheceu que a cena e dialogue");


                string[] effectParts = TrimSplitDialogueCode(line);

                Debug.Log("dividiu o a linha em partes");

                var effectDetermined = Effects.DetermineEffect(effectParts[2]);

                Debug.Log("determinou o efeito");

                if (effectDetermined == null)
                {

                    Debug.Log($"o effeito {effectParts[2]} deu null");
                    break;
                }
                Debug.Log("o efeito nao e nulo");

                //if (effectDetermined is Effects effect)
                //{
                Debug.Log("entrou no if");

                Character characterPartEffect = Character.DetermineCharacter(effectParts[1]);
                if (characterPartEffect == null)
                    return;

                Debug.Log("determinou a personagem objetivo");


                foreach (RawImage CharacterSpace in CharacterSpaces)
                {

                    Text nameCharacter = CharacterSpace.GetComponentInChildren<Text>();

                    Debug.Log("obteve o nome da personagem");

                    Vector3 position = CharacterSpace.gameObject.transform.position;
                    string charSpaceGOName = CharacterSpace.gameObject.name.ToLower();
                    bool left = charSpaceGOName.Contains("left");
                    bool right = charSpaceGOName.Contains("right");

                    if (left)
                        position.x = -6;
                    position.y = 1.8f;

                    if (characterPartEffect.name == nameCharacter.text)
                    {

                        Debug.Log("viu a personagem presente");

                        foreach (GameObject effectPrebab in effectsPrefabs)
                        {

                            if (effectDetermined is Effects.EffectType tipo)
                            {

                                Debug.Log("reconheceu a o tipo de efeito");

                                if (effectPrebab.GetComponent<Effects>().type == tipo)
                                {

                                    Debug.Log("instanciou");
                                    Instantiate(effectPrebab, position, Quaternion.identity);

                                    EmotionEnum previousEmotion = characterPartEffect.currentEmotion;

                                    characterPartEffect.currentEmotion = GameManager.GetEmotionForEffect(tipo);

                                    string emoString = "";
                                    bool charSuprised = tipo == EffectType.Shocked;
                                    if (charSuprised)
                                        emoString = "_Suprised";
                                    else
                                        emoString = GameManager.GetEmoStringForSprite(characterPartEffect, previousEmotion);

                                    GameManager.UpdateCharacterSprite(characterPartEffect, emoString);
                                    break;
                                }
                            }

                        }
                        break;
                    }
                }

                //}

                NextLine();

                break;
        }
    }

    private IEnumerator HandleNewDay()
    {
        namePanelTxt.text = lineName = "";
        dialoguePanelTxt.text = "";
        lineDialogue = "A few days later....";
        yield return TypeLine(dialoguePanelTxt, lineDialogue, false);

        yield return WaitXSeconds(1.5f, null);
        dialoguePanelTxt.text = lineDialogue = "";
        this.GetComponent<RawImage>().enabled = false;

        if (GameManager.startDayNum > 3 || GameManager.startDayNum == 2)
        {
            endGameManager.gameObject.SetActive(true);

            yield return endGameManager.StartEndGameCoroutine();

            if (GameManager.startDayNum > 3)
                yield break; // Exit the coroutine if day number is greater than 3
        }

        yield return WaitXSeconds(1.5f, null);

        // Start new day
        Start();
        this.GetComponent<RawImage>().enabled = true;

        startingNewDay = false;
        OnClickDialogue();
    }

    public static string[] TrimSplitDialogueCode(string line)
    {
        line = line.Trim('(').Trim(')');
        string[] parts = line.Split('_');
        return parts;
    }

    private string GetTypeOfText(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            lines = System.IO.File.ReadAllLines(filePath);
            line = lines[lineIndex];
        }

        string lineLowerCase = line.ToLower();

        if (lineLowerCase.Contains("(drink"))
            return "drink";

        if (lineLowerCase.StartsWith("1)") || lineLowerCase.StartsWith("2)") || lineLowerCase.StartsWith("3)") || lineLowerCase.StartsWith("(back"))
            return "options";

        if (lineLowerCase.StartsWith("(emo_"))
            return "emo";

        if (lineLowerCase.StartsWith("(show_"))
            return "show";
        if (lineLowerCase.StartsWith("(hide_"))
            return "hide";

        if (lineLowerCase.Contains("(endday_"))
            return "endingDay";
        if (lineLowerCase.Contains("(effect_"))
            return "effect";

        return "conversation";
    }

    private void ReadOptions()
    {
        int optionsRead = 0;
        while (lineIndex < lines.Length)
        {
            string line = lines[lineIndex];
            if (line.Contains("(BACK TO REGULAR DIALOGUE.)"))
            {
                backToDialogueIndex = ++lineIndex;
                break;
            }

            if (line.StartsWith("1)") || line.StartsWith("2)") || line.StartsWith("3)"))
            {
                string valueAndOption = SeparateOption(line);

                var option = new DialogueOption { Prompt = valueAndOption };
                lineIndex++;

                int promptIndex = -1;

                while (lineIndex < lines.Length && !lines[lineIndex].StartsWith("1)") && !lines[lineIndex].StartsWith("2)") && !lines[lineIndex].StartsWith("3)") && !lines[lineIndex].StartsWith("(BACK"))
                {
                    int number = 1;
                    while (number <= 3)
                    {
                        if (lines[lineIndex - 1].StartsWith(number.ToString()) || promptIndex != -1)
                        {
                            if (promptIndex == -1)
                                promptIndex = number - 1;
                            dialogueOptions[promptIndex].Responses.Add(lines[lineIndex]);
                            break;
                        }
                        number++;
                    }
                    lineIndex++;
                }
                if (valueAndOption != "")
                    dialogueOptions.Add(option);
            }
            else
                lineIndex++;

            optionsRead++;
        }
        ShowOptions();
    }

    private string SeparateOption(string optionLine)
    {
        string pattern = @"^\d*\)?\s*[�\""]?(.*?)[�\""]?\s*\(([^)]*)\)$";

        Match match = Regex.Match(optionLine, pattern);
        if (match.Success)
        {
            string option = match.Groups[1].Value;
            string value = match.Groups[2].Value;
            return value + "_" + option;
        }
        return string.Empty;
    }

    private void ShowOptions()
    {
        options.SetActive(true);

        if (dialogueOptions.Count == 3)
        {
            string[][] parts = new string[3][];
            for (int i = 0; i < 3; i++)
                parts[i] = dialogueOptions[i].Prompt.Split('_');

            optionText1.text = parts[0][1];
            optionText2.text = parts[1][1];
            optionText3.text = parts[2][1];
        }
    }

    private void ChooseOption(int optionNumber)
    {
        optionIndex = optionNumber - 1;

        ExecuteOption();
    }

    private void ExecuteOption()
    {
        waitingForDialogue = false;

        charAnswering = true;
        isChoosing = false;

        options.SetActive(false);

        char value = dialogueOptions[optionIndex].Prompt[0];
        if (value == '+')
            Money.playerScore += 100;
        else if (value == '*')
            Money.playerScore += 50;
        else if (value == '-')
            Money.playerScore += 0;
        else
        {
            string[] parts = dialogueOptions[optionIndex].Prompt.Split('_');
            string clientName = parts[0];

            var character = characters.FirstOrDefault(c => c.name == clientName);
            if (character != null)
                character.favoredLevel++;
        }

        this.GetComponent<RawImage>().enabled = true;

        StopAllCoroutines();
        NextLine();
    }

    public static string GetMostFavoredCharacter()
    {
        if (characters == null || characters.Count == 0)
            return null;

        var mostFavoredCharacter = characters.Aggregate((x, y) => x.favoredLevel > y.favoredLevel ? x : y);
        return mostFavoredCharacter.name;
    }

    public static bool ExtractClientNameAndDrinkNumber(string input, out string name, out int number)
    {
        name = null;
        number = 0;

        input = input.Trim('(').Trim(')');
        string[] parts = input.Split('_');

        if (parts.Length == 3 && parts[0].ToLower() == "drink")
        {
            name = parts[1];
            if (int.TryParse(parts[2], out number))
                return true;
        }
        return false;
    }

    private string FormatItalic(string line)
    {
        string pattern = @"(?<!\()\*(.*?)\*(?!\))";
        return Regex.Replace(line, pattern, "<i>$1</i>");
    }

    public void LoadMusicTutorial()
    {
        List<string> txtMusic = new()
        {
            "Tutorial : (Upgrades can be bought at the upgrades store so you�re able to see which emotion songs portray.)",
            "Tutorial : (Feel the music and try to figure out if it better fits the mood.)",
            "Tutorial : (Go on the music tab to change your caf�s tune when the " +
            "customers complain, and find a better fitting choice so you can carry out " +
            "the conversation without losing points. )",
            "Tutorial : (However, as conversations with customers shift and mood changes," +
            " the music has to be changed as well. No one likes to hear happy music while having" +
            " a sad conversation, or, at the very least, your customers don�t.)",
            "Tutorial : (Like any Caf�, Cloud Caf� has background music playing.)"
        };

        foreach (var txt in txtMusic)
        {
            InsertAtIndex(txt, lineIndex + 1);
        }

        if (SceneManager.GetActiveScene().name != "Dialogue")
        {
            pauseBetweenSkips = defaultTimeBetweenSkips;
            skip = false;
            nameTxt = namePanelTxt.text;
            dialogueTxt = dialoguePanelTxt.text;

            if (SceneManager.GetActiveScene().name == "TrashScene")
                TableManager.inAnotherView = true;

            onTutorial = true;
            SceneManager.LoadScene("Dialogue");
        }
    }

    public void LoadDrinkTutorial()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "DrinkStation")
        {
            if (sceneName == "Tables" || sceneName == "TrashScene")
            {
                TableManager tableManager = FindObjectOfType<TableManager>();
                if (tableManager != null)
                {
                    if (sceneName != "TrashScene")
                        tableManager.UpdateCleanTimerOnExit();
                    else
                        TableManager.inAnotherView = true;

                    tableManager.UpdateTrashTimerOnExit();
                }
            }

            pauseBetweenSkips = -2f;
            skip = true;

            Dialogue.nameTxt = namePanelTxt.text;
            Dialogue.dialogueTxt = dialoguePanelTxt.text;

            onTutorial = true;
            SceneManager.LoadScene("DrinkStation");
        }
    }

    public void LoadTrashTutorial()
    {
        List<string> txtTrash = new()
        {
            "Tutorial: (It is also possible to increase your trash bin's capacity on the upgrades' store.)",
            "Tutorial: (Just drag the trash bag to the correspondent and correct trash can, " +
                "and the trash will be dealt with. Throwing it on the wrong trash can will deduct points " +
                "from your score too.)",
            "Tutorial: (Trash can't be accumulated at the Café, so to get rid of it is of" +
                " upmost importance, otherwise points will be deducted from the final score and" +
                " character ending.)",
            "Tutorial: (When there's trash to be taken out, hurry up! )"
        };

        lineIndex--;
        foreach (var txt in txtTrash)
        {
            InsertAtIndex(txt, lineIndex + 1);
        }
        lineIndex++;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "TrashScene")
        {
            TableManager.inAnotherView = true;
            if (sceneName == "DrinkStation")
            {
                DrinkManager drinkManager = FindObjectOfType<DrinkManager>();
                if (drinkManager != null)
                    drinkManager.UpdateDrinkTimerOnExit();
            }
            else if (sceneName == "Tables")
            {
                TableManager tableManager = FindObjectOfType<TableManager>();
                if (tableManager != null)
                    tableManager.UpdateTrashTimerOnExit();
            }

            pauseBetweenSkips = 0.2f;
            skip = false;

            Dialogue.nameTxt = namePanelTxt.text;
            Dialogue.dialogueTxt = dialoguePanelTxt.text;

            onTutorial = true;
            SceneManager.LoadScene("TrashScene");
        }
    }
    public void LoadTableTutorial()
    {
        List<string> txtTable = new()
        {
            "Tutorial: (Bigger cloth sizes are available in the upgrades store.)",
            "Tutorial: (Just click and drag the mouse on top of the stains on top of the table to clean them.)",
            "Tutorial: (You have limited time to do it, and not being able to do it in time will result" +
            " in losing points which will affect the final score and character’s ending!)",
            "Tutorial: (As the task of cleaning a table appears on the notification’s tab, hurry to the table" +
            " section to clean the table that’s dirty.)"
        };

        lineIndex--;
        foreach (var txt in txtTable)
        {
            InsertAtIndex(txt, lineIndex + 1);
        }
        lineIndex++;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Tables")
        {
            if (sceneName == "DrinkStation")
            {
                DrinkManager drinkManager = FindObjectOfType<DrinkManager>();
                if (drinkManager != null)
                    drinkManager.UpdateDrinkTimerOnExit();
            }
            else if (sceneName == "TrashScene")
            {
                TableManager.inAnotherView = true;
                TableManager tableManager = FindObjectOfType<TableManager>();
                if (tableManager != null)
                    tableManager.UpdateTrashTimerOnExit();
            }

            pauseBetweenSkips = 0.2f;
            skip = false;

            Dialogue.nameTxt = namePanelTxt.text;
            Dialogue.dialogueTxt = dialoguePanelTxt.text;

            onTutorial = true;
            SceneManager.LoadScene("Tables");
        }
    }
}

[System.Serializable]
public class DialogueOption
{
    public string Prompt;
    public List<string> Responses = new List<string>();
}


