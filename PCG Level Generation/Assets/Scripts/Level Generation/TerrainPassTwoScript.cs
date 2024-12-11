using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPassTwoScript : MonoBehaviour
{
    [SerializeField]
    private TerrainPassOneScript firstPass;     //access previous pass
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
        }
    }

    //create platforms in the air
    private void MakePlatforms()
    {

    }
}
