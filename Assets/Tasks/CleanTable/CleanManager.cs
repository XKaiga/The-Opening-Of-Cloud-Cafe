using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CleanManager : MonoBehaviour
{
    public Coroutine drawing;
    [SerializeField] private GameObject linePrefab;
    public static float widthMultiplier = 0.3f;

    public Camera mainCam;
    public static List<LineRenderer> drawnLineRenderers = new List<LineRenderer>();

    [SerializeField] private SpriteMask spriteMask;
    [SerializeField] private Camera spriteCam;

    [SerializeField] private BlobManager blobManager;
    public static bool clean = true;

    public static int taskTimer = 20;

    private void Awake(){ linePrefab.GetComponent<LineRenderer>().widthMultiplier = widthMultiplier; }

    void Update()
    {
        if (!clean)
        {
            if (Input.GetMouseButtonDown(0))
                StartLine();
            if (Input.GetMouseButtonUp(0))
                FinishLine();
        }
    }

    private void StartLine()
    {
        if (drawing != null)
            StopCoroutine(drawing);
        drawing = StartCoroutine(DrawLine());
    }

    private void FinishLine()
    {
        if (drawing != null)
            StopCoroutine(drawing);

        blobManager.CaptureBlobState();
        clean = blobManager.IsBoardClean();
        if (clean)
        {
            blobManager.RemoveBlobs();
            ClearLines();

            Money.AddTaskScore();
        }
    }

    IEnumerator DrawLine()
    {
        GameObject newGameObject = Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        LineRenderer line = newGameObject.GetComponent<LineRenderer>();
        line.startColor = Color.white;
        drawnLineRenderers.Add(line);
        line.positionCount = 0;
        while (true)
        {
            Vector3 position = mainCam.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0;
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, position);
            AssignScreenAsMask();
            yield return null;
        }
    }

    private void AssignScreenAsMask()
    {
        int height = Screen.height;
        int width = Screen.width;
        int depth = 24;

        RenderTexture renderTexture = new RenderTexture(width, height, depth);
        Rect rect = new Rect(0, 0, width, height);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        spriteCam.targetTexture = renderTexture;
        spriteCam.Render();

        RenderTexture currRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();

        spriteCam.targetTexture = null;
        RenderTexture.active = currRenderTexture;
        Destroy(renderTexture);

        Sprite sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), Screen.height / 10);

        spriteMask.sprite = sprite;
    }

    private void ClearLines()
    {
        foreach (var line in drawnLineRenderers)
            Destroy(line.gameObject);
        drawnLineRenderers = null;
    }
}
