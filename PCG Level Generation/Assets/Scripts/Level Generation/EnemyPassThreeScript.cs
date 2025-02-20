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
    private float batHeight = 4, wormHeight = 1.64f;    //distance above ground to spawn bat, distance to move worm down
    private bool groundBelow, platformBelow;   //whether currently checking a ground pos or gap
    
    public int maxEnemies;  //max enemies per level as set in parameters UI

    //enemy check states
    private enum EnemyStates
    {
        edgeCheck,  //do not spawn too close to start/end
        timeSinceEnemies,   //increase chance the longer without an enemy
        chooseEnemy,    //spin 1-3 to choose which enemy to attempt
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
        for (int i = firstPass.startPlatformLength; i < firstPass.endX - firstPass.endPlatformLength; i++)
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
                            progressChance = 0.05f; //start other checks with a 5% chance
                            currentState = EnemyStates.timeSinceEnemies;
                            continue;
                        }

                    case EnemyStates.timeSinceEnemies:
                        progressChance += lastEnemyBlocks * 0.05f;  //adds a 5% chance to pass per block since enemy
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

                    //put enemies into a list
                    List<GameObject> enemies = new List<GameObject> { bat, spider };

                    //assigns each object in the list a random float from 0-1 then orders in the list according to the value returned
                    enemies = enemies.OrderBy(randFlt => UnityEngine.Random.value).ToList();  

                    //cycles through list
                    for (int j = 0; j < enemies.Count; j++)
                    {
                        enemyChoice = enemies[j];

                        //gets the current enemy being checked, runs the check Markov function for appropriate enemy, and either spawns or moves on according to return
                        if (enemyChoice == bat && BatCheck(i))
                        {
                            currentState = EnemyStates.spawnEnemy;
                            break;
                        }
                        else if (enemyChoice == spider && SpiderCheck(i))
                        {
                            currentState = EnemyStates.spawnEnemy;
                            break;
                        }
                        //---------------------------------------------Removed for now--------------------------------
                        /*else if (enemyChoice == worm && WormCheck(i))
                        {
                            Debug.Log("Worm spawning.");
                            currentState = EnemyStates.spawnEnemy;
                            break;
                        }*/
                    }

                    //go to cancel if no enemy was found, otherwise cycle through while loop again to reach enemyStates.spawnenemy
                    if (currentState != EnemyStates.spawnEnemy)
                    {
                        currentState = EnemyStates.cancel;
                        continue;
                    }
                    else
                    {
                        continue;
                    }

                    case EnemyStates.spawnEnemy:
                        //two seperate spawn locations since bat should be higher than the ground
                        if (enemyChoice == bat)
                        {        
                            Instantiate(enemyChoice, new Vector3(i, groundY + batHeight), Quaternion.identity);
                        }
                        else if (enemyChoice == spider)
                        {                           
                            Instantiate(enemyChoice, new Vector3(i, groundY + 1), Quaternion.identity);
                        }
                        else
                        {
                            Instantiate(enemyChoice, new Vector3(i, groundY + wormHeight), Quaternion.identity);
                        }

                        lastEnemyBlocks = 0;  //reset time counter
                        checkComplete = true;   //exit while loop
                        break;
                        
                    //move on to next block if no enemy can be spawned here
                    case EnemyStates.cancel:
                    lastEnemyBlocks++;
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

    private bool BatCheck(int spawnX)
    {
        BatStates currentState = BatStates.pathCheck;   
        
        //distance to check forward/back (later this could be calculated from the flight path length in bat's script)
        bool check = false;
        int distanceAhead = 2, distanceBack = 2;

        while (!check)
        {
            switch (currentState)
            {
            case BatStates.pathCheck:
                //fire a raycast right to check for blocks
                RaycastHit2D checkForGround = Physics2D.Raycast(new Vector2(spawnX, groundY + batHeight), Vector2.right, distanceAhead);

                //if the ground exists, end check, else check left
                if (checkForGround.collider != null)  
                {
                    return false; //fail check
                }                                                      
                else
                {
                    //now use -.right to go left, and if it still passes then move on
                    checkForGround = Physics2D.Raycast(new Vector2(spawnX, groundY + batHeight), -Vector2.right, distanceBack);
                    if (checkForGround.collider != null)  
                    {
                        return false;   //fail check
                    }   
                    else
                    {
                        //passed - move to next stage
                        currentState = BatStates.platformChance;
                        continue;
                    }
                }

            //if theres a platform then chance = 20%, else = 40%
            case BatStates.platformChance:
                float progressChance = 0.4f;
                if (platformBelow)
                    progressChance -= 0.2f;

                if (!MoveStages(progressChance))
                    return false;
                else
                    return true;
            }
        }

        return false;   //should never be reached but editor was complaining that not all paths return a value
    }

    private bool SpiderCheck(int spawnX)
    {
        SpiderStates currentState = SpiderStates.groundCheck;   
        
        //distance to check forward/back (later this could be calculated from the flight path length in bat's script)
        bool check = false;
        int distanceAhead = 4, distanceBack = 4;

        while (!check)
        {
            switch (currentState)
            {
            case SpiderStates.groundCheck:
                //if ground is below then continue to next state, else fail
                if (groundBelow)
                {
                    currentState = SpiderStates.flatCheck;
                    continue;
                }
                else
                {
                    return false;
                }

            case SpiderStates.flatCheck:
                //fire a raycast right to check for blocks
                RaycastHit2D checkForGround = Physics2D.Raycast(new Vector2(spawnX, groundY + batHeight), Vector2.right, distanceAhead);

                //if the ground exists, end check, else check left
                if (checkForGround.collider != null)  
                {
                    return false; //fail check
                }                                                      
                else
                {
                    //now use -.right to go left, and if it still passes then move on
                    checkForGround = Physics2D.Raycast(new Vector2(spawnX, groundY + batHeight), -Vector2.right, distanceBack);
                    if (checkForGround.collider != null)  
                    {
                        return false;   //fail check
                    }   
                    else
                    {
                        //passed - spawn (this one has no chance modifier)
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool WormCheck(int spawnX)
    {
        WormStates currentState = WormStates.groundCheck;   
        
        //distance to check forward/back (later this could be calculated from the flight path length in bat's script)
        bool check = false;

        while (!check)
        {
            switch (currentState)
            {
            case WormStates.groundCheck:
                //if there is no ground, or there is a platform, worm cant spawn
                if (!groundBelow || platformBelow)
                {
                    return false;
                }

                //check to make sure it wont spawn on edges of ground (makes some level sections impossible if jumping over a gap right before)
                bool groundAhead = Physics2D.Raycast(new Vector2(spawnX + 1, groundY), Vector2.down, 1f);
                bool groundBehind = Physics2D.Raycast(new Vector2(spawnX - 1, groundY), Vector2.down, 1f);

                //fail if on the edge
                if (!groundAhead || !groundBehind)
                {
                    return false;
                }

                //move on if the ground is suitable
                currentState = WormStates.heightChance;
                continue;

            case WormStates.heightChance:
                //add to current chance based on how close to min the ground is
                float progressChance = 0.02f;
                progressChance += (1 - ((groundY - firstPass.lowestY) / (firstPass.highestY - firstPass.lowestY)));

                //check against move stages function
                if (!MoveStages(progressChance))
                    return false;
                else
                    return true;
            }
        }

        return false;
    }
}