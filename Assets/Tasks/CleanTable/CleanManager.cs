using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanManager : MonoBehaviour
{
    [SerializeField] private Timer timer;

    public Coroutine drawing;
    [SerializeField] private GameObject linePrefab;
    public static float widthMultiplier = 0.3f;

    public Camera mainCam;
    public static List<LineRenderer> drawnLineRenderers = new List<LineRenderer>();

    [SerializeField] private SpriteMask spriteMask;
    [SerializeField] private Camera spriteCam;

    [SerializeField] private BlobManager blobManager;
    public static bool clean = true;

    [SerializeField] private GameObject clothSprite; // Prefab do pano
    private GameObject activeCloth; // Instância ativa do pano


    public static int taskTimer = 20;

    private void Awake() { linePrefab.GetComponent<LineRenderer>().widthMultiplier = widthMultiplier; }

    void Update()
    {
        if (!clean || clean)
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

            // Instanciar o pano
            if (activeCloth == null)
            {
                activeCloth = Instantiate(clothSprite, Vector3.zero, Quaternion.identity);
            }

        drawing = StartCoroutine(DrawLine());
    }

    private void FinishLine()
    {
        if (drawing != null)
            StopCoroutine(drawing);

        blobManager.CaptureBlobState();
        clean = blobManager.IsBoardClean();

        if (activeCloth != null)
        {
            Destroy(activeCloth); //Remover o pano
            activeCloth = null;
        }



        if (clean && timer.timerIsRunning)
        {
            timer.StopTimer(timer.timeRemaining);
            Money.AddTaskScore();
        }
        
        if (clean || !timer.timerIsRunning)
        {
            blobManager.RemoveBlobs();
            ClearLines();
            MainCoffeeManager.RemoveTask(TaskType.Clean);
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
            //play pano sound
            AudioClip soundEffect = Resources.Load<AudioClip>("SoundEffects/" + "sfx_pano");
            AudioSource.PlayClipAtPoint(soundEffect, Vector3.zero, Music.vfxVolume);

            Vector3 position = mainCam.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0;
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, position);

                // Atualizar a posição do pano
            if (activeCloth != null)
            {
                activeCloth.transform.position = position;
            }

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
        clean = true;
    }
}
