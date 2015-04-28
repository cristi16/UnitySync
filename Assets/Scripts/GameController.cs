using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public List<Color> assignableColors;
    public RawImage colorImage;
    public Text nickname;
    public GameObject loginUI;
    public GameObject playerInfoUI;

    public static Color PlayerColor;

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings("0.1");     
    }

    public void AssingPlayerName(string name)
    {
        PhotonNetwork.player.name = name;
        var pickedColor = assignableColors[PhotonNetwork.playerList.Length];
        PlayerColor = pickedColor;
        playerInfoUI.SetActive(true);
        loginUI.SetActive(false);
        nickname.text += " " + name;
        colorImage.color = pickedColor;

        var dot = PhotonNetwork.Instantiate("PlayerDot", Vector3.zero, Quaternion.identity, 0);
        dot.transform.position = this.transform.position + transform.forward * (Camera.main.nearClipPlane + 2f);       
    }

    void OnJoinedLobby()
    {
        Debug.Log("JoinRandom");
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("New Player Connected: " + newPlayer.name);
    }

    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        AssingPlayerName(GameObject.FindGameObjectWithTag("PlayerName").GetComponent<UnityEngine.UI.Text>().text);
    }
}
