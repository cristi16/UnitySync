using UnityEngine;
using System.Collections;


[RequireComponent(typeof(BoundBoxes_BoundBox))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [HideInInspector]
    public bool IsBeingHovered = false;
    private GizmoControllerCS GizmoController = null;
    private GameController gameController;

    public bool IsDuplicate {get; set;}
    private PhotonView photonView;
    private BoundBoxes_BoundBox boundingBox;

    void Start()
    {
        GizmoController = GameObject.FindGameObjectWithTag("Gizmo").GetComponent<GizmoControllerCS>();
        gameController = FindObjectOfType<GameController>();

        if (GizmoController == null)
        {
            Debug.LogError(this + " Unable To Find Gizmo Advanced Controller. Please be sure one is in the scene.");
        }

        this.photonView = GetComponent<PhotonView>();
        if (this.IsDuplicate)
            OnMouseDown();
        boundingBox = GetComponent<BoundBoxes_BoundBox>();
        ClearSelection();
    }

    void OnMouseEnter()
    {
        if (GameController.IsPlayMode) return;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            var oldColor = renderer.material.color;
            renderer.material.color = new Color(oldColor.r * 1.2f, oldColor.g * 1.2f, oldColor.b * 1.2f, oldColor.a); ;
        }
        IsBeingHovered = true;
    }

    void OnMouseExit()
    {
        if (GameController.IsPlayMode) return;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            var oldColor = renderer.material.color;
            renderer.material.color = new Color(oldColor.r / 1.2f, oldColor.g / 1.2f, oldColor.b / 1.2f, oldColor.a); ;
        }
        IsBeingHovered = false;
    }

    void OnMouseDown()
    {
        if (GameController.IsPlayMode) return;

        if (GizmoController == null)
            return;

        if (GizmoController.IsOverAxis())
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
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
        {
            if(GizmoController.SelectedObject == this.transform)
            {
                InteractableObject clone = PhotonNetwork.Instantiate(gameObject.name.Replace("(Clone)", "").Trim(), transform.position, transform.rotation, 0).GetComponent<InteractableObject>();
                clone.transform.localScale = transform.localScale;
                clone.IsDuplicate = true;
            }
        }
    }


    public void OnOwnershipRequest(object[] viewAndPlayer)
    {
        PhotonView view = viewAndPlayer[0] as PhotonView;
        PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;

        if (view.viewID != photonView.viewID) return;

        Debug.Log("OnOwnershipRequest(): Player " + requestingPlayer + " requests ownership of: " + view + ".");
        if (GizmoController.SelectedObject != this.transform)
        {
            view.TransferOwnership(requestingPlayer.ID);
            view.RPC("GrantOwnership", requestingPlayer);
        }
        else
        {
            view.RPC("DenyOwnership", requestingPlayer);
        }
    }

    public void DoClearSelection()
    {
        photonView.RPC("ClearSelection", PhotonTargets.OthersBuffered);
    }

    [RPC]
    void GrantOwnership()
    {
        if(transform != GizmoController.SelectedObject)
        {
            if(GizmoController.SelectedObject != null)
                GizmoController.SelectedObject.GetComponent<InteractableObject>().DoClearSelection();
        }

        Debug.Log("Granted ownership");
        GizmoController.SetSelectedObject(transform);

        if (GizmoController.IsHidden())
            GizmoController.Show(GizmoControllerCS.GIZMO_MODE.TRANSLATE);

        photonView.RPC("MarkSelection", PhotonTargets.OthersBuffered, GameController.PlayerColor.r, GameController.PlayerColor.g, GameController.PlayerColor.b);
    }

    [RPC]
    void DenyOwnership()
    {
        Debug.Log("Ownership transfer denied");
    }

    [RPC]
    void MarkSelection(float r, float g, float b)
    {
        boundingBox.enabled = true;
        boundingBox.lineColor = new Color(r, g, b, 0.7f);
    }

    [RPC]
    void ClearSelection()
    {
        boundingBox.enabled = false;
    }
}
