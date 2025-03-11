using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject controlsPanel, mainPanel;    //panel with controls, panel of main menu
    [SerializeField]
    private string mainLevel;   //name of the level which should be launched by default, uses most up-to-date pass
    
    //open controls menu
    public void EnableControls()
    {
        controlsPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    //close controls menu
    public void DisableControls()
    {
        controlsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    //start the game
    public void StartLevels()
    {
        SceneManager.LoadScene(mainLevel);
    }

    //quit the game
    public void ExitGame()
    {
        Application.Quit();
    }

    //open the survey (for user feedback)
    public void GoToSurvey()
    {
        string url = "https://forms.office.com/Pages/ResponsePage.aspx?id=uhrLqo_zDkGRU8FqAOv0zIoh24ArNJlPrAvDbJSdOlBUOFNDQU5CSFE1WVowUkY5WVo4TVYyTjZFRi4u";

        Application.OpenURL(url); // Opens the link in the browser
    }
}
