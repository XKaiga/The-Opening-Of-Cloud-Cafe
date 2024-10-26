using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class DrinkStationManager : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private SpriteRenderer currBase;
    [SerializeField] private Sprite[] basesTexs;

    [SerializeField] private GameObject toppingSpawner;
    [SerializeField] private Sprite[] toppingsTexs;

    [SerializeField] private Transform drinkPlace;
    [SerializeField] private Transform drinkBasePlace;
    [SerializeField] private Transform drinkToppingsPlace;

    [SerializeField] private float cupAnimDuration = 2f;
    private GameObject currentCup = null;
    [SerializeField] private int cupMaxCapacity = 5;
    private List<String> cupRecepe = new List<string>();

    private void Awake()
    {
        mainCam = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(mainCam.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;

        Collider2D collider = rayHit.collider;
        string colliderName = collider.name.ToLower();

        if (CupClicked(collider, colliderName) || DrinkStationClicked(collider, colliderName))
            return;
    }

    private bool CupClicked(Collider2D collider, string colliderName)
    {
        if (colliderName.Contains("cup"))
        {
            if (colliderName.Contains("clone") && currentCup.transform.position == drinkPlace.position)
            {
                Debug.Log("Foi servido uma bebida com:");
                foreach (var item in cupRecepe)
                    Debug.Log(item);

                Destroy(currentCup);
                cupRecepe.Clear();
            }
            else
            {
                if (currentCup == null)
                    currentCup = Instantiate(collider.gameObject, drinkPlace.position, drinkPlace.rotation);
                else if (currentCup.transform.position == drinkPlace.position)
                {
                    Destroy(currentCup);
                    cupRecepe.Clear();

                    currentCup = Instantiate(collider.gameObject, drinkPlace.position, drinkPlace.rotation);
                }
            }
            return true;
        }
        return false;
    }

    private bool DrinkStationClicked(Collider2D collider, string colliderName)
    {
        var colliderParent = collider.transform.parent;
        if (colliderParent != null && colliderParent.parent != null)
        {
            var drinkStationClicked = colliderParent.parent.name.ToLower().Contains("station");
            if (drinkStationClicked && currentCup != null && currentCup.transform.position == drinkPlace.position && cupRecepe.Count < cupMaxCapacity)
            {

                var colliderParentName = colliderParent.name.ToLower();
                if (colliderParentName.Contains("base"))
                    StartCoroutine(PutInCup(drinkBasePlace.position, basesTexs, colliderName, currBase));
                else if (colliderParentName.Contains("top"))
                    StartCoroutine(PutInCup(drinkToppingsPlace.position, toppingsTexs, colliderName, toppingSpawner.GetComponent<SpriteRenderer>()));

                return true;
            }
        }
        return false;
    }

    private IEnumerator PutInCup(Vector3 cupPosition, Sprite[] textures, string colliderName, SpriteRenderer currentSpriteRender)
    {
        currentCup.transform.position = cupPosition;

        bool spriteFound = false;
        foreach (Sprite sprite in textures)
        {
            if (sprite.name.ToLower().Contains(colliderName.ToLower()))
            {
                spriteFound = true;

                if (toppingsTexs.Contains(sprite))
                    StartCoroutine(StartToppingAnimation(sprite, cupAnimDuration));
                else
                    currentSpriteRender.sprite = sprite;

                cupRecepe.Add(colliderName.ToLower());
                break;
            }
        }

        if (spriteFound == false)
            Debug.LogWarning("No sprite contains the name " + colliderName + ".");

        yield return new WaitForSeconds(cupAnimDuration);

        currentSpriteRender.sprite = null;

        currentCup.transform.position = drinkPlace.position;
    }

    private IEnumerator StartToppingAnimation(Sprite topping, float duration)
    {
        GameObject newTopping = Instantiate(toppingSpawner, toppingSpawner.transform.position, toppingSpawner.transform.rotation);
        newTopping.transform.parent = currentCup.transform;

        SpriteRenderer sr = newTopping.GetComponent<SpriteRenderer>();
        sr.sprite = topping;

        float elapsedTime = 0f;
        Vector3 startPos = newTopping.transform.position;

        float minXPos = newTopping.transform.position.x - currentCup.GetComponent<BoxCollider2D>().size.x / 4;
        float maxXPos = newTopping.transform.position.x + currentCup.GetComponent<BoxCollider2D>().size.x / 4;

        float minYPos = currentCup.transform.position.y + currentCup.GetComponent<BoxCollider2D>().size.y / 4;
        float maxYPos = currentCup.transform.position.y + currentCup.GetComponent<BoxCollider2D>().size.y / 2;

        Vector3 endPos = new Vector3(Random.Range(minXPos, maxXPos), Random.Range(minYPos, maxYPos), newTopping.transform.position.z);

        float rotationSpeed = Random.Range(60f, 180f);
        int rotationDirection = Random.Range(0, 2) == 0 ? 1 : -1;

        while (elapsedTime < duration)
        {
            newTopping.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);

            newTopping.transform.Rotate(0, 0, rotationDirection * rotationSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        newTopping.transform.position = endPos;
    }
}
