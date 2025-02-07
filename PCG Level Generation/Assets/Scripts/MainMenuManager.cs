using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject controlsPanel;
    
    public void EnableControls()
    {
        controlsPanel.SetActive(true);
    }
}
