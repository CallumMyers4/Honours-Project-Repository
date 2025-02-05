using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EnemyPassThreeScript : MonoBehaviour
{
    [SerializeField]
    private TerrainPassOneScript firstPass;     //access first pass
    [SerializeField]
    private TerrainPassTwoScript secondPass;    //access second pass
    [SerializeField]
    private GameObject bat, spider, worm;
    public bool passThreeCompleted;   //tells next pass when to run
    private float groundY;  //height of ground at pos being checked
    private bool groundBelow, platformBelow;   //whether currently checking a ground pos or gap

    // Update is called once per frame
    void Update()
    {
        if (secondPass.passTwoCompleted == true && passThreeCompleted == false)
        {
            SpawnEnemies();
            passThreeCompleted = true;
        }
    }

    //spawn enemies
    private void SpawnEnemies()
    {
        //currentState = first state
        //bool checkComplete = false;

        //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX; i++)
        {
            //currentState = platformStates.heightCheck;  //reset state at start of each block
            //checkComplete = false;
            //float progressChance = 0.6f;    //initial chance of progressing to next stage (start at 1 to guarentee passing stage
                                            //1 so long as theres not too long a gap)
            //start of Markovs
            CheckGround(i);  //pass in the current X value being checked to allow the Markov to change values

            //uses an internal loop to get around C# requiring a continue statement and not just allowing a fall to the next state,
            //which was causing it to skip iterations of the block checking
            /*while (!checkComplete)
            {
                switch (currentState)
                {
                    case platformStates.heightCheck:
                        //if starting a new platform find a new Y value to draw at by taking the Y value of the last ground block found
                        //and adding 3
                        if (consecutivePlatformBlocks == 0)
                            platformY = groundY + 3.0f;
                        /////////-----------------------------This may be a TEMP ELSE------------------------------------------
                        //guarentees all platforms are min 2 wide
                        else if (consecutivePlatformBlocks == 1)
                        {
                            currentState = platformStates.placePlatform;
                            break;
                        }
                        //otherwise run it as a check
                        else
                        {
                            //if too close
                            if (platformY < groundY + 3.0f)
                            {
                                //fail
                                progressChance = 0.0f;
                            }
                            else
                            {
                                //pass
                                progressChance = 1.0f;
                            }
                        }
                        //if it fails to pass the stage check then end the chain and move on to the next loop,
                        //otherwise fall through to next stage
                        if (!MoveStagesPlatform(currentState, progressChance))
                        {
                            consecutivePlatformBlocks = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = platformStates.lengthCheck;
                            continue;
                        }

                    case platformStates.lengthCheck:
                        //ensure a new platform block wont go over the max limit
                        if (consecutivePlatformBlocks >= platformLengthLimit)
                        {
                            progressChance = 0;
                        }
                        else
                        {
                            progressChance = 1.0f;
                        }
                        if (!MoveStagesPlatform(currentState, progressChance))
                        {
                            consecutivePlatformBlocks = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = platformStates.groundBelow;
                            continue;
                        }

                    case platformStates.groundBelow:
                        //increases the chance if there is no ground found, or decreases if not over a gap
                        if (groundBelow)
                            progressChance -= 0.8f;
                        else
                            progressChance += 0.5f;
                        if (!MoveStagesPlatform(currentState, progressChance))
                        {
                            consecutivePlatformBlocks = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = platformStates.lengthChance;
                            continue;
                        }

                    case platformStates.lengthChance:
                        //decreases chance by 1% for every platform block on current platform
                        progressChance -= 0.01f * consecutivePlatformBlocks;
                        if (!MoveStagesPlatform(currentState, progressChance))
                        {
                            consecutivePlatformBlocks = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = platformStates.placePlatform;
                            continue;
                        }

                    case platformStates.placePlatform:
                        Instantiate(platformTile, new Vector3(i, platformY), Quaternion.identity);
                        consecutivePlatformBlocks++;  //add to the consecutive gap counter
                        checkComplete = true;
                        break;
                }
            }*/
        }
    }

     private void CheckGround(int currentX)
    {     
        //fires a raycast to get the ground at current X position (using Raycast instead of RaycastAll because we only need the
        //first object hit since this will be the main ground)        
        RaycastHit2D currentGround = Physics2D.Raycast(new Vector2(currentX, firstPass.highestY + secondPass.platformIncrease + 1), Vector2.down, 
                                                                (MathF.Abs(firstPass.highestY - firstPass.lowestY) + secondPass.platformIncrease + 1));

        //stored in the list of platform vars for now, was previously exclusive to this function but saves rewriting code to create
        //a second function to check Y levels for platform checks
        if (currentGround.collider != null)
        {
            groundY = currentGround.collider.gameObject.transform.position.y;    //store the Y value of the ground
            groundBelow = true;     //used in platforms to affect chance based on whether there is ground found below

            if (currentGround.collider.transform.childCount > 0)
            {
                platformBelow = true;
                Debug.Log("Platform detected at x = " + currentX);
            }
            else
            {
                platformBelow = false;
                Debug.Log("Ground detected at x = " + currentX);
            }
        }
        else
        {
            groundBelow = false;
            platformBelow = false;
            Debug.Log("No ground detected at x = " + currentX);
        }
    }
}
