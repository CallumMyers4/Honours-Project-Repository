using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class EnemyPassThreeScript : MonoBehaviour
{
    [SerializeField]
    private TerrainPassOneScript firstPass;     //access first pass
    [SerializeField]
    private TerrainPassTwoScript secondPass;    //access second pass
    [SerializeField]
    private GameObject bat, spider, worm;   //enemy prefabs
    private GameObject enemyChoice;     //stores the current enemy selected by the markov
    public bool passThreeCompleted;   //tells next pass when to run
    private float groundY;  //height of ground at pos being checked
    private bool groundBelow, platformBelow;   //whether currently checking a ground pos or gap

    //enemy check states
    private enum EnemyStates
    {
        edgeCheck,  //do not spawn too close to start/end
        timeSinceEnemies,   //increase chance the longer without an enemy
        chooseEnemy,    //spin 1-3 to choose which enemy to attempt
        chooseAgain,    //if one enemy fails, try another
        spawnEnemy,     //spawn the appropriate enemy
        cancel  //try again elsewhere if all 3 fail
    }
    private enum BatStates
    {
        pathCheck,  //ensure flight path is not blocked
        platformChance  //lower chance over platforms
    }

    private enum SpiderStates
    {
        groundCheck,    //ensures there is not a gap
        flatCheck   //check if there is enough flat ground nearby
    }

    private enum WormStates
    {
        groundCheck,    //ensures there is not a gap (will also check if on a platform here)
        heightChance    //increase spawn chance lower in the level
        
    }

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
        EnemyStates currentState = EnemyStates.edgeCheck;
        bool checkComplete = false;
        int lastEnemyBlocks = 0;    //blocks since an enemy was last spawned

        //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX; i++)
        {
            currentState = EnemyStates.edgeCheck;  //reset state at start of each block
            checkComplete = false;
            float progressChance = 0.5f;    //initial chance of progressing to next stage

            //start of Markovs
            CheckGround(i);  //pass in the current X value being checked to see whether there is ground

            while (!checkComplete)
            {
                switch (currentState)
                {
                    case EnemyStates.edgeCheck:
                        //check if we are too close to edges of the level
                        if (i < (firstPass.startX + firstPass.startPlatformLength) ||
                                i > firstPass.endX - firstPass.endPlatformLength)
                        {
                            progressChance = 0.0f;
                        }
                        else
                        {
                           progressChance = 1.0f;
                        }
                        //if it fails to pass the stage check then end the chain and move on to the next loop,
                        //otherwise fall through to next stage
                        if (!MoveStages(progressChance))
                        {
                            lastEnemyBlocks++;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = EnemyStates.timeSinceEnemies;
                            continue;
                        }

                    case EnemyStates.timeSinceEnemies:
                        progressChance += lastEnemyBlocks * 0.1f;  //adds a 10% chance to pass per block since enemy
                        if (!MoveStages(progressChance))
                        {
                            lastEnemyBlocks++;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = EnemyStates.chooseEnemy;
                            continue;
                        }

                    case EnemyStates.chooseEnemy:
                    //----------------------------------------------This needs to be random--------------------------------------------
                    enemyChoice = bat;
                    if (!BatCheck())
                    {
                        enemyChoice = spider;
                        if (!SpiderCheck())
                        {
                            enemyChoice = worm;

                            if (!WormCheck())
                            {
                                lastEnemyBlocks++;
                                checkComplete = true;
                                break;
                            }
                            else
                            {
                                currentState = EnemyStates.spawnEnemy;
                                continue;
                            }
                        }
                        else
                        {
                            currentState = EnemyStates.spawnEnemy;
                            continue;
                        }
                    }
                    else
                    {
                        currentState = EnemyStates.spawnEnemy;
                        continue;
                    }
                       
                    //will probably need when properly coding the choosing section of the chain
                    /*case EnemyStates.chooseAgain:
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
                    */

                    case EnemyStates.spawnEnemy:
                        Instantiate(enemyChoice, new Vector3(i, groundY), Quaternion.identity);
                        lastEnemyBlocks = 0;  //reset time counter
                        checkComplete = true;
                        break;
                }
            }
        }
    }

    private void CheckGround(int currentX)
    {     
        //fires a raycast to get the ground at current X position (using Raycast instead of RaycastAll because we only need the
        //first object hit since this will be the main ground)        
        RaycastHit2D currentGround = Physics2D.Raycast(new Vector2(currentX, firstPass.highestY + secondPass.platformIncrease + 1), Vector2.down, 
                                                                (MathF.Abs(firstPass.highestY - firstPass.lowestY) + secondPass.platformIncrease + 1));

        //checks for ground at current X
        if (currentGround.collider != null)
        {
            groundY = currentGround.collider.gameObject.transform.position.y;    //store the Y value of the ground
            groundBelow = true;     //used in platforms to affect chance based on whether there is ground found below

            //since platform prefab attaches a child, this is an easy way to check if the ground tile is a platform or not
            if (currentGround.collider.transform.childCount > 0)
            {
                platformBelow = true;
            }
            else
            {
                platformBelow = false;
            }
        }
        else
        {
            groundBelow = false;
            platformBelow = false;
        }
    }

    private bool MoveStages(float chance)
    {
        //get a random float to compare current chance to
        float comparison = UnityEngine.Random.Range(0.0f, 1.0f);

        //decide whether or not to progress (false means the ground stays, true means move to next stage then decide)
        if (comparison < chance)
            return true;
        else
           return false;
    }

    private bool BatCheck()
    {
        return false;
    }

    private bool SpiderCheck()
    {
        return false;
    }

    private bool WormCheck()
    {
        return false;
    }
}