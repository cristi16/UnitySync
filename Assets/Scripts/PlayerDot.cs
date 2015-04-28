using UnityEngine;
using System.Collections;

public class PlayerDot : MonoBehaviour
{

    void Start()
    {
        var view = GetComponentInChildren<PhotonView>();
        if (view.isMine)
        {
            view.RPC("Initialize", PhotonTargets.OthersBuffered, GameController.PlayerColor.r, GameController.PlayerColor.g, GameController.PlayerColor.b);
        }
    }


    [RPC]
    void Initialize(float r, float g, float b)
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
        GetComponent<Renderer>().material.color = new Color(r, g, b);
    }
}
