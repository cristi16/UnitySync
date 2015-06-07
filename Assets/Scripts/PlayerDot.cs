using UnityEngine;
using System.Collections;

public class PlayerDot : MonoBehaviour
{
    public float offsetFromNearPlane = 2f;
    public float scaleRatio = 0.5f;
    public float minDistanceToScaleFrom = 1f;

    PhotonView view;
    Transform cameraTransform;
    float offsetFromCamera;
    bool firstLoop = true;

    void Start()
    {
        view = GetComponentInChildren<PhotonView>();
        cameraTransform = Camera.main.transform;
        offsetFromCamera = Camera.main.nearClipPlane + offsetFromNearPlane;

        if (view.isMine)
        {
            view.RPC("Initialize", PhotonTargets.OthersBuffered, GameController.PlayerColor.r, GameController.PlayerColor.g, GameController.PlayerColor.b);
        }

        transform.localScale = Vector3.one * minDistanceToScaleFrom * scaleRatio;
        StartCoroutine(FakeUpdate());
    }


    [RPC]
    void Initialize(float r, float g, float b)
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
        GetComponent<Renderer>().material.color = new Color(r, g, b);
    }

    IEnumerator FakeUpdate()
    {
        yield return new WaitForSeconds(1f);

        while(true)
        {
            if (view.isMine)
            {
                transform.position = cameraTransform.position + cameraTransform.forward * offsetFromCamera;
            }
            else
            {
                var distance = Vector3.Distance(transform.position, cameraTransform.position);
                if(distance > minDistanceToScaleFrom)
                    transform.localScale =  Vector3.one * distance * scaleRatio;
            }
            yield return null;
        }
    }
}
