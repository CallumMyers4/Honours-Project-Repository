using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GeneralLevelManagerScript : MonoBehaviour
{
    public GameObject mainPanel, winPanel;
    public TMP_Text coinsCounterText;
    public PlayerMovementScript player;
    public CollectablesPassFourScript collectables;

    public Slider lengthSlider, hillsSlider, gapsSlider, platformsSlider, enemiesSlider, coinsSlider;   //references to UI parameter sliders
    // Start is called before the first frame update
    void Start()
    {
        
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
        Debug.Log("Length: " + lengthSlider.value);
        Debug.Log("Hills: " + hillsSlider.value);
        Debug.Log("Gaps: " + gapsSlider.value);
        Debug.Log("Platforms: " + platformsSlider.value);
        Debug.Log("Enemies: " + enemiesSlider.value);
        Debug.Log("Coins: " + coinsSlider.value);
    }
}
