using UnityEngine;

public class ShaderHeightController2 : MonoBehaviour
{
    public Material material; // Assign your material here in the Inspector
    public float changeInterval = 0.5f; // Time interval between changes
    public int type = 0; // Time interval between changes
    private float[] yValues_2 =  { 0.15f, 0.7f, 0.35f, 0.65f, 0.2f }; // The Y values to cycle through
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
                currentIndex = (currentIndex + 1) % yValues_2.Length;
                float yValue2 = yValues_2[currentIndex];

                // Set the new vector value for the shader
                material.SetVector("_Height", new Vector2(0, yValue2));

            }   
            
        }
    }
}
