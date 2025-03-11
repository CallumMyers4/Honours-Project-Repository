using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseGeneratorScript : MonoBehaviour
{
    private int width = 256, height = 256;  //width and height of map to draw
    private int xOffset, yOffset;   //used to randomise the position of the noise to create new levels
    [HideInInspector]
    public float scale = 20.0f;     //zoom of the noise, affects how jagged terrain is
    [SerializeField]
    public int seed = 0;   //randomise map each time
    private Texture2D perlinNoise;  //store the noise map
    public bool noiseGenerated = false;     //check when noise generation is completed

    void Start()
    {
        seed = Random.Range(0, 9999);     //set a random seeds

        //offset using the seed for new textures each time
        xOffset = seed;
        yOffset = seed;

        CreatePerlinNoise();    //populate noise texture

        //zoom in/out according to value set by player on parameter menu
        scale = GameObject.Find("Level Manager").GetComponent<GeneralLevelManagerScript>().hills;

        noiseGenerated = true;          //tell next script to run
    }

    //returns the generated perlin noise texture
    public Texture2D GetNoiseTexture()
    {
        return perlinNoise;
    }

    //generate a noise map
    public void CreatePerlinNoise()
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

                float value = Mathf.PerlinNoise(xCoordinate, yCoordinate);      //calculate the value according to Unity built-in perlin function
                Color color = new Color(value, value, value);   //creates a colour based on the value given by perlin function

                perlinNoise.SetPixel(x, y, color);  //add the colour calculated to the texture
            }
        }

        perlinNoise.Apply();    //update texture with new colours
    }
}
