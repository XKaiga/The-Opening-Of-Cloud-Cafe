using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    [SerializeField] private TrashDrag trash;
    private Vector2 initTrashPos = Vector2.zero;
    public static int trashMaxQty = 5;
    private int currTrashQty = 2;

    public static int taskTimer = 5;

    private void Start()
    {
        initTrashPos = trash.transform.position;
        trash.gameObject.SetActive(false);

        FillTrash(0);
    }

    public void FillTrash(int quantity)
    {
        if (quantity < 0)
            return;

        currTrashQty += quantity;

        if (currTrashQty >= trashMaxQty)
        {
            currTrashQty = trashMaxQty;
            TrashDrag.readyToRemoveTrash = true;
        }

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
        TrashRemoved();
        Money.AddTaskScore();
    }

    private void TrashRemoved()
    {
        currTrashQty = 0;
        trash.transform.position = initTrashPos;
        trash.gameObject.SetActive(false);

        TrashDrag.readyToRemoveTrash = false;
    }
}
