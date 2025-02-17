using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralLevelManagerScript : MonoBehaviour
{
    public GameObject mainPanel, winPanel;
    public TMP_Text coinsCounterText;
    public PlayerMovementScript player;
    public CollectablesPassFourScript collectables;
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
}
