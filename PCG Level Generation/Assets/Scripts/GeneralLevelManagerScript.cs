using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.ExceptionServices;

public class GeneralLevelManagerScript : MonoBehaviour
{
    public GameObject mainPanel, winPanel, parametersPanel;
    public TMP_Text coinsCounterText;
    public PlayerMovementScript player;
    public PerlinNoiseGeneratorScript perlin;
    public TerrainPassOneScript firstPass;
    public TerrainPassTwoScript secondPass;
    public EnemyPassThreeScript thirdPass;
    public CollectablesPassFourScript collectables;

    public Slider lengthSlider, hillsSlider, gapsSlider, platformsSlider, enemiesSlider, coinsSlider;   //references to UI parameter sliders
    public float length, hills, gaps, platforms, enemies, coins;

    public static GeneralLevelManagerScript thisScript;
    public float perlinScale = 20.0f; // Default value

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
        else
        {
            Debug.Log("Perlin.scale: " + perlin.scale);
            coinsCounterText.SetText(player.coinsCollected.ToString() + "/" + collectables.totalCoins.ToString());
        }
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
}