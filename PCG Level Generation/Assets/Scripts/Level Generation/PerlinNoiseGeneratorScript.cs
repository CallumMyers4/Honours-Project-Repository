using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseGeneratorScript : MonoBehaviour
{
    private int width = 256, height = 256;
    private int xOffset, yOffset;   //used to randomise the position of the noise to create new levels
    private float scale = 20.0f;
    [SerializeField]
    private int seed = 0;   //in later iteration there will be an option for user input, but random for now
    public Texture2D perlinNoise;
    private Renderer rendererC;     //just stores component (remove after testing)

    // Start is called before the first frame update
    void Start()
    {
        rendererC = GetComponent<Renderer>();   //gets component (remove after testing)
    }

    // Update is called once per frame
    void Update()
    {
        //------------------------move to start function after testing-------------------------------//
        //seed = Random.Range(0, 9999);     //set a random seed
        
        //set the offsets to use this seed
        xOffset = seed;
        yOffset = seed;
        //------------------------------------------------------------------------------------------//

        CreatePerlinNoise();    //generates the texture
    }

    private void CreatePerlinNoise()
    {
        perlinNoise = new Texture2D(width, height);     //create a new texture to store the noise

        //run through every pixel and set it to the perlin noise value for that position
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //find the correct position by dividing by the scale (zoom) then adding the offset
                float xCoordinate = (float) x / scale + xOffset;
                float yCoordinate = (float) y / scale + yOffset;

                float value = Mathf.PerlinNoise(xCoordinate, yCoordinate);      //calculate the value according to built in function
                Color color = new Color(value, value, value);   //creates a colour based on the value given by perlin function

                perlinNoise.SetPixel(x, y, color);  //add the colour calculated to the texture
            }
        }

        perlinNoise.Apply();    //update texture with new colours
        rendererC.material.mainTexture = perlinNoise;    //shows in renderer (remove after testing)
    }
}
