using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControlsScript : MonoBehaviour
{
    private GeneralLevelManagerScript levelManager;
    
    void Start()
    {
        levelManager = FindObjectOfType<GeneralLevelManagerScript>();
    }

    void Update()
    {
        //reload current level if F5 pressed, otherwise if player presses a button between 1 and 4, they can go to the level specific to that pass
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("PassOneOnlyScene");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("PassOneTwoScene");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene("PassOneTwoThreeScene");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadScene("PassOneTwoThreeFourScene");
        }

        //pause or unpause when player presses escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //if game is currently paused, close menu then go back to level, else pause game and open menu
            if (levelManager.gamePaused)
                levelManager.Unpause();
            else
                levelManager.Pause();
        }
    }
}
