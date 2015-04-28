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

    private Dictionary<int, int> takenColorIndices;

    private PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
        takenColorIndices = new Dictionary<int, int>();
    }

    public void Connect()
    {
        PhotonNetwork.player.name = GameObject.FindGameObjectWithTag("PlayerName").GetComponent<UnityEngine.UI.Text>().text;
        PhotonNetwork.ConnectUsingSettings("0.1");     
    }

    public IEnumerator Initialize()
    {
        while (takenColorIndices.Count != PhotonNetwork.playerList.Length - 1)
            yield return null;

        var pickedColor = Color.white;
        for(int i = 0; i < assignableColors.Count; i++)
        {
            if (takenColorIndices.ContainsValue(i))
                continue;
            else
            {
                pickedColor = assignableColors[i];
                view.RPC("MarkAssignedColor", PhotonTargets.AllBuffered, i, PhotonNetwork.player.ID);
                break;
            }
        }
        assignableColors.RemoveAt(0);
        PlayerColor = pickedColor;
        playerInfoUI.SetActive(true);
        loginUI.SetActive(false);
        nickname.text += " " + PhotonNetwork.player.name;
        colorImage.color = pickedColor;

        var dot = PhotonNetwork.Instantiate("PlayerDot", Vector3.zero, Quaternion.identity, 0);
        dot.transform.position = this.transform.position + transform.forward * (Camera.main.nearClipPlane + 2f);       
    }

    [RPC]
    void MarkAssignedColor(int index, int playerID)
    {
        Debug.Log("marking color index: " + index + " for player: " + playerID);
        takenColorIndices.Add(playerID, index);
    }

    void OnJoinedLobby()
    {
        Debug.Log("JoinRandom");
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("New Player Connected: " + newPlayer.name + "[" + newPlayer.ID + "]");
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
    {
        takenColorIndices.Remove(oldPlayer.ID);
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
