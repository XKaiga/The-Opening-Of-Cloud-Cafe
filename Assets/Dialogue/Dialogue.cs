using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using Input = UnityEngine.Input;

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
    private List<DialogueOption> dialogueOptions = new();
    private int optionIndex = 0;
    public static bool isChoosing = false;
    public static bool isMusicDoneVar=false;

    public static List<Character> characters = new() {
        new Character(name: "ALYIA", hateMusicTxt: "\"Can you put another music plz, i don't really vibe with this one.\""),
        new Character(name: "RONNIE", hateMusicTxt: "\"I hate this music, can you put another.\""),
        new Character(name : "JASPER", hateMusicTxt: "\"Ca...c..can you change the music plz.\""),
        new Character(name : "AMARA", hateMusicTxt: "\"With a song like that it would be better to have zero music.\"")
    };
    private static List<string> charsPosData = new() { "", "", "" };

    [SerializeField] private float textSpeed;
    [SerializeField] private TextMeshProUGUI namePanelTxt;
    [SerializeField] private TextMeshProUGUI dialoguePanelTxt;
    private string lineName;
    private string lineDialogue;

    private string filePath;
    private static int fileDayNumRead = -1;
    private static string[] lines;
    public static int lineIndex = -1;

    public static bool startingNewDay = false;
    private static int dialoguePauseSecs = 3;

    private const float resetTimerToHateMusic = 10;//!!!100
    private static float timerToHateMusicSec = resetTimerToHateMusic;
    private static bool hatingcurrMusic = false;

    private void Start()
    {
        UpdateCharacters();

        if (fileDayNumRead != GameManager.startDayNum)
        {
            filePath = Application.dataPath + "/Dialogue/DayFiles/day" + GameManager.startDayNum;
            if (GameManager.startDayNum == 3)
            {
                filePath += "RONNIE";//GetMostFavoredCharacter().ToUpper();
            }
            filePath += ".txt";

            lines = System.IO.File.ReadAllLines(filePath);

            fileDayNumRead = GameManager.startDayNum;
        }

        if (startingNewDay)
            return;
        if (lineIndex > 0)
            lineIndex--;

        InicializeButtons();

        NextLine();
    }

    private void InicializeButtons()
    {
        optionButton1.onClick.AddListener(() => ChooseOption(1));
        optionButton2.onClick.AddListener(() => ChooseOption(2));
        optionButton3.onClick.AddListener(() => ChooseOption(3));

        options.SetActive(false);
    }

    public static bool skip = false;
    [SerializeField] private float pauseBetweenSkips = 0.1f;
    void Update()
    {
        if (!startingNewDay)
        {
            if (Input.GetMouseButtonDown(0) || skip)
            {
                StartCoroutine(WaitXSeconds(skip ? pauseBetweenSkips : 0, () =>
                {
                    if (!isChoosing && !DrinkManager.mainClientWaiting && !startingNewDay)
                    {
                        this.GetComponent<RawImage>().enabled = true;

                        if (dialoguePanelTxt.text == lineDialogue)
                        {
                            NextLine();

                            int lineStopIndex = GameManager.startDayNum == 1 ? 173 : GameManager.startDayNum == 2 ? 383 : 182;
                            if (lineIndex == lineStopIndex)
                                skip = false;
                        }
                        else
                        {
                            StopAllCoroutines();
                            namePanelTxt.text = lineName;
                            dialoguePanelTxt.text = lineDialogue;
                        }
                    }
                }));
            }

            UpdateHateTimer();
        }
    }

    private void UpdateHateTimer()
    {
        Debug.LogWarning("timer da musica: " + timerToHateMusicSec);
        if (timerToHateMusicSec > 0)
        {
            timerToHateMusicSec -= Time.deltaTime;
            if (hatingcurrMusic)
                Money.playerScore -= Time.deltaTime;
        }
        else
        {
            List<Character> charsHateCurrMusic = Music.WhoHatesMusic();
            foreach (var character in charsHateCurrMusic)
            {
                if (!lines[lineIndex + 1].Contains(character.hateMusicTxt))
                {
                    if (isMusicDoneVar == false)
                    {
                        isMusicDoneVar = true;
                        LoadMusicTutorial();
                    }
                    InsertAtIndex(character.name + ": " + character.hateMusicTxt, lineIndex + 1);
                }
            }

            if (charsHateCurrMusic.Count > 0)
                hatingcurrMusic = true;

            timerToHateMusicSec = resetTimerToHateMusic;
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

    public static IEnumerator WaitXSeconds(float seconds = -1, Action callback = null)
    {
        yield return new WaitForSeconds(seconds == -1 ? dialoguePauseSecs : seconds);
        callback?.Invoke();
    }

    private IEnumerator TypeLine(TextMeshProUGUI panelTxt, string line)
    {
        panelTxt.gameObject.SetActive(true);

        string[] textFormats = { "</i>" };
        bool hasItalic = line.Contains("</i>");

        for (int i = 0; i < line.Length; i++)
        {
            panelTxt.text += line[i];
            if (hasItalic)
            {
                for (int j = 0; j < textFormats.Length; j++)
                {
                    if (textFormats[j].Contains(line[i]))
                    {
                        while (textFormats[j].Contains(line[i + 1]))
                        {
                            i++;
                            panelTxt.text += line[i];
                            if (line[i] == '>')
                                break;
                        }
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(textSpeed);
        }
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
        if (dialogueOptions.Count != 0 && dialogueOptions[optionIndex].Responses.Count > 0)
        {
            line = dialogueOptions[optionIndex].Responses[0];
            dialogueOptions[optionIndex].Responses.RemoveAt(0);
        }
        else
            lineIndex++;

        dialoguePanelTxt.text = string.Empty;
        namePanelTxt.text = string.Empty;

        IdentifyAndExecuteTypeOfText(line);
    }

    private void IdentifyAndExecuteTypeOfText(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            line = lines[lineIndex];
            line = FormatItalic(line);
        }

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

                StartCoroutine(TypeLine(namePanelTxt, lineName));
                StartCoroutine(TypeLine(dialoguePanelTxt, lineDialogue));
                break;

            case "options":
                this.GetComponent<RawImage>().enabled = false;

                dialogueOptions.Clear();
                isChoosing = true;
                optionIndex = 0;
                ReadOptions();

                break;

            case "drink":
                this.GetComponent<RawImage>().enabled = false;

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
                        Drink mainDrink = DrinkManager.FindOrder(name, drinkNumber, DrinkManager.mainDrinks);

                        if (mainDrink != null && !DrinkManager.mainDrinksToServe.Contains(mainDrink))
                            DrinkManager.mainDrinksToServe.Add(mainDrink);
                        continue;
                    }

                    int indexMaxDrink = DrinkManager.secondariesDrinks.Count - 1;
                    int randomDrinkIndex = Random.Range(0, indexMaxDrink);
                    DrinkManager.secondariesDrinksToServe.Add(DrinkManager.secondariesDrinks[randomDrinkIndex]);

                    string drinkServeAnswer = lines[lineIndex + 1];
                    lineIndex++; //skip answer of receiving drink !!!how secondaries work
                    NextLine();
                }
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
                GameManager.startDayNum++;
                lineIndex = -1;
                startingNewDay = true;

                this.GetComponent<RawImage>().enabled = false;

                StartCoroutine(WaitXSeconds(-1, () =>
                {
                    if (GameManager.startDayNum > 3 || GameManager.startDayNum == 2)
                    {
                        EndGameManager.instance.gameObject.SetActive(true);
                        EndGameManager.instance.StartEndGame();

                        if (GameManager.startDayNum > 3)
                            return;
                    }

                    //começar dia
                    Start();
                    this.GetComponent<RawImage>().enabled = true;

                    startingNewDay = false;
                    NextLine();
                }));

                break;
        }
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

        if (lineLowerCase.StartsWith("1)") || lineLowerCase.StartsWith("2)") || lineLowerCase.StartsWith("3)"))
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

    /*
    private void ReadRegularDialogue()
    {
        while (lineIndex < lines.Length)
        {
            if (lines[lineIndex].Contains("(BACK TO REGULAR DIALOGUE.)"))
            {
                lineIndex++;
                break;
            }
            lineIndex++;
        }

        if (lineIndex < lines.Length)
        {
            IdentifyTypeOfText();
            StartCoroutine(TypeLine(namePanelTxt, lineName));
            StartCoroutine(TypeLine(dialoguePanelTxt, lineDialogue));
        }
    }
    */

    private void ReadOptions()
    {
        int optionsRead = 0;
        while (lineIndex < lines.Length && optionsRead < 6)
        {
            string line = lines[lineIndex];
            if (line.Contains("(BACK TO REGULAR DIALOGUE.)"))
                break;

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
        if (dialogueOptions.Count == 3)
        {
            options.SetActive(true);

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
        options.SetActive(false);
        isChoosing = false;

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

    public static void LoadMusicTutorial()
    {
        List<string> txtMusic = new List<string>();
        txtMusic.Add("\"\": (Like any Café, Cloud Café has background music playing.)");
        txtMusic.Add("\"\": (However, as conversations with customers shift and mood changes," +
            " the music has to be changed as well. No one likes to hear happy music while having" +
            " a sad conversation, or, at the very least, your customers don’t.)");
        txtMusic.Add("\"\": (Go on the music tab to change your café’s tune when the " +
            "customers complain, and find a better fitting choice so you can carry out " +
            "the conversation without losing points. )");
        txtMusic.Add("\"\": (Feel the music and try to figure out if it better fits the mood.)");
        txtMusic.Add("\"\": (Upgrades can be bought at the upgrades store so you’re able to see which emotion songs portray.)");
        foreach (var txt in txtMusic)
        {
            InsertAtIndex(txt, lineIndex + 1);
        }
    }
}

[System.Serializable]
public class DialogueOption
{
    public string Prompt;
    public List<string> Responses = new List<string>();
}


