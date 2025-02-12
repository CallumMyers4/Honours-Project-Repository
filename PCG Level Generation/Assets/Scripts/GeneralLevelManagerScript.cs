using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralLevelManagerScript : MonoBehaviour
{
    public GameObject winPanel;
    public PlayerMovementScript player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.win)
        {
            winPanel.SetActive(true);
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
