using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashDrag : MonoBehaviour
{
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

    void OnMouseDrag()
    {
        if (isDragging && readyToRemoveTrash)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition + offset;
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}
