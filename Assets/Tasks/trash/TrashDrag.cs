using UnityEngine;

public class TrashDrag : MonoBehaviour
{
    [SerializeField] private TrashManager trashManager;

    public static bool readyToRemoveTrash = false;
    private bool isDragging = false;

    private Vector3 offset;
    private Camera mainCamera;
    public AudioClip soundEffect;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        if (readyToRemoveTrash)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            offset = transform.position - mousePosition;

            isDragging = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (TableManager.inAnotherView)
            return;

        if (collision.name.Contains("recicle"))
        {
            string recicleColor = TrashManager.currTrashType == TrashType.Vidro ? "Green" :
                                  TrashManager.currTrashType == TrashType.Plastico ? "Yellow" : "Blue";

            string[] recicleNameParts = collision.name.Split('_');
            if (recicleNameParts[0] == recicleColor && trashManager.trashTimer.timerIsRunning)
                Money.AddTaskScore();
            
            trashManager.TrashRemoved();
        }

    }

    private bool correctSprite = false;
    private bool isSoundPlaying = false;
    void OnMouseDrag()
    {
        if (isDragging && readyToRemoveTrash)
        {
            if (!correctSprite)
            {
                SpriteRenderer spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();

                Sprite trashSprite = Resources.Load<Sprite>("Trash/Lixo_" + TrashManager.currTrashType);
                spriteRenderer.sprite = trashSprite;

                if (!spriteRenderer.sprite.name.Contains("Default"))
                    correctSprite = true;
            }

            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition + offset;

            if (!isSoundPlaying)
            {
                AudioSource.PlayClipAtPoint(soundEffect, transform.position);
                isSoundPlaying = true;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        isSoundPlaying = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}
