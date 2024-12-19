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
    private int consecutiveGaps = 0, lastGapX = 0, blocksSinceGap = 0, blocksAtCurY;
    
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
        //run through each position in the level (starting at the end of the initial platform, ending at the end of the level)
        for (int i = firstPass.startPlatformLength; i < firstPass.endX; i++)
        {
            //temp - use Markov Chains later
            int randChoice = UnityEngine.Random.Range(0, 3);
            if (randChoice == 1)
            {
                /*this fires a raycast to find all objects at the curernt X position in the loop, starting from just above the highest
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
            }


            //start of Markovs
            AdjustGroundValues(i);  //pass in the current X value being checked to allow the Markov to change values

            if (GroundMarkov() == true)
            {
                /*this fires a raycast to find all objects at the curernt X position in the loop, starting from just above the highest
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

                lastGapX = i;   //set the X position of the last gap to be here
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
        return false;
    }


    //set blocks since gap, blocks at Y, etc.
    private void AdjustGroundValues(int currentX)
    {        
        //fires a raycast to get the ground at current X position (using Raycast instead of RaycastAll because we only need the
        //first object hit since this will be the main ground)        
        RaycastHit2D currentGround = Physics2D.Raycast(new Vector2(currentX, firstPass.highestY + 1), Vector2.down, 
                                                                (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));

        //store the Y value of the ground
        float currentY = currentGround.collider.gameObject.transform.position.y;

        //runs over all the positions looking backwards from current X
        for (int i = currentX; i > currentX - positionsToCheckBack; i--)
        {  
            //do the same as above but for the ground positions which are to be checked
            RaycastHit2D checkGround = Physics2D.Raycast(new Vector2(currentX, firstPass.highestY + 1), Vector2.down, 
                                                                    (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));
            float checkGroundY = checkGround.collider.gameObject.transform.position.y;

            //if they are the same Y level then add 1 to the flat ground counter
            if (checkGroundY == currentY)
               blocksAtCurY++;
            else
                continue;   //if not then end the search since this is as far back as the flat ground goes
        }

        //runs over all the positions looking forwards from current X
        for (int i = currentX; i < currentX + positionsToCheckForward; i++)
        {  
            //again exactly same as looking backwards just moving in the other direction
            RaycastHit2D checkGround = Physics2D.Raycast(new Vector2(currentX, firstPass.highestY + 1), Vector2.down, 
                                                                    (MathF.Abs(firstPass.highestY - firstPass.lowestY) + 2));
            float checkGroundY = checkGround.collider.gameObject.transform.position.y;

            if (checkGroundY == currentY)
               blocksAtCurY++;
            else
                continue;
        }


        blocksSinceGap = currentX - lastGapX;   //simple calculation to find the last gap
    }
}