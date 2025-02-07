using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject controlsPanel, mainPanel;
    [SerializeField]
    private string mainLevel;
 
    public void EnableControls()
    {
        controlsPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void DisableControls()
    {
        controlsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void StartLevels()
    {
        SceneManager.LoadScene(mainLevel);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
