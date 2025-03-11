using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
#if UNITY_EDITOR
using Microsoft.Unity.VisualStudio.Editor;
#endif
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.ShaderGraph;
#endif
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;

public class CollectablesPassFourScript : MonoBehaviour
{
    [SerializeField]
    private TerrainPassOneScript firstPass;     //access first pass
    [SerializeField]
    private TerrainPassTwoScript secondPass;    //access second pass
    [SerializeField]
    private EnemyPassThreeScript thirdPass;    //access second pass
    [SerializeField]
    private GameObject coin;   //coin prefab
    public int maxLevelCoins = 70;   //max number of coins per level (set in parameter UI)
    private int maxCoins = 7, minCoins = 3, coinsInSet = 0;     //the most/least coins that should be spawned together, how many coins in the current set
    private float groundY;  //height of ground at pos being checked
    private bool groundBelow, platformBelow;   //whether currently checking a ground pos or gap
    public int totalCoins;  //store how many coins are in the level to display in UI later
    private int yOffset = 1;    //distance from ground to spawn coin
    public bool passFourCompleted;  //keep track of when this script finishes

    //coin check states
    private enum CoinStates
    {
        edgeCheck,  //check for edges of the level
        maxLimit,  //how many in current set
        minLimit,   //ensure there are enough coins together (dont want individual coins)
        platformChance,     //increase chance for a platform
        timeSinceCoin,    //increase the further apart they are
        spawnCoin   //create the coin
    }
    
    // Update is called once per frame
    void Update()
    {
        //run this pass once previous pass finishes
        if (thirdPass.passThreeCompleted == true && passFourCompleted == false)
        {
            GenerateCoins();
            passFourCompleted = true;
        }
    }

    private void GenerateCoins()
    {
        CoinStates currentState = CoinStates.edgeCheck;
        bool checkComplete = false;
        int blocksSinceCoin = 0;    //blocks since a coin was last spawned

         //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX - firstPass.endPlatformLength; i++)
        {
            currentState = CoinStates.edgeCheck;  //reset state at start of each block
            checkComplete = false;
            float progressChance = 0.5f;    //initial chance of progressing to next stage

            //start of Markovs
            CheckGround(i);  //pass in the current X value being checked to see whether there is ground

            while (!checkComplete)
            {
                switch (currentState)
                {
                    case CoinStates.edgeCheck:
                        //check if we are too close to edges of the level
                        //reason this is before the minLimit may seem counterintuative but I dont want coins in end zone even if a chain has already started,
                        //therefore having this first will cut it off regardless
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
                            blocksSinceCoin++;
                            coinsInSet = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = CoinStates.minLimit;
                            continue;
                        }
                    
                    case CoinStates.minLimit:
                        //check if there are enough coins in set
                        if (coinsInSet > 0 && coinsInSet < minCoins)
                        {
                            //skip to spawning a coin to make correct number
                            currentState = CoinStates.spawnCoin;
                            continue;
                        }
                        else
                        {
                            //progress as normal
                            progressChance = 0.05f; //start other checks with a 5% chance
                            currentState = CoinStates.maxLimit;
                            continue;
                        }

                    case CoinStates.maxLimit:
                        //check too many coins in set
                        if (coinsInSet > maxCoins)
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
                            blocksSinceCoin++;
                            coinsInSet = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            progressChance = 0.05f; //start other checks with a 5% chance
                            currentState = CoinStates.timeSinceCoin;
                            continue;
                        }
                    
                    case CoinStates.timeSinceCoin:
                        
                        progressChance += blocksSinceCoin * 0.05f;  //adds a 5% chance to pass per block since enemy
                       
                        if (coinsInSet > 0)
                            progressChance += 0.65f; // increase chance if a set has already began

                        //if it fails to pass the stage check then end the chain and move on to the next loop,
                        //otherwise fall through to next stage
                        if (!MoveStages(progressChance))
                        {
                            blocksSinceCoin++;
                            coinsInSet = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = CoinStates.platformChance;
                            continue;
                        }

                    case CoinStates.platformChance:
                        //add 60% to current progress chance if there is a platform
                        //the idea here is that if it comes into this check with, e.g. 20% chance it will be 80% likely to pass if on a platform, but if not
                        //will have to beat another progress chance at just 20%
                        if (platformBelow)
                            progressChance += 0.6f;

                        //if it fails to pass the stage check then end the chain and move on to the next loop,
                        //otherwise fall through to next stage
                        if (!MoveStages(progressChance))
                        {
                            blocksSinceCoin++;
                            coinsInSet = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            progressChance = 0.05f; //start other checks with a 5% chance
                            currentState = CoinStates.spawnCoin;
                            continue;
                        }

                    case CoinStates.spawnCoin:
                        //if starting a new set then get a new yOffset, else keep them in a line
                        if (coinsInSet == 0)
                        {
                            yOffset = UnityEngine.Random.Range(1, 4);   //choose a random height off the ground
                            Instantiate(coin, new Vector3(i, groundY + yOffset), Quaternion.identity);
                        }
                        else
                        {
                            //skip the randomiser line
                            Instantiate(coin, new Vector3(i, groundY + yOffset), Quaternion.identity);
                        }

                        totalCoins++;   //increase count of coins in the level
                        blocksSinceCoin = 0;  //reset time counter
                        coinsInSet++;   //add 1 to coins counter for set
                        checkComplete = true;   //exit while loop
                        break;
                }
            }
        }
    }

    //check what is below at current point
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

    //decide whether to progress stage
    private bool MoveStages(float chance)
    {
        //get a random float to compare current chance to
        float comparison = UnityEngine.Random.Range(0.0f, 1.0f);

        //decide whether or not to progress (false means end, true means move to next stage then decide)
        if (comparison < chance)
            return true;
        else
           return false;
    }
}
