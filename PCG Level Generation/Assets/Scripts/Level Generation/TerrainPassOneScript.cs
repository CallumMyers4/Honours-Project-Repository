using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPassOneScript : MonoBehaviour
{
    [SerializeField]
    private float startX, endX;     //start and end positions for the level
    [SerializeField]
    private float lowestY, highestY;    //top and bottom constraints of the level height
    [SerializeField]
    private PerlinNoiseGeneratorScript noiseGenerator;  //reference to script for generating noise
    [SerializeField]
    private GameObject groundPrefab;

    void Start()
    {
        StartCoroutine(GenerateLevel());
    }

    // Start is called before the first frame update
    IEnumerator GenerateLevel()
    {
        yield return new WaitForSeconds(0.05f);     //gives time for noise generator to populate

        float levelWidth = endX - startX;   //works out the total width of the level by subtracting start pos from end pos

        Texture2D noiseTexture = noiseGenerator.GetNoiseTexture();     //assign the premade texture to a var

        //run through each x position in the level
        for (int x = 0; x < levelWidth; x++)
        {
            int textureX = Mathf.FloorToInt((float)(x + startX) / levelWidth * noiseTexture.width); //moves to the correct pixel within the texture
            Color pixelColor = noiseTexture.GetPixel(textureX, 0); //gets the noise value at this position (0 means it goes in a straight
                                                                    //horiontal line through the texture)

            float noiseValue = pixelColor.grayscale;    //gives a value between 0 and 1
            float groundDirection = (noiseValue * 2) - 1;   //scales to between -1 and 1 to allow for going above or below 0

            /*converts to an int, then ensures a value of -1 would map to the lowest Y value constraint and a value of 1 would map
              to the highest Y constraint
              (highestY - lowestY)/2 scales value
              (highestY + lowestY)/2 centres it back to 0
            */
            int groundHeight = Mathf.FloorToInt(groundDirection * ((highestY - lowestY) / 2) + ((highestY + lowestY) / 2));
            groundHeight = Mathf.Max(groundHeight, 1); //always place at least one block

            //places blocks up to the correct ground height for the current X position
            for (int y = 0; y < groundHeight; y++)
            {
                Instantiate(groundPrefab, new Vector3(x + startX, y, 0), Quaternion.identity, transform);
            }
        }
    }
}
