using UnityEngine;
using System.Collections;
using System;

public class Logout : MonoBehaviour
{
    DateTime resetStartTime;
    bool waiting;

    public void Activate()
    {
        FindObjectOfType<GameController>().Disconnect();
        PhotonNetwork.Disconnect();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if(waiting && (DateTime.Now - resetStartTime).TotalHours > 1f)
        {
            waiting = false;
            Activate();
        }

        if(mouseX == 0 && mouseY == 0)
        {
            if(!waiting)
            {
                resetStartTime = DateTime.Now;
                waiting = true; 
            }
        }
        else
        {
            if(waiting)
            {
                waiting = false;
            }
        }
    }
}
