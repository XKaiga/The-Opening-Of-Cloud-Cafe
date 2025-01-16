using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BlobManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;

    [SerializeField] private float cleanThreshold = 0.999f;
    [SerializeField] private TrashManager trashManager;

    [SerializeField] private GameObject blobPrefab = null;
    [SerializeField] private Sprite[] blobsSprites = null;
    private SpriteRenderer[] blobs = null;
    private Vector2 positionMin = new Vector2(-2.19f, -1.06f); //left bottom
    private Vector2 positionMax = new Vector2(2.27f, -0.08f); //right top
    private float scaleMin = 0.15f;
    private float scaleMax = 0.3f;

    private RenderTexture boardRenderTexture;
    private RenderTexture blobRenderTexture;

    private void Awake() { SpawnBlobs(); }
    private void SpawnBlobs()
    {
        if (CleanManager.clean)
            return;

        int numberOfBlobs = Random.Range(3, 11);

        blobs = new SpriteRenderer[numberOfBlobs];

        for (int i = 0; i < numberOfBlobs; i++)
        {
            GameObject blobInstance = Instantiate(blobPrefab, transform);

            SpriteRenderer spriteRenderer = blobInstance.GetComponent<SpriteRenderer>();
            if (blobsSprites.Length != 0)
                spriteRenderer.sprite = blobsSprites[Random.Range(0, blobsSprites.Length)];

            float randomPosX = Random.Range(positionMin.x, positionMax.x);
            float randomPosY = Random.Range(positionMin.y, positionMax.y);
            blobInstance.transform.localPosition = new Vector3(randomPosX, randomPosY, 0);

            float randomScale = Random.Range(scaleMin, scaleMax);
            blobInstance.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            float randomRotationZ = Random.Range(0f, 360f);
            blobInstance.transform.rotation = Quaternion.Euler(0f, 0f, randomRotationZ);

            blobs[i] = spriteRenderer;
        }
    }

    private void Start()
    {
        if (CleanManager.clean)
            return;

        boardRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        blobRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);

        CaptureBoardState();
        CaptureBlobState();
    }

    public void CaptureBoardState()
    {
        foreach (var blob in blobs)
            blob.enabled = false;

        mainCam.targetTexture = boardRenderTexture;
        mainCam.Render();
        mainCam.targetTexture = null;

        foreach (var blob in blobs)
            blob.enabled = true;

        //DisplayRenderTexture(boardRenderTexture, boardImage);
    }

    public void CaptureBlobState()
    {
        foreach (var blob in blobs)
            blob.enabled = true;

        mainCam.targetTexture = blobRenderTexture;
        mainCam.Render();
        mainCam.targetTexture = null;

        //DisplayRenderTexture(blobRenderTexture, blobImage);
    }

    public bool IsBoardClean()
    {
        Texture2D boardTexture = CaptureTextureFromRenderTexture(boardRenderTexture);
        Texture2D blobTexture = CaptureTextureFromRenderTexture(blobRenderTexture);

        int totalPixels = boardTexture.width * boardTexture.height;
        int differingPixels = 0;

        for (int y = 0; y < boardTexture.height; y++)
        {
            for (int x = 0; x < boardTexture.width; x++)
            {
                Color boardPixel = boardTexture.GetPixel(x, y);
                Color blobPixel = blobTexture.GetPixel(x, y);

                if (boardPixel == blobPixel)
                    differingPixels++;
            }
        }

        float cleanPercentage = (float)differingPixels / totalPixels;
        return cleanPercentage >= cleanThreshold;
    }

    private Texture2D CaptureTextureFromRenderTexture(RenderTexture renderTexture)
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;
        return texture;
    }

    public void RemoveBlobs()
    {
        if (SceneManager.GetActiveScene().name == "TrashScene")
            trashManager.ShowTrash(blobs.Length);
        else
            TrashManager.FillTrash(blobs.Length);

        foreach (var blob in blobs)
            Destroy(blob.GameObject());
        blobs = null;
    }

    // Use for Debug: Convert the RenderTexture to Texture2D and assign it to a RawImage to display
    //private void DisplayRenderTexture(RenderTexture renderTexture, RawImage rawImage)
    //{
    //    Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
    //    RenderTexture.active = renderTexture;
    //    texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    //    texture.Apply();
    //    RenderTexture.active = null;

    //    // Assign the texture to the RawImage
    //    rawImage.texture = texture;
    //}
}
