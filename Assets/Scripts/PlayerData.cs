using UnityEngine;
using System.Collections;

public class PlayerData
{
    public string playerName;
    public Color color;
    public GameObject playerInfoUI;

    public PlayerData(string playerName, Color color, GameObject playerInfoUI)
    {
        this.playerName = playerName;
        this.color = color;
        this.playerInfoUI = playerInfoUI;
    }

    public void OnDisconnect()
    {
        GameObject.Destroy(playerInfoUI);
    }
}
