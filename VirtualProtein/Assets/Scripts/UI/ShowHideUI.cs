using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideUI : MonoBehaviour
{

    public static bool showUI = false;
    public GameObject UI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showUI)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }
    }

    void ShowUI()
    {
        UI.SetActive(true);
        showUI = true;
    }

    void HideUI()
    {
        UI.SetActive(false);
        showUI = false;
    }
}
