using UnityEngine;
using System.Collections;

public class PlayerDot : MonoBehaviour
{
    public float offsetFromNearPlane = 2f;
    public float scaleRatio = 0.5f;

    PhotonView view;
    Transform cameraTransform;
    float offsetFromCamera;

    void Start()
    {
        view = GetComponentInChildren<PhotonView>();
        cameraTransform = Camera.main.transform;
        offsetFromCamera = Camera.main.nearClipPlane + offsetFromNearPlane;

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

    void Update()
    {
        if (view.isMine)
        {
            transform.position = cameraTransform.position + cameraTransform.forward * offsetFromCamera;
        }
        //else
           // transform.localScale = Vector3.Distance(transform.position, cameraTransform.position) * Vector3.one * scaleRatio;
    }
}
