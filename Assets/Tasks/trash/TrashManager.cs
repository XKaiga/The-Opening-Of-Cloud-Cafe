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
    private Vector2 initTrashPos = new Vector2(-3.17f, -0.59f); 
    public static int trashMaxQty = 10;
    [SerializeField] public static int currTrashQty = 0;

    public static int taskTimer = 10;

    private void Start()
    {
        initTrashPos = trash.transform.position;
        trash.gameObject.SetActive(false);

        foreach (var bin in recicleBins)
            bin.SetActive(TrashDrag.readyToRemoveTrash);

        ShowTrash(0);
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

            float qtyOfTrashShowing = currTrashQty - showLimit;

            float qtyNeededToMoveOne = (float)(trashMaxQty - showLimit) / 5;

            float needToMove = qtyOfTrashShowing / qtyNeededToMoveOne;

            trash.transform.position = new Vector3(trash.transform.position.x, trash.transform.position.y + needToMove / 10, trash.transform.position.z);
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