using UnityEngine;
using System.Collections;

public class HelpButton : MonoBehaviour
{

    public GameObject helpPanel;

    public void Activate()
    {
        helpPanel.SetActive(true);
    }

    public void Deactivate()
    {
        helpPanel.SetActive(false);
    }
}
