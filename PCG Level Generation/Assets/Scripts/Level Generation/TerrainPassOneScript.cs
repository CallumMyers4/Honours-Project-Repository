using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPassOneScript : MonoBehaviour
{
    [SerializeField]
    public float startX, endX = 200;     //start and end positions for the level
    [SerializeField]
    public int lowestY, highestY;    //top and bottom constraints of the level height
    [SerializeField]
    public int maxHeightChange, startPlatformLength, endPlatformLength = 4;     //max height between blocks, length of starting flat ground, length of win zone
    [SerializeField]
    public PerlinNoiseGeneratorScript noiseGenerator;  //reference to script for generating noise
    [SerializeField]
    private GameObject groundPrefab, endZonePrefab, deathZonePrefab;    //prefabs to make ground, end zone and lose zone
    int previousHeight = 0;     //stores the height of the last column generated
    public bool passOneCompleted = false;   //checks when pass is completed

    void Update()
    {
        //runs once last pass completes
        if (noiseGenerator.noiseGenerated == true && passOneCompleted == false)
        {
            GenerateLevel();
            passOneCompleted = true;    //tells next script that first pass is finished
        }
    }

    private void GenerateLevel()
    {
        float levelWidth = endX - startX;   //works out the total width of the level by subtracting start pos from end pos

        Texture2D noiseTexture = noiseGenerator.GetNoiseTexture();     //assign the premade texture to a var

        //run through each x position in the level
        for (int x = 0; x < levelWidth; x++)
        {
            //create death zone layer 1 block below lowest point in the level
            Instantiate(deathZonePrefab, new Vector3(x + startX, lowestY - 1, 0), Quaternion.identity, transform);

            int groundHeight;
            //sets ground height to 0 until a flat starting platform has been generated (prevents player spawning inside a tile)
            if (x < startPlatformLength || x > endX - endPlatformLength)
            {
                groundHeight = 1;   //keep consistent height

                //create a block of win zone prefabs at the end platform of the level so player has a goal to aim for
                if (x > endX - endPlatformLength)
                {
                    for (int i = x; i < endX; i++)
                    {
                        for (int j = 1; j < highestY; j++)
                        {
                            Instantiate(endZonePrefab, new Vector3(x + startX, j, 0), Quaternion.identity, transform);
                        }
                    }
                }
            }
            //once moved past starting platform
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
                groundHeight = Mathf.Max(groundHeight, 1); //always place at least one block, gaps are created in future script

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