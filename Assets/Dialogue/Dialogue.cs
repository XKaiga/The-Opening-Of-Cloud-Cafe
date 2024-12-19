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

public class Dialogue : MonoBehaviour
{
    [SerializeField] private List<RawImage> CharacterSpaces;

    [SerializeField] private GameObject options;
    [SerializeField] private Button optionButton1;
    [SerializeField] private Button optionButton2;
    [SerializeField] private Button optionButton3;
    [SerializeField] private TextMeshProUGUI optionText1;
    [SerializeField] private TextMeshProUGUI optionText2;
    [SerializeField] private TextMeshProUGUI optionText3;
    private static List<DialogueOption> dialogueOptions = new();
    private static int optionIndex = 0;
    private static int responseIndex = 0;
    public static bool isChoosing = false;
    public static bool charAnswering = false;
    private static int backToDialogueIndex = -1;

    public static bool isMusicDoneVar = false;
    public static bool isTrashTutDoneVar = false;

    public static List<Character> characters = new() {
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

    private void Start()
    {
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
    public static float pauseBetweenSkips = 0.2f;
    public bool waitingForDialogue = false;
    void Update()
    {
        if (!startingNewDay)
        {
            if (!isChoosing && !charAnswering && !onTutorial)
                UpdateHateTimer();

            if (skip)
                OnClickDialogue();

            if (TrashDrag.readyToRemoveTrash && !isTrashTutDoneVar)
            {
                isTrashTutDoneVar = true;
                LoadTrashTutorial();
            }
            if (!TableManager.isCleanTutDone && TableManager.doCleanTut)
            {
                TableManager.isCleanTutDone = true;
                LoadTrashTutorial();
            }
        }
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

    private void UpdateCharacters()
    {
        if (CharacterSpaces[0] == null)
            return;

        for (int i = 0; i < charsPosData.Count; i++)
            CharacterSpaces[i].GetComponentInChildren<Text>(true).text = charsPosData[i];

        foreach (var character in characters)
        {
            bool toShowCharacter = character.active;

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
                    lineIndex--;

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
                    //    ScndNPCs.secondariesDrinksToServe.Add(secDrink);
                    ScndNPCs.secondariesDrinksToServe.Add(ScndNPCs.GenerateRandomScndDrink());
                    MainCoffeeManager.activeTasks.Add(new(Drink.drinkTaskTimer, TaskType.NPCOrder));
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
                    characterPartEmo.currentEmotion = emotion;
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
                skip = false;
                GameManager.startDayNum++;
                lineIndex = -1;
                startingNewDay = true;

                StartCoroutine(HandleNewDay());

                break;
        }
    }

    private IEnumerator HandleNewDay()
    {
        EndGameManager.instance.image.enabled = true;
        namePanelTxt.text = lineName = "";
        dialoguePanelTxt.text = "";
        lineDialogue = "A few days later....";
        yield return TypeLine(dialoguePanelTxt, lineDialogue, false);

        yield return WaitXSeconds(1.5f, null);
        dialoguePanelTxt.text = lineDialogue = "";
        this.GetComponent<RawImage>().enabled = false;

        if (GameManager.startDayNum > 3 || GameManager.startDayNum == 2)
        {
            EndGameManager.instance.gameObject.SetActive(true);

            yield return EndGameManager.instance.StartEndGameCoroutine();

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
        string pattern = @"^\d*\)?\s*[“\""]?(.*?)[”\""]?\s*\(([^)]*)\)$";

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
        List<string> txtMusic = new List<string>
        {
            "Tutorial : (Upgrades can be bought at the upgrades store so you’re able to see which emotion songs portray.)",
            "Tutorial : (Feel the music and try to figure out if it better fits the mood.)",
            "Tutorial : (Go on the music tab to change your café’s tune when the " +
            "customers complain, and find a better fitting choice so you can carry out " +
            "the conversation without losing points. )",
            "Tutorial : (However, as conversations with customers shift and mood changes," +
            " the music has to be changed as well. No one likes to hear happy music while having" +
            " a sad conversation, or, at the very least, your customers don’t.)",
            "Tutorial : (Like any Café, Cloud Café has background music playing.)"
        };

        foreach (var txt in txtMusic)
        {
            InsertAtIndex(txt, lineIndex + 1);
        }

        if (SceneManager.GetActiveScene().name != "Dialogue")
        {
            pauseBetweenSkips = 0.2f;
            skip = false;
            nameTxt = namePanelTxt.text;
            dialogueTxt = dialoguePanelTxt.text;

            if (SceneManager.GetActiveScene().name == "Tables")
                TableManager.inAnotherView = true;

            SceneManager.LoadScene("Dialogue");
        }
    }

    public void LoadDrinkTutorial()
    {
        List<string> txtDrink = new()
        {
            "Tutorial : (If you make a mistake in the process, just press the red button on the left side of the drink machine. Be careful " +
            "because all the already poured ingredients will be going to the trash!!!)",
            "Tutorial : (Repeat the process to the other categories, and then press the green button on the left side of the drink machine to serve it.)",
            "Tutorial : (Then, when you are ready to pour the ingredient onto the cup press the yellow button on the left side of the drink machine.)",
            "Tutorial : (To make a drink, select the desired category and then choose the ingredient by clicking its respective icon.)",
            "Tutorial : (In the top right corner of the machine, you’re able to select what category of what part of the drink you want to " +
            "add. It should be made in a sweetener-base-topping order.)",
            "Tutorial : (To make a drink, you’ll need to choose one sweetener/syrup, one base and one topping.)"
        };

        foreach (var txt in txtDrink)
        {
            Dialogue.InsertAtIndex(txt, Dialogue.lineIndex + 1);
        }

        if (SceneManager.GetActiveScene().name != "DrinkStation")
        {
            Dialogue.skip = true;
            Dialogue.pauseBetweenSkips = -2f;

            Dialogue.nameTxt = namePanelTxt.text;
            Dialogue.dialogueTxt = dialoguePanelTxt.text;

            if (SceneManager.GetActiveScene().name == "Tables")
                TableManager.inAnotherView = true;

            SceneManager.LoadScene("DrinkStation");
        }
    }

    public void LoadTrashTutorial()
    {
        List<string> txtTrash = new List<string>
        {
            "Tutorial: (It is also possible to increase your trash bin’s capacity on the upgrades’ store.)",
            "Tutorial: (Just drag the trash bag to the correspondent and correct trash can, " +
                "and the trash will be dealt with. Throwing it on the wrong trash can will deduct points " +
                "from your score too.)",
            "Tutorial: (Trash can’t be accumulated at the Café, so to get rid of it is of" +
                " upmost importance, otherwise points will be deducted from the final score and" +
                " character ending.)",
            "Tutorial: (When there’s trash to be taken out, hurry up! )"
        };

        foreach (var txt in txtTrash)
        {
            InsertAtIndex(txt, lineIndex + 1);
        }

        if (SceneManager.GetActiveScene().name != "Tables")
        {
            Dialogue.skip = true;
            Dialogue.pauseBetweenSkips = -2f;

            Dialogue.nameTxt = namePanelTxt.text;
            Dialogue.dialogueTxt = dialoguePanelTxt.text;

            SceneManager.LoadScene("Tables");
        }
    }
    public void LoadTableTutorial()
    {
        List<string> txtTable = new List<string>
        {
            "Tutorial: (Bigger cloth sizes are available in the upgrades store.)",
            "Tutorial: (Just click and drag the mouse on top of the stains on top of the table to clean them.",
            "Tutorial: (You have limited time to do it, and not being able to do it in time will result" +
            " in losing points which will affect the final score and character’s ending!",
            "Tutorial: (As the task of cleaning a table appears on the notification’s tab, hurry to the table" +
            " section to clean the table that’s dirty.)"
        };

        foreach (var txt in txtTable)
        {
            InsertAtIndex(txt, lineIndex + 1);
        }

        if (SceneManager.GetActiveScene().name != "Tables")
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
public class DialogueOption
{
    public string Prompt;
    public List<string> Responses = new List<string>();
}


