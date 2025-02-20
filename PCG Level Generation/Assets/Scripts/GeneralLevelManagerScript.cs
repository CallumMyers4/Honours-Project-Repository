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
            length = lengthSlider.value;
            hills = hillsSlider.value;
            gaps = gapsSlider.value;
            platforms = platformsSlider.value;  
            enemies = enemiesSlider.value;
            coins = coinsSlider.value;
    }

    // Update is called once per frame
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
        length = lengthSlider.value;
        hills = hillsSlider.value;
        gaps = gapsSlider.value;
        platforms = platformsSlider.value;  
        enemies = enemiesSlider.value;
        coins = coinsSlider.value;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        lengthSlider.value = length;
        hillsSlider.value = hills;
        gapsSlider.value = gaps;
        platformsSlider.value = platforms;  
        enemiesSlider.value = enemies;
        coinsSlider.value = coins;
    }
}
