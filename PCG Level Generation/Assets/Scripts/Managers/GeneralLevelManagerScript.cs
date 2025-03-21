using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.ExceptionServices;
using System;

public class GeneralLevelManagerScript : MonoBehaviour
{
    public GameObject mainPanel, winPanel, parametersPanel, pausePanel;     //game UI, UI to show if player wins, parameter menu, pause menu
    public TMP_Text coinsCounterText;   //coins collected by player
    public PlayerMovementScript player;     //ref to player object
    public PerlinNoiseGeneratorScript perlin;   //perlin generator
    public TerrainPassOneScript firstPass;  //first pass script
    public TerrainPassTwoScript secondPass;     //second pass script
    public EnemyPassThreeScript thirdPass;  //third pass script
    public CollectablesPassFourScript collectables;     //fourth pass script

    public Slider lengthSlider, hillsSlider, gapsSlider, platformsSlider, enemiesSlider, coinsSlider;   //references to UI parameter sliders
    public float length, hills, gaps, platforms, enemies, coins;    //stores current value that player has entered on each slider

    public bool gamePaused = false; //stop game when paused

    [SerializeField]
    private int passes;     //keep track of how many passes are in current level

    void Awake()
    {
        //use saved values if possible otherwise use the slider's value
        hills = PlayerPrefs.GetFloat("hills", hillsSlider.value);
        length = PlayerPrefs.GetFloat("length", lengthSlider.value);
        gaps = PlayerPrefs.GetFloat("gaps", gapsSlider.value);
        platforms = PlayerPrefs.GetFloat("platforms", platformsSlider.value);
        enemies = PlayerPrefs.GetFloat("enemies", enemiesSlider.value);
        coins = PlayerPrefs.GetFloat("coins", coinsSlider.value);

        //apply values to generation scripts according to the level and how many scripts should run
        if (passes >= 0)
            perlin.scale = hills;
        if (passes >= 1)
            firstPass.endX = length;
        if (passes >= 2)
        {
            secondPass.gapsMultiplier = gaps;
            secondPass.platformsMultiplier = platforms;
        }
        if (passes >= 3)
            thirdPass.maxEnemies = (int)enemies;
        if (passes >= 4)
            collectables.maxLevelCoins = (int)coins;

        //set slider values (stay consistent when new levels are generated)
        lengthSlider.value = length;
        hillsSlider.value = hills;
        gapsSlider.value = gaps;
        platformsSlider.value = platforms;
        enemiesSlider.value = enemies;
        coinsSlider.value = coins;
    }

    void Update()
    {
        //if player reaches the win zone, change UI to win screen
        if (player.win)
        {
            mainPanel.SetActive(false);
            winPanel.SetActive(true);
        }
        else
        {
            if (mainPanel.activeInHierarchy)
               coinsCounterText.SetText(Convert.ToString(player.coinsCollected));
        }
    }

    public void GenerateLevel()
    {
        //save slider values before reloading the scene
        PlayerPrefs.SetFloat("length", lengthSlider.value);
        PlayerPrefs.SetFloat("hills", hillsSlider.value);
        PlayerPrefs.SetFloat("gaps", gapsSlider.value);
        PlayerPrefs.SetFloat("platforms", platformsSlider.value);
        PlayerPrefs.SetFloat("enemies", enemiesSlider.value);
        PlayerPrefs.SetFloat("coins", coinsSlider.value);
        PlayerPrefs.Save();

        //reload current scene with new parameters
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //generate a new level
    public void NewLevel()
    {
        winPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //open main menu
    public void Menu()
    {
        winPanel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    //pause game
    public void Pause()
    {
        gamePaused = true;
        mainPanel.SetActive(false);
        parametersPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    //back to level
    public void Unpause()
    {
        gamePaused = false;
        pausePanel.SetActive(false);
        parametersPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    //edit parameters
    public void ParameterMenu()
    {
        gamePaused = true;
        pausePanel.SetActive(false);
        parametersPanel.SetActive(true);
    }
}