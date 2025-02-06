using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPassOneScript : MonoBehaviour
{
    //made all these public for now for easy access to next script, may change later
    [SerializeField]
    public float startX, endX;     //start and end positions for the level
    [SerializeField]
    public int lowestY, highestY;    //top and bottom constraints of the level height
    [SerializeField]
    public int maxHeightChange, startPlatformLength, endPlatformLength = 4;
    [SerializeField]
    public PerlinNoiseGeneratorScript noiseGenerator;  //reference to script for generating noise
    [SerializeField]
    private GameObject groundPrefab;
    int previousHeight = 0;     //stores the height of the last column generated
    public bool passOneCompleted = false;   //checks when pass is completed

    //in update now instead of start because if noise generation script is not completed by the time this one runs start it will never
    //run the script
    void Update()
    {
        //will not run script unless the noise has successfully generated
        if (noiseGenerator.noiseGenerated == true && passOneCompleted == false)
        {
            GenerateLevel();
            passOneCompleted = true;    //tells next script that first pass is finished
        }
    }

    // Start is called before the first frame update
    private void GenerateLevel()
    {
        float levelWidth = endX - startX;   //works out the total width of the level by subtracting start pos from end pos

        Texture2D noiseTexture = noiseGenerator.GetNoiseTexture();     //assign the premade texture to a var

        //run through each x position in the level
        for (int x = 0; x < levelWidth; x++)
        {
            int groundHeight;

            //sets ground height to 0 until a flat starting platform has been generated (prevents player spawning inside a tile)
            if (x < startPlatformLength)
            {
                groundHeight = 1;
            }
            else
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
                groundHeight = Mathf.FloorToInt(groundDirection * ((highestY - lowestY) / 2) + ((highestY + lowestY) / 2));
                groundHeight = Mathf.Max(groundHeight, 1); //always place at least one block

                //check to ensure the new height does not make the level unplayable, then sets last height to this one for next loop
                groundHeight = Mathf.Clamp(groundHeight, previousHeight - maxHeightChange, previousHeight + maxHeightChange);
                previousHeight = groundHeight;
            }

            //places blocks up to the correct ground height for the current X position
            for (int y = lowestY; y < groundHeight; y++)
            {
                Instantiate(groundPrefab, new Vector3(x + startX, y, 0), Quaternion.identity, transform);
            }
        }
    }
}
