using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager instance;

    [SerializeField] private GameObject points;
    [SerializeField] private Text pointsTxt;
    [SerializeField] private GameObject ending;
    [SerializeField] private Text endingTxt;

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
        if (GameManager.startDayNum <= 3 || !Dialogue.startingNewDay)
            return;

        DeactivateAllButtons();
        StartCoroutine(ShowEndGame());
    }

    private IEnumerator ShowEndGame()
    {
        while (image.color.a < 1)
        {
            Color currentColor = image.color;
            float newAlpha = Mathf.MoveTowards(currentColor.a, 1f, fadeSpeed * Time.deltaTime);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        points.SetActive(true);
        ending.SetActive(true);

        pointsTxt.text = ((int)Money.playerScore).ToString();

        string typeOfEnding = Money.playerScore < 1167 ? "Bad" : Money.playerScore > 2333 ? "Good" : "Neutral";
        endingTxt.text = typeOfEnding + " Ending";
        //while (image.color.a > 0)
        //{
        //    Color currentColor = image.color;
        //    float newAlpha = Mathf.MoveTowards(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
        //    image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
        //    yield return null;
        //}
    }

    private void DeactivateAllButtons()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
            if (obj.name.Contains("Btn"))
                obj.SetActive(false);
    }
}
