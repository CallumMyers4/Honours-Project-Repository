using System;
using System.Collections;
using System.Collections.Generic;
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
    
    //keep track of current state being checked
    private enum groundStates
    {
        limit,  //is the gap too long?
        xPositionQuota,  //how far along in the level is being checked compared to the quota
        surroundingYLevels,     //how much does the Y level change in the area?
        timeSinceLastGap,   //how long has it been since the last time a gap was made?
        destroyGround   //if this state is reached then return true to break ground at current pos
    }

    //keep track of chance of moving to other states
    private float[,] probabilities = new float[5, 5]
    {
        {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},     //from limit to other states    
        {0.0f, 0.0f, 0.0f, 0.0f, 0.0f},     //from xPos to other states
        {0.0f, 0.0f, 0.0f, 0.0f, 0.0f},     //from YLevels to other states
        {0.0f, 0.0f, 0.0f, 0.0f, 0.0f},     //from time between gaps to other states
        {0.0f, 0.0f, 0.0f, 0.0f, 0.0f}      //shouldnt move from destroy ground
    };

    [SerializeField]
    public bool passTwoCompleted;   //tells next pass when to run


    // Update is called once per frame
    void Update()
    {
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
        groundStates currentState = groundStates.limit;

        //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX; i++)
        {
            float progressChance = 0.5f;    //initial chance of progressing to next stage

            //start of Markovs
            AdjustGroundValues(i);  //pass in the current X value being checked to allow the Markov to change values

            switch (currentState)
            {
                case groundStates.limit:
                    if (consecutiveGaps == gapsLimit)
                    {
                        blocksSinceGap++;
                        consecutiveGaps = 0;
                        progressChance = 0;
                    }
                    //if it fails to pass the stage check then end the chain and move on to the next loop,
                    //otherwise fall through to next stage
                    if (!MoveStages(currentState, progressChance))
                        break;
                    else
                    {
                        currentState = groundStates.xPositionQuota;
                        continue;
                    }
                case groundStates.xPositionQuota:
                    break;
                case groundStates.surroundingYLevels:
                    break;
                case groundStates.timeSinceLastGap:
                    break;
                case groundStates.destroyGround:
                    /*this fires a raycast to find all objects at the current X position in the loop, starting from just above the highest
                    possible Y position set in firstPass, uses vector2.down to fire directly downwards, at a length of just over the
                    difference between highest and lowest Y to make sure it doesnt miss any objects, and returns all of these objects it
                    finds as a struct*/
                    RaycastHit2D[] objectsAtX = Physics2D.RaycastAll(new Vector2(i, firstPass.highestY + 1), Vector2.down, 
                                                                    (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));

                    //runs a loop to go through all objects stored in the struct to delete them
                    for (int j = 0; j < objectsAtX.Length; j++)
                    {
                        Destroy(objectsAtX[j].collider.gameObject);     //uses the collider component of each object found 
                                                                        //to access the gameObject and destroys it
                    }

                    lastGapX = i;   //update last gap position to be here
                    break;
            }
        }
    }


    //create platforms in the air
    private void MakePlatforms()
    {

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

        //this decreases the chance based on how many blocks are at a different Y - may remove later unsure as of now
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

        float currentY = currentGround.collider.gameObject.transform.position.y;    //store the Y value of the ground

        float checkGroundY = 10000; //y pos of ground to be checked (start at high number so it does not accidentally add a +1 when there
                                    //is no ground)

        //runs over all the positions looking backwards from current X
        for (int i = currentX; i > currentX - positionsToCheckBack; i--)
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
            if (checkGroundY == currentY)
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
            if (checkGroundY == currentY)
               blocksAtCurY++;
            else
                continue;   //if not then end the search since this is as far back as the flat ground goes
        }

        blocksSinceGap = currentX - lastGapX;   //simple calculation to find the last gap
    }

    private bool MoveStages(groundStates curState, float chance)
    {
        //get the chance of moving from the current stage to the next ((int)curState gets the current state's pos in enum)
        float moveChances = probabilities[(int)curState, (int)curState + 1];

        //get a random float to compare current chance to
        float comparison = UnityEngine.Random.Range(0.0f, 1.0f);

        //decide whether or not to progress (false means the ground stays, true means move to next stage then decide)
        if (comparison < chance)
            return true;
        else
           return false;
    }
}