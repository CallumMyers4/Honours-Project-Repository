using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TerrainPassTwoScript : MonoBehaviour
{
    [SerializeField]
    private TerrainPassOneScript firstPass;     //access previous pass
    private int positionsToCheckBack = 4, positionsToCheckForward = 5;  //ints to decide how far forward/back the chain will check to make
                                                                //its choice

    //number of gaps been placed in a row, gap length limit, position of last gap, blocks since this position, blocks at
    //the same Y as block being checked
    private int consecutiveGaps = 0, gapsLimit = 4, lastGapX = 0, blocksSinceGap = 0, blocksAtCurY;

    //number of blocks in current platform, platform length limit, platform level
    private int consecutivePlatformBlocks = 0, platformLengthLimit = 4; float platformY = 0, groundY = 0;
    
    //keep track of current state being checked for gaps
    private enum groundStates
    {
        limit,  //is the gap too long?
        xPositionQuota,  //how far along in the level is being checked compared to the quota
        surroundingYLevels,     //how much does the Y level change in the area?
        timeSinceLastGap,   //how long has it been since the last time a gap was made?
        destroyGround   //if this state is reached then return true to break ground at current pos
    }

    //keep track of current state being checked for platforms
    private enum platformStates
    {
        heightCheck,    //is it high enough for a platform?
        lengthCheck,    //is the platform still below the max length?
        groundBelow,    //change the chance based on whether there is ground or a gap below
        lengthChance,   //change the chance based on the current length of the platform
        placePlatform   //if all checks pass then make a platform
    }

    //----------------------------------------------NOT IN USE CURRENTLY----------------------------------------------------
    //I may use this section if i decide to make move chances vary for each stage (i.e. if i want some stages to be harder to pass than
    //others) but for now it just works as a cumulative percentage chance
    //keep track of chance of moving to other states
    private float[,] groundProbabilities = new float[5, 5]
    {
        {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},     //from limit to other states    
        {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},     //from xPos to other states
        {0.0f, 0.0f, 0.0f, 1.0f, 0.0f},     //from YLevels to other states
        {0.0f, 0.0f, 0.0f, 0.0f, 1.0f},     //from time between gaps to other states
        {0.0f, 0.0f, 0.0f, 0.0f, 0.0f}      //shouldnt move from destroy ground
    };

    //keep track of chance of moving to other states
    private float[,] platformProbabilities = new float[5, 5]
    {
        {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},     //from heightCheck to other states    
        {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},     //from lengthCheck to other states
        {0.0f, 0.0f, 0.0f, 1.0f, 0.0f},     //from groundBelow to other states
        {0.0f, 0.0f, 0.0f, 0.0f, 1.0f},     //from lengthChance between gaps to other states
        {0.0f, 0.0f, 0.0f, 0.0f, 0.0f}      //shouldnt move from placing
    };
    //-----------------------------------------------------------------------------------------------------------------------
    [SerializeField]
    public bool passTwoCompleted;   //tells next pass when to run


    // Update is called once per frame
    void Update()
    {
        if (firstPass.passOneCompleted == true && passTwoCompleted == false)
        {
            MakeGaps();
            //MakePlatforms();  //NOT READY YET
            passTwoCompleted = true;
        }
    }


    //create gaps in the ground
    private void MakeGaps()
    {
        groundStates currentState = groundStates.limit;
        bool checkComplete = false;

        //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX; i++)
        {
            currentState = groundStates.limit;  //reset state at start of each block
            checkComplete = false;
            float progressChance = 0.6f;    //initial chance of progressing to next stage (start at 1 to guarentee passing stage
                                            //1 so long as theres not too long a gap)
            //start of Markovs
            AdjustGroundValues(i);  //pass in the current X value being checked to allow the Markov to change values

            //uses an internal loop to get around C# requiring a continue statement and not just allowing a fall to the next state,
            //which was causing it to skip iterations of the block checking
            while (!checkComplete)
            {
                switch (currentState)
                {
                    case groundStates.limit:
                        if (consecutiveGaps >= gapsLimit)
                        {
                            blocksSinceGap++;
                            consecutiveGaps = 0;
                            progressChance = 0;
                        }
                        //if it fails to pass the stage check then end the chain and move on to the next loop,
                        //otherwise fall through to next stage
                        if (!MoveStages(currentState, progressChance))
                        {
                            blocksSinceGap++;
                            consecutiveGaps = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = groundStates.xPositionQuota;
                            continue;
                        }
                    
                    //-------------------NOT YET IMPLENENTED-------------------//
                    case groundStates.xPositionQuota:
                        currentState = groundStates.surroundingYLevels;
                        continue;
                    //---------------------------------------------------------//

                    case groundStates.surroundingYLevels:
                        //increase chance based on how flat ground is
                        for (int j = 0; j < blocksAtCurY; j++)
                        {
                            progressChance += 0.08f;
                        }
                        //this decreases the chance based on how many blocks are at a different Y
                        for (int j = 0; j < (positionsToCheckForward + positionsToCheckBack) - blocksAtCurY; j++)
                        {
                            progressChance -= 0.15f;
                        }
                        if (!MoveStages(currentState, progressChance))
                        {
                            blocksSinceGap++;
                            consecutiveGaps = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = groundStates.timeSinceLastGap;
                            continue;
                        }

                    case groundStates.timeSinceLastGap:
                        progressChance += blocksSinceGap * 0.01f;   //increase chance as gaps get further apart
                        progressChance += consecutiveGaps * 0.5f;    //increase chance if already a gap
                        if (!MoveStages(currentState, progressChance))
                        {
                            blocksSinceGap++;
                            consecutiveGaps = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = groundStates.destroyGround;
                            continue;
                        }

                    case groundStates.destroyGround:
                        /*this fires a raycast to find all objects at the current X position in the loop, starting from just above the highest
                        possible Y position set in firstPass, uses vector2.down to fire directly downwards, at a length of just over the
                        difference between highest and lowest Y to make sure it doesnt miss any objects, and returns all of these objects it
                        finds as a struct*/
                        RaycastHit2D[] objectsAtX = Physics2D.RaycastAll(new Vector2(i, firstPass.highestY + 1), Vector2.down, 
                                                                        (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));

                        //runs a loop to go through all objects stored in the struct to delete them
                        for (int k = 0; k < objectsAtX.Length; k++)
                        {
                            Destroy(objectsAtX[k].collider.gameObject);     //uses the collider component of each object found 
                                                                            //to access the gameObject and destroys it
                        }
                        consecutiveGaps++;  //add to the consecutive gap counter
                        blocksSinceGap = 0; //there is no blocks since last gap now
                        lastGapX = i;   //update last gap position to be here
                        checkComplete = true;
                        break;
                }
            }          
        }
    }


    //create platforms in the air
    private void MakePlatforms()
    {
        platformStates currentState = platformStates.lengthCheck;
        bool checkComplete = false;

        //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX; i++)
        {
            currentState = platformStates.lengthCheck;  //reset state at start of each block
            checkComplete = false;
            float progressChance = 0.6f;    //initial chance of progressing to next stage (start at 1 to guarentee passing stage
                                            //1 so long as theres not too long a gap)
            //start of Markovs
            AdjustGroundValues(i);  //pass in the current X value being checked to allow the Markov to change values

            //uses an internal loop to get around C# requiring a continue statement and not just allowing a fall to the next state,
            //which was causing it to skip iterations of the block checking
            while (!checkComplete)
            {
                switch (currentState)
                {
                    case platformStates.heightCheck:
                        //if starting a new platform find a new Y value to draw at by taking the Y value of the last ground block found
                        //and adding 3
                        if (consecutiveGaps == 0)
                        {
                            platformY = groundY + 3.0f;
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
                }
            }
        }
    }

    //decide if ground should be destroyed
    private bool GroundMarkov()
    {
        float chance = 0.5f;    //set a default chance

        //hard limit check, if it returns true dont make gap any bigger
        if (consecutiveGaps == gapsLimit)
        {
            blocksSinceGap++;
            consecutiveGaps = 0;
            return false;
        }

        //increase chance based on how flat ground is
        for (int i = 0; i < blocksAtCurY; i++)
        {
            chance += 0.02f;
        }

        //this decreases the chance based on how many blocks are at a different Y
        for (int i = 0; i < (positionsToCheckForward + positionsToCheckBack) - blocksAtCurY; i++)
        {
            chance -= 0.1f;
        }

        chance += blocksSinceGap * 0.05f;   //increase chance as gaps get further apart
        chance += consecutiveGaps * 0.1f;    //increase chance if already a gap

        //generate a random number between 1 and 0 to essentially turn the chance into a percentage
        float randChoice = UnityEngine.Random.Range(0.0f, 1.0f);
        Debug.Log($"Chance:  {chance}");
        Debug.Log($"Random:  {randChoice}");
        if (randChoice < chance)
        {
            //reset blocks since gap because this is one, then add 1 to consecutive gaps and tell main to destroy
            blocksSinceGap = 0;
            consecutiveGaps++;
            return true;
        }
        else
        {   
            //add 1 to blocks since the last gap, reset consecutive gaps and tell main to keep ground and move on
            blocksSinceGap++;
            consecutiveGaps = 0;
            return false;
        }
    }


    //set blocks since gap, blocks at Y, etc.
    private void AdjustGroundValues(int currentX)
    {     
        blocksAtCurY = 0;   //reset counter each time a new tile is being checked

        //fires a raycast to get the ground at current X position (using Raycast instead of RaycastAll because we only need the
        //first object hit since this will be the main ground)        
        RaycastHit2D currentGround = Physics2D.Raycast(new Vector2(currentX, firstPass.highestY + 1), Vector2.down, 
                                                                (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));

        //stored in the list of platform vars for now, was previously exclusive to this function but saves rewriting code to create
        //a second function to check Y levels for platform checks
        groundY = currentGround.collider.gameObject.transform.position.y;    //store the Y value of the ground

        float checkGroundY = 10000; //y pos of ground to be checked (start at high number so it does not accidentally add a +1 to
                                    //ground at different level when when there is no ground)

        //runs over all the positions looking backwards from current X
        for (int i = currentX; i > currentX - positionsToCheckBack; i--)
        {  
            //do the same as above but for the ground positions which are to be checked
            RaycastHit2D checkGround = Physics2D.Raycast(new Vector2(i, firstPass.highestY + 1), Vector2.down, 
                                                                    (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));

            //if the ground exists, compare Y otherwise end search because it must be a gap (+1 to different Y)
            if (checkGround.collider != null)                                                        
                checkGroundY = checkGround.collider.gameObject.transform.position.y;
            else
                continue;
            
            //if they are the same Y level then add 1 to the flat ground counter
            if (checkGroundY == groundY)
               blocksAtCurY++;
            else
                continue;   //if not then end the search since this is as far back as the flat ground goes
        }

        //runs over all the positions looking forwards from current X
        for (int i = currentX; i < currentX + positionsToCheckForward; i++)
        {  
            //do the same as above but for the ground positions which are to be checked
            RaycastHit2D checkGround = Physics2D.Raycast(new Vector2(i, firstPass.highestY + 1), Vector2.down, 
                                                                    (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));

            //if the ground exists, compare Y otherwise end search because it must be a gap (different Y)
            if (checkGround.collider != null)                                                        
                checkGroundY = checkGround.collider.gameObject.transform.position.y;
            else
                continue;
            
            //if they are the same Y level then add 1 to the flat ground counter
            if (checkGroundY == groundY)
               blocksAtCurY++;
            else
                continue;   //if not then end the search since this is as far back as the flat ground goes
        }

        blocksSinceGap = currentX - lastGapX;   //simple calculation to find the last gap
    }

    private void PlatformGroundChecks(int currentX)
    {
        //fires a raycast to get the ground at current X position (using Raycast instead of RaycastAll because we only need the
        //first object hit since this will be the main ground)        
        RaycastHit2D groundInDistance = Physics2D.Raycast(new Vector2(currentX, firstPass.highestY + 1), Vector2.down, 
                                                                (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));

        //groundY = currentGround.collider.gameObject.transform.position.y;    //store the Y value of the ground
    }
    private bool MoveStages(groundStates curState, float chance)
    {
        //get the chance of moving from the current stage to the next ((int)curState gets the current state's pos in enum)
        float moveChances = groundProbabilities[(int)curState, (int)curState + 1];
        Debug.Log("Move Chance:" + moveChances);
        //get a random float to compare current chance to
        float comparison = UnityEngine.Random.Range(0.0f, 1.0f);

        //decide whether or not to progress (false means the ground stays, true means move to next stage then decide)
        if (comparison < chance)
            return true;
        else
           return false;
    }

    private bool MoveStagesPlatform(platformStates curState, float chance)
    {
        //get the chance of moving from the current stage to the next ((int)curState gets the current state's pos in enum)
        float moveChances = platformProbabilities[(int)curState, (int)curState + 1];
        //get a random float to compare current chance to
        float comparison = UnityEngine.Random.Range(0.0f, 1.0f);

        //decide whether or not to progress (false means the ground stays, true means move to next stage then decide)
        if (comparison < chance)
            return true;
        else
           return false;
    }
}