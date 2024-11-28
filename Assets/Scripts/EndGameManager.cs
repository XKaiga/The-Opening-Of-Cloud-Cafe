using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager instance;

    [SerializeField] private GameObject points;
    [SerializeField] private Text pointsTxt;
    [SerializeField] private GameObject ending;
    [SerializeField] private Text endingTxt;
    [SerializeField] private GameObject txtExt;

    private RawImage image;
    private const float fadeSpeed = 0.5f;

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
    private void Start()
    {
        image = GetComponent<RawImage>();
    }

    public void StartEndGame()
    {
        if (!Dialogue.startingNewDay)
            return;

        DeactivateAllButtons();
        StartCoroutine(ShowEndGame());
    }

    private IEnumerator ShowEndGame()
    {
        //aparecer mensagem final
        string typeOfEnding = Money.playerScore < 1167 ? "Bad" : Money.playerScore > 2333 ? "Good" : "Neutral";
        
        var finalTypeDetermined = Finals.DetermineType(typeOfEnding);
        if (finalTypeDetermined == null)
            yield return null;

        if (finalTypeDetermined is FinalType finalType)
        {
            txtExt.SetActive(true);
            string name = GameManager.startDayNum == 2? "ALYIA" : Dialogue.GetMostFavoredCharacter().ToUpper();
            txtExt.GetComponent<TextMeshPro>().text = Finals.finalsList.First(f => f.finalType == finalType && f.clientName == "").finalTxt;
        }

        //fade in
        while (image.color.a < 1)
        {
            Color currentColor = image.color;
            float newAlpha = Mathf.MoveTowards(currentColor.a, 1f, fadeSpeed * Time.deltaTime);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        //amostrar stats
        points.SetActive(true);
        ending.SetActive(true);

        pointsTxt.text = ((int)Money.playerScore).ToString();

        endingTxt.text = typeOfEnding + " Ending";

        //fade out
        while (image.color.a > 0)
        {
           Color currentColor = image.color;
           float newAlpha = Mathf.MoveTowards(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
           image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }
    }

    public static void DeactivateAllButtons()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
            if (obj.name.ToLower().Contains("btn"))
                obj.SetActive(false);
    }
}
