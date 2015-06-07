using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public List<Color> assignableColors;
    public Transform otherPlayersGroup;
    public GameObject playerInfoUI;
    public GameObject loginUI;
    public GameObject teamUI;
    public Transform overlayCube;
    public static Color PlayerColor;
    public static bool IsPlayMode;
    public static bool IsConnected = false;

    private Dictionary<int, int> takenColorIndices;
    private Dictionary<int, PlayerData> playersData;
    private PhotonView view;
    private bool joinedLobby = false;

    void Start()
    {
        view = GetComponent<PhotonView>();
        takenColorIndices = new Dictionary<int, int>();
        playersData = new Dictionary<int, PlayerData>();
        GameObject.FindGameObjectWithTag("FPS").SetActive(false);
        InitialConnect();
    }

    public void InitialConnect()
    {
        PhotonNetwork.ConnectUsingSettings("0.1");
    }

    public IEnumerator Initialize()
    {
        while (takenColorIndices.Count != PhotonNetwork.playerList.Length - 1)
            yield return null;

        teamUI.SetActive(true);
        loginUI.SetActive(false);
        IsConnected = true;

        var pickedColor = Color.white;
        for(int i = 0; i < assignableColors.Count; i++)
        {
            if (takenColorIndices.ContainsValue(i))
                continue;
            else
            {
                pickedColor = assignableColors[i];
                view.RPC("MarkAssignedColor", PhotonTargets.AllBuffered, i, PhotonNetwork.player.ID, PhotonNetwork.player.name);
                break;
            }
        }
        PlayerColor = pickedColor;

        var position = this.transform.position + transform.forward * (Camera.main.nearClipPlane + 2f);       
        var dot = PhotonNetwork.Instantiate("PlayerDot", position, Quaternion.identity, 0);
    }

    [RPC]
    void MarkAssignedColor(int index, int playerID, string playerName)
    {
        Debug.Log("marking color index: " + index + " for player: " + playerID);
        takenColorIndices.Add(playerID, index);

        var playerInfo = GameObject.Instantiate<GameObject>(playerInfoUI);
        playerInfo.transform.SetParent(otherPlayersGroup, false);
        playerInfo.GetComponent<Text>().text = "        " + playerName;
        playerInfo.GetComponent<Text>().color = assignableColors[index];
        var colorImage = playerInfo.transform.GetChild(0).GetComponent<RawImage>();
        colorImage.color = assignableColors[index];

        var playerData = new PlayerData(playerName, assignableColors[index], playerInfo);
        playersData.Add(playerID, playerData);
    }

    void OnJoinedLobby()
    {
        joinedLobby = true;
    }

    public void Connect()
    {
        Debug.Log("JoinRandom");
        PhotonNetwork.player.name = GameObject.FindGameObjectWithTag("PlayerName").GetComponent<UnityEngine.UI.Text>().text;
        var connectText = GameObject.FindGameObjectWithTag("ConnectButton").GetComponentInChildren<Text>();
        connectText.fontSize = 16;
        connectText.text = "Joining co-op group..";

        StartCoroutine(ConnectToRoom());
    }

    IEnumerator ConnectToRoom()
    {
        while (!joinedLobby)
            yield return null;
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("New Player Connected: " + newPlayer.name + "[" + newPlayer.ID + "]");
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
    {
        takenColorIndices.Remove(oldPlayer.ID);
        playersData[oldPlayer.ID].OnDisconnect();
        playersData.Remove(oldPlayer.ID);

        foreach (var interactionObject in FindObjectsOfType<InteractableObject>())
            interactionObject.DoClearSelection();

        Debug.Log("New Player Disconnected: " + oldPlayer.name + "[" + oldPlayer.ID + "]");
    }

    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        StartCoroutine(Initialize());
    }
}
