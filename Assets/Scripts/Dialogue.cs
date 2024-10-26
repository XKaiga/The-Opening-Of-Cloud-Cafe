using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private RawImage bgImg;
    [SerializeField] private Texture AlyaEnteringImg;

    [SerializeField] private GameObject options;
    [SerializeField] private Button optionButton1;
    [SerializeField] private Button optionButton2;
    [SerializeField] private Button optionButton3;
    [SerializeField] private TextMeshProUGUI optionText1;
    [SerializeField] private TextMeshProUGUI optionText2;
    [SerializeField] private TextMeshProUGUI optionText3;
    private List<DialogueOption> dialogueOptions = new List<DialogueOption>();
    private int optionIndex = 0;
    private bool isChoosing = false;

    [SerializeField] private float textSpeed;
    [SerializeField] private TextMeshProUGUI namePanelTxt;
    [SerializeField] private TextMeshProUGUI dialoguePanelTxt;
    private string lineName;
    private string lineDialogue;

    private string filePath;
    private string[] lines;
    public static int lineIndex = 0;
    private List<int> indexClientsOrders = new List<int>();

    private void Start()
    {
        CleanManager.clean = false;

        filePath = Application.dataPath + "/Dialogue/introducao.txt";
        lines = File.ReadAllLines(filePath);

        dialoguePanelTxt.text = string.Empty;
        namePanelTxt.text = string.Empty;
        StartDialogue();

        optionButton1.onClick.AddListener(() => ChooseOption(1));
        optionButton2.onClick.AddListener(() => ChooseOption(2));
        optionButton3.onClick.AddListener(() => ChooseOption(3));

        options.SetActive(false);

        indexClientsOrders.Add(22);
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (int indexClientOrder in indexClientsOrders)
                if (lineIndex == indexClientOrder)
                {
                    DrinkManager.clientWaiting = true;
                    break;
                }

            if (!isChoosing && !DrinkManager.clientWaiting)
                if (dialoguePanelTxt.text == lineDialogue)
                {
                    NextLine();
                    if (lineIndex == 1)
                        bgImg.texture = AlyaEnteringImg;
                }
                else
                {
                    StopAllCoroutines();
                    namePanelTxt.text = lineName;
                    dialoguePanelTxt.text = lineDialogue;
                }
        }
    }

    private void StartDialogue()
    {
        SeparateLine();
        StartCoroutine(TypeLine(namePanelTxt, lineName));
        StartCoroutine(TypeLine(dialoguePanelTxt, lineDialogue));
    }

    private IEnumerator TypeLine(TextMeshProUGUI panelTxt, string line)
    {
        panelTxt.gameObject.SetActive(true);
        foreach (char c in line)
        {
            panelTxt.text += c;
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

        SeparateLine(line);
        StartCoroutine(TypeLine(namePanelTxt, lineName));
        StartCoroutine(TypeLine(dialoguePanelTxt, lineDialogue));
    }

    private void SeparateLine(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            lines = File.ReadAllLines(filePath);
            line = lines[lineIndex];
        }

        if (line.StartsWith("1)") || line.StartsWith("2)") || line.StartsWith("3)"))
        {
            namePanelTxt.transform.parent.gameObject.SetActive(false);
            dialoguePanelTxt.transform.parent.gameObject.SetActive(false);

            dialogueOptions.Clear();
            isChoosing = true;
            optionIndex = 0;
            ReadOptions();
        }
        else
        {
            string patternName = @"^(.*?): ";
            string patternDialogue = @": (.*)";

            Match matchName = Regex.Match(line, patternName);
            Match matchDialogue = Regex.Match(line, patternDialogue);

            if (matchName.Success)
                lineName = matchName.Groups[1].Value;

            if (matchDialogue.Success)
                lineDialogue = matchDialogue.Groups[1].Value;
        }
    }

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
            SeparateLine();
            StartCoroutine(TypeLine(namePanelTxt, lineName));
            StartCoroutine(TypeLine(dialoguePanelTxt, lineDialogue));
        }
    }

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

                while (lineIndex < lines.Length && !lines[lineIndex].StartsWith("1)") && !lines[lineIndex].StartsWith("2)") && !lines[lineIndex].StartsWith("3)") && !lines[lineIndex].StartsWith("("))
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
        string pattern = @"“(.*?)”\(([-+*]?)\)";

        Match match = Regex.Match(optionLine, pattern);
        if (match.Success)
        {
            string option = match.Groups[1].Value;
            string value = match.Groups[2].Value;
            return value + option;
        }
        return (string.Empty);
    }

    private void ShowOptions()
    {
        if (dialogueOptions.Count == 3)
        {
            options.SetActive(true);
            optionText1.text = dialogueOptions[0].Prompt.Substring(1);
            optionText2.text = dialogueOptions[1].Prompt.Substring(1);
            optionText3.text = dialogueOptions[2].Prompt.Substring(1);
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

        namePanelTxt.transform.parent.gameObject.SetActive(true);
        dialoguePanelTxt.transform.parent.gameObject.SetActive(true);

        StopAllCoroutines();
        NextLine();
    }
}

[System.Serializable]
public class DialogueOption
{
    public string Prompt;
    public List<string> Responses = new List<string>();
}
