using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.ExceptionServices;

public class GeneralLevelManagerScript : MonoBehaviour
{
    public GameObject mainPanel, winPanel, parametersPanel, pausePanel;
    public TMP_Text coinsCounterText;
    public PlayerMovementScript player;
    public PerlinNoiseGeneratorScript perlin;
    public TerrainPassOneScript firstPass;
    public TerrainPassTwoScript secondPass;
    public EnemyPassThreeScript thirdPass;
    public CollectablesPassFourScript collectables;

    public Slider lengthSlider, hillsSlider, gapsSlider, platformsSlider, enemiesSlider, coinsSlider;   //references to UI parameter sliders
    public float length, hills, gaps, platforms, enemies, coins;

    public bool gamePaused = false; //stop game when paused

    public static GeneralLevelManagerScript thisScript;

    void Awake()
    {
        //use saved values if possible otherwise use the slider's value
        hills = PlayerPrefs.GetFloat("hills", hillsSlider.value);
        length = PlayerPrefs.GetFloat("length", lengthSlider.value);
        gaps = PlayerPrefs.GetFloat("gaps", gapsSlider.value);
        platforms = PlayerPrefs.GetFloat("platforms", platformsSlider.value);
        enemies = PlayerPrefs.GetFloat("enemies", enemiesSlider.value);
        coins = PlayerPrefs.GetFloat("coins", coinsSlider.value);

        //apply values to generation scripts
        perlin.scale = hills;
        firstPass.endX = length;
        secondPass.gapsMultiplier = gaps;
        secondPass.platformsMultiplier = platforms;
        thirdPass.maxEnemies = (int)enemies;
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
        if (player.win)
        {
            mainPanel.SetActive(false);
            winPanel.SetActive(true);
        }
    }

    public void GenerateLevel()
    {
        // Save slider values before reloading the scene
        PlayerPrefs.SetFloat("length", lengthSlider.value);
        PlayerPrefs.SetFloat("hills", hillsSlider.value);
        PlayerPrefs.SetFloat("gaps", gapsSlider.value);
        PlayerPrefs.SetFloat("platforms", platformsSlider.value);
        PlayerPrefs.SetFloat("enemies", enemiesSlider.value);
        PlayerPrefs.SetFloat("coins", coinsSlider.value);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NewLevel()
    {
        winPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Menu()
    {
        winPanel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void Pause()
    {
        gamePaused = true;
        mainPanel.SetActive(false);
        parametersPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void Unpause()
    {
        gamePaused = false;
        pausePanel.SetActive(false);
        parametersPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void ParameterMenu()
    {
        gamePaused = true;
        pausePanel.SetActive(false);
        parametersPanel.SetActive(true);
    }
}