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

    public void GoToSurvey()
    {
        string url = "https://forms.office.com/Pages/ResponsePage.aspx?id=uhrLqo_zDkGRU8FqAOv0zIoh24ArNJlPrAvDbJSdOlBUOFNDQU5CSFE1WVowUkY5WVo4TVYyTjZFRi4u";

        Application.OpenURL(url); // Opens the link in the browser
    }
}
