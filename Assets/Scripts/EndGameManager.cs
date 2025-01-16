using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] private Dialogue dialogueManager;

    public static EndGameManager instance;

    [SerializeField] private Image pointsEndBgImg;
    [SerializeField] private GameObject points;
    [SerializeField] private Text pointsTxt;
    [SerializeField] private GameObject ending;
    [SerializeField] private Text endingTxt;

    [SerializeField] private GameObject finalParent;
    [SerializeField] private TextMeshProUGUI txtExt;

    public RawImage image;
    private const float fadeSpeed = 0.5f;

    private static List<GameObject> btnsGameObjects = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    public Coroutine StartEndGameCoroutine()
    {
        if (!Dialogue.startingNewDay)
            return null;

        DeactivateAllButtons();
        return StartCoroutine(ShowEndGame());
    }

    private IEnumerator ShowEndGame()
    {
        //aparecer mensagem final
        string typeOfEnding = Money.playerScore < 1167 ? "Bad" : Money.playerScore > 2333 ? "Good" : "Neutral";

        var finalTypeDetermined = Finals.DetermineType(typeOfEnding);
        if (finalTypeDetermined == null)
            yield break;

        if (finalTypeDetermined is FinalType finalType)
        {
            finalParent.SetActive(true);
            txtExt.enabled = true;
            string name = GameManager.startDayNum == 2 ? "ALYIA" : Dialogue.GetMostFavoredCharacter()?.ToUpper() ?? "RONNIE";

            var finalData = Finals.finalsList.FirstOrDefault(f => f.finalType == finalType && f.clientName == name);
            if (finalData != null)
                txtExt.text = finalData.finalTxt;
        }

        //pause for some seconds
        float waitingTime = dialogueManager.EstimateSpeakingTimeSecs(txtExt.text) + 1;
        yield return Wait(waitingTime);

        //fade in
        image.enabled = true;
        pointsEndBgImg.enabled = true;
        bool fadeStats = false;
        while (image.color.a < 1)
        {
            Color currentColor = image.color;
            float newAlpha = Mathf.MoveTowards(currentColor.a, 1f, fadeSpeed * Time.deltaTime);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            if (!fadeStats && image.color.a > 0.85f)
            {
                fadeStats = true;
                points.SetActive(true);
                ending.SetActive(true);

                pointsTxt.text = ((int)Money.playerScore).ToString();

                endingTxt.text = typeOfEnding + " Ending";
            }
            else if (finalParent.activeSelf && image.color.a > 0.55f)
            {
                finalParent.SetActive(false);
                txtExt.enabled = false;
            }
            else if (image.color.a > 0.65f)
            {
                currentColor = pointsEndBgImg.color;
                pointsEndBgImg.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            }
            yield return null;
        }

        yield return Wait(4.5f);

        //fade out
        fadeStats = false;
        while (pointsEndBgImg.color.a > 0)
        {
            Color currentColor = pointsEndBgImg.color;
            float newAlpha = Mathf.MoveTowards(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
            pointsEndBgImg.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            if (!fadeStats && pointsEndBgImg.color.a < 0.85f)
            {
                fadeStats = true;
                points.SetActive(false);
                ending.SetActive(false);
            }
            else if (pointsEndBgImg.color.a < 0.45f)
            {
                currentColor = image.color;
                newAlpha = Mathf.MoveTowards(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
                image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            }
            yield return null;
        }
        pointsEndBgImg.enabled = false;

        while (image.color.a > 0)
        {
            Color currentColor = image.color;
            float newAlpha = Mathf.MoveTowards(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }
        image.enabled = false;

        ActivateAllButtons();
    }

    public static void ActivateAllButtons()
    {
        foreach (GameObject obj in btnsGameObjects)
            obj.SetActive(true);
        btnsGameObjects.Clear();
    }

    public static void DeactivateAllButtons()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
            if (obj.name.ToLower().Contains("btn"))
            {
                btnsGameObjects.Add(obj);
                obj.SetActive(false);
            }
    }

    public static IEnumerator Wait(float seconds, Action callback = null)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }
}
