using UnityEngine;
using System.Collections;

public class BoxSelectorCS : MonoBehaviour
{

    private Color NormalColor;
    private Color HilightColor;
    public bool OverBox = false;
    public GizmoControllerCS GC = null;

    private PhotonView photonView;


    void Start()
    {
        if (!GetComponent<Renderer>())
            return;

        NormalColor = GetComponent<Renderer>().material.color;
        HilightColor = new Color(NormalColor.r * 1.2f, NormalColor.g * 1.2f, NormalColor.b * 1.2f, 1.0f);
        GC = GameObject.Find("GizmoAdvancedCS").GetComponent<GizmoControllerCS>();

        if (GC == null)
        {
            Debug.LogError(this + " Unable To Find Gizmo Advanced Controller. Please be sure one is in the scene.");
        }
        else
        {
            GC.Hide();
        }

        this.photonView = GetComponent<PhotonView>();
    }//Start

    void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = HilightColor;
        OverBox = true;
    }//OnMouseEnter

    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = NormalColor;
        OverBox = false;
    }//OnMouseEnter

    void OnMouseDown()
    {
        if (GC == null)
            return;

        if (GC.IsOverAxis())
            return;

        if (photonView.isMine)
        {
            GrantOwnership();
            return;
        }

        if (photonView.ownerId == PhotonNetwork.player.ID)
        {
            Debug.Log("Not requesting ownership. Already mine.");
            return;
        }
        Debug.Log("Requesting ownership");
        photonView.RequestOwnership();


    }//OnMouseDown


    public void OnOwnershipRequest(object[] viewAndPlayer)
    {

        PhotonView view = viewAndPlayer[0] as PhotonView;
        PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;

        if (view.viewID != photonView.viewID) return;

        Debug.Log("OnOwnershipRequest(): Player " + requestingPlayer + " requests ownership of: " + view + ".");
        if (GC.SelectedObject != this.transform)
        {
            view.TransferOwnership(requestingPlayer.ID);
            view.RPC("GrantOwnership", requestingPlayer);
        }
        else
        {
            view.RPC("DenyOwnership", requestingPlayer);
        }
    }

    [RPC]
    void GrantOwnership()
    {
        Debug.Log("Granted ownership");
        GC.SetSelectedObject(transform);

        if (GC.IsHidden())
            GC.Show(GizmoControllerCS.GIZMO_MODE.TRANSLATE);
    }

    [RPC]
    void DenyOwnership()
    {
        Debug.Log("Ownership transfer denied");
    }
}
