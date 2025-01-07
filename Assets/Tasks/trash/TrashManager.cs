using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    public Timer trashTimer;
    [SerializeField] private TableManager tableManager;

    [SerializeField] private List<GameObject> recicleBins;
    [SerializeField] private TrashDrag trash;
    public static TrashType currTrashType = TrashType.Default;
    private Vector2 initTrashPos;
    private const float finalTrashYPos = -0.2f;
    public static int trashMaxQty = 3;
    [SerializeField] public static int currTrashQty = 0;

    public static int taskTimer = 23;

    private void Start()
    {
        initTrashPos = trash.transform.localPosition;
        trash.gameObject.SetActive(false);

        ShowTrash(0);
    }

    private void OnEnable()
    {
        foreach (var bin in recicleBins)
            bin.SetActive(TrashDrag.readyToRemoveTrash);
    }

    public static void FillTrash(int quantity)
    {
        if (quantity < 0)
            return;

        currTrashQty += quantity;

        if (currTrashQty >= trashMaxQty && !TrashDrag.readyToRemoveTrash)
        {
            MainCoffeeManager.activeTasks.Add(new(TrashManager.taskTimer, TaskType.Trash));
            currTrashQty = trashMaxQty;
            TrashDrag.readyToRemoveTrash = true;
        }
    }

    public void ShowTrash(int quantity)
    {
        FillTrash(quantity);

        float showLimit = trashMaxQty * 2 / 3;
        if (currTrashQty >= showLimit)
        {
            trash.gameObject.SetActive(true);

            float value = currTrashQty - showLimit;
            float maxValue = trashMaxQty - showLimit;

            float proportion = value / maxValue;
            float newYPos = initTrashPos.y + proportion * (finalTrashYPos - initTrashPos.y);

            trash.transform.localPosition = new Vector3(initTrashPos.x, newYPos, trash.transform.position.z);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Sprite defaultTrashSprite = Resources.Load<Sprite>("Trash/Lixo_Default");
        if (trash.gameObject.GetComponent<SpriteRenderer>().sprite != defaultTrashSprite)
            return;

        currTrashType = (TrashType)Random.Range(1, 4);
        Sprite trashSprite = Resources.Load<Sprite>("Trash/Lixo_" + currTrashType);
        trash.gameObject.GetComponent<SpriteRenderer>().sprite = trashSprite;
    }

    public void TrashRemoved()
    {
        currTrashQty = 0;
        trash.transform.position = initTrashPos;
        Sprite defaultTrashSprite = Resources.Load<Sprite>("Trash/Lixo_Default");
        trash.gameObject.GetComponent<SpriteRenderer>().sprite = defaultTrashSprite;
        currTrashType = TrashType.Default;
        trash.gameObject.SetActive(false);

        foreach (var bin in recicleBins)
            bin.SetActive(false);

        TrashDrag.readyToRemoveTrash = false;

        MainCoffeeManager.RemoveTask(TaskType.Trash);

        tableManager.RemoveTrashTimer();
    }
}

public enum TrashType
{
    Default,
    Vidro,
    Plastico,
    Cartao
}