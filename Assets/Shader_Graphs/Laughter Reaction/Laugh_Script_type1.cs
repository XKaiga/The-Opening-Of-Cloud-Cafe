using UnityEngine;

public class ShaderHeightController : MonoBehaviour
{
    public Material material; // Assign your material here in the Inspector
    public float changeInterval = 0.5f; // Time interval between changes
    public int type = 0; // Time interval between changes
    private float[] yValues_1 = { 0.35f, 0f, 0.7f, 0.25f, 0.6f }; // The Y values to cycle through
    private int currentIndex = 0; // Index to track the current Y value
    private float timer = 2f; // Timer to track time elapsed

    void Start()
    {
        if (material == null)
        {
            Debug.LogError("Please assign a material to this script!");
        }
    }

    void Update()
    {
            
        if (material != null)
        {

            Debug.Log("Type: " + type); // Debug para garantir que o tipo está correto
          
                // Increment the timer
                timer += Time.deltaTime;

                // Check if it's time to change the Y value
                if (timer >= changeInterval)
                {
                // Reset the timer
                timer = 0f;

                // Update to the next Y value in the array
                currentIndex = (currentIndex + 1) % yValues_1.Length;
                float yValue1 = yValues_1[currentIndex];

                // Set the new vector value for the shader
                
                material.SetVector("_Height", new Vector2(0, yValue1));
                }
            
        }
    }
}
