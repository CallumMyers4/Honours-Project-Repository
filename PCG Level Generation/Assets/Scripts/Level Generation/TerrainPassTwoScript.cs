using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using Microsoft.Unity.VisualStudio.Editor;
#endif
using UnityEditor;
using UnityEngine;

public class TerrainPassTwoScript : MonoBehaviour
{
    [SerializeField]
    private TerrainPassOneScript firstPass;     //access previous pass
    [SerializeField]
    private GameObject platformTile;    //the prefab used for making platforms
    private int positionsToCheckBack = 4, positionsToCheckForward = 5;  //ints to decide how far forward/back the chain will check to make its choice

    //number of gaps been placed in a row, gap length limit, position of last gap, blocks since this position, blocks at
    //the same Y as block being checked
    private int consecutiveGaps = 0, gapsLimit = 4, lastGapX = 0, blocksSinceGap = 0, blocksAtCurY;

    //number of blocks in current platform, platform length limit, min blocks before a gap, platform level, ground level at current X
    private int consecutivePlatformBlocks = 0, platformLengthLimit = 4, minGround = 3; float platformY = 0, groundY = 0;
    public float platformIncrease = 3.0f;   //how high above ground to place platform
    private bool groundBelow;       //checks if there is ground at the current pos being checked

    public float gapsMultiplier = 1.0f, platformsMultiplier = 1.0f;     //chance multiplier for the Markov, set by player in parameters menu
    
    //keep track of current state being checked for gaps
    private enum groundStates
    {
        groundLength,   //prevent 1 block long ground sections
        limit,  //is the gap too long?
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

    [SerializeField]
    public bool passTwoCompleted;   //tells next pass when to run
    private groundStates currentState = groundStates.groundLength;

    void Update()
    {
        //run when previous pass completes
        if (firstPass.passOneCompleted == true && passTwoCompleted == false)
        {
            MakeGaps();
            MakePlatforms();
            passTwoCompleted = true;
        }
    }

    //create gaps in the ground
    private void MakeGaps()
    {
        bool checkComplete = false;

        //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX - firstPass.endPlatformLength; i++)
        {
            currentState = groundStates.groundLength;  //reset state at start of each block
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
                    //makes sure there is always a min amount of ground before a block
                    case groundStates.groundLength:
                    //if the ground is too short, end loop, otherwise go to limit check
                    if (blocksSinceGap < minGround && consecutiveGaps == 0)
                    {
                            blocksSinceGap++;
                            consecutiveGaps = 0;
                            checkComplete = true;
                            break;
                    }
                    else
                    {
                            currentState = groundStates.limit;
                            continue;
                    }

                    //check how long current gap is
                    case groundStates.limit:
                        if (consecutiveGaps >= gapsLimit)
                        {
                            blocksSinceGap++;
                            consecutiveGaps = 0;
                            progressChance = 0;
                        }
                        //if it fails to pass the stage check then end the chain and move on to the next loop,
                        //otherwise fall through to next stage
                        if (!MoveStages(progressChance))
                        {
                            blocksSinceGap++;
                            consecutiveGaps = 0;
                            checkComplete = true;
                            break;
                        }
                        else
                        {
                            currentState = groundStates.surroundingYLevels;
                            continue;
                        }

                    //increase chance on flatter ground to break up monotonous levels
                    case groundStates.surroundingYLevels:
                        //increase chance based on how flat ground is
                        for (int j = 0; j < blocksAtCurY; j++)
                        {
                            progressChance += 0.08f;
                        }
                        //this decreases the chance based on how many blocks are at a different Y
                        for (int j = 0; j < (positionsToCheckForward + positionsToCheckBack) - blocksAtCurY; j++)
                        {
                            progressChance -= 0.1f;
                        }
                        if (!MoveStages(progressChance))
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

                    //edit chance based on how long its been since the last gap was made
                    case groundStates.timeSinceLastGap:
                        progressChance += blocksSinceGap * 0.05f;   //increase chance as gaps get further apart
                        progressChance += consecutiveGaps * 0.6f;    //increase chance if already a gap
                        if (!MoveStages(progressChance))
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

                        //runs a loop to go through all objects labelled as ground stored in the struct to delete them
                        for (int k = 0; k < objectsAtX.Length; k++)
                        {
                            if (objectsAtX[k].collider.gameObject.CompareTag("Ground"))
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
        for (int i = firstPass.startPlatformLength; i < firstPass.endX - firstPass.endPlatformLength; i++)
        {
            currentState = platformStates.heightCheck;  //reset state at start of each block
            checkComplete = false;
            float progressChance = 0.6f;    //initial chance of progressing to next stage
            //start of Markovs
            AdjustGroundValues(i);  //pass in the current X value being checked to allow the Markov to change values

            //uses an internal loop to get around C# requiring a continue statement and not just allowing a fall to the next state,
            //which was causing it to skip iterations of the block checking
            while (!checkComplete)
            {
                switch (currentState)
                {
                    //ensure platform is not too close to ground
                    case platformStates.heightCheck:
                        //if starting a new platform find a new Y value to draw at by taking the Y value of the last ground block found
                        //and adding 3
                        if (consecutivePlatformBlocks == 0)
                            platformY = groundY + platformIncrease;
                        else if (consecutivePlatformBlocks == 1)
                        {
                            currentState = platformStates.placePlatform;
                            break;
                        }
                        //otherwise run it as a check
                        else
                        {
                            //if too close
                            if (platformY < groundY + platformIncrease)
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
                        if (!MoveStagesPlatform(progressChance))
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

                    //check length of current platform in progress
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
                        if (!MoveStagesPlatform(progressChance))
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

                    //change chance based on what is below current pos
                    case platformStates.groundBelow:
                        //increases the chance if there is no ground found, or decreases if not over a gap
                        if (groundBelow)
                            progressChance -= 0.8f;
                        else
                            progressChance += 0.5f;
                        if (!MoveStagesPlatform(progressChance))
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

                    //decrease chance as platform gets longer
                    case platformStates.lengthChance:
                        //decreases chance by 1% for every platform block on current platform
                        progressChance -= 0.01f * consecutivePlatformBlocks;
                        if (!MoveStagesPlatform(progressChance))
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
            }
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

        if (!currentGround.collider.gameObject.CompareTag("Ground"))
        {
            currentGround = new RaycastHit2D(); //make hit null if it is not a ground object
        }

        //stored in the list of platform vars for now, was previously exclusive to this function but saves rewriting code to create
        //a second function to check Y levels for platform checks
        if (currentGround.collider != null)
        {
            groundY = currentGround.collider.gameObject.transform.position.y;    //store the Y value of the ground
            groundBelow = true;     //used in platforms to affect chance based on whether there is ground found below
        }
        else
        {
            groundBelow = false;
        }

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

    private bool MoveStages(float chance)
    {
        //get a random float to compare current chance to
        float comparison = UnityEngine.Random.Range(0.0f, 1.0f);

        //decide whether or not to progress (false means the ground stays, true means move to next stage then decide)
        //chance is multiplied by multiplier set by player, moving the slider down in params menu will make multiplier smaller, decreasing chance of passing,
        //and therefore less gaps
        if (comparison < (chance * gapsMultiplier))
            return true;
        else
        {
            return false;
        }
    }

    private bool MoveStagesPlatform(float chance)
    {
        //get a random float to compare current chance to
        float comparison = UnityEngine.Random.Range(0.0f, 1.0f);

        //decide whether or not to progress (false means the ground stays, true means move to next stage then decide)
        if (comparison < (chance * platformsMultiplier))
            return true;
        else
           return false;
    }
}