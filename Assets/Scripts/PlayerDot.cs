using UnityEngine;
using System.Collections;

public class PlayerDot : MonoBehaviour
{
    public float offsetFromNearPlane = 2f;
    public float scaleRatio = 0.5f;
    public float minDistanceToScaleFrom = 1f;
    public float distanceToGUISwap = 60f;
    public Transform playerUIPrefab;

    private RectTransform playerUIPanel;
    private RectTransform playerUI;
    private Camera uiCamera;
    private Color dotColor;
    PhotonView view;
    Transform cameraTransform;
    float offsetFromCamera;
    bool guiMode = false;

    void Start()
    {
        view = GetComponentInChildren<PhotonView>();
        cameraTransform = Camera.main.transform;
        offsetFromCamera = Camera.main.nearClipPlane + offsetFromNearPlane;

        if (view.isMine)
        {
            view.RPC("Initialize", PhotonTargets.OthersBuffered, GameController.PlayerColor.r, GameController.PlayerColor.g, GameController.PlayerColor.b);
        }
        else
        {
            uiCamera = UIController.instance.GetComponent<Camera>();
            playerUIPanel = GameObject.FindGameObjectWithTag("PlayerUIPanel").GetComponent<RectTransform>();
            playerUI = Instantiate(playerUIPrefab, Vector3.zero, Quaternion.identity) as RectTransform;
            playerUI.SetParent(playerUIPanel, false);
            playerUI.GetComponent<UnityEngine.UI.Image>().color = dotColor;
        }

        transform.localScale = Vector3.one * minDistanceToScaleFrom * scaleRatio;
        StartCoroutine(FakeUpdate());
    }


    [RPC]
    void Initialize(float r, float g, float b)
    {
        dotColor = new Color(r, g, b);

        gameObject.layer = LayerMask.NameToLayer("Default");
        foreach(Transform child in gameObject.transform)
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.material.color = dotColor;    
    }

    IEnumerator FakeUpdate()
    {
        yield return new WaitForSeconds(1f);

        while(true)
        {
            if (view.isMine)
            {
                transform.position = cameraTransform.position + cameraTransform.forward * offsetFromCamera;
                transform.rotation = cameraTransform.rotation;
            }
            else
            {
                if (GameController.IsPlayMode)
                {
                    yield return null;
                    SwapGfx(false);
                    continue;
                }

                var distance = Vector3.Distance(transform.position, cameraTransform.position);
                var screenPos = Camera.main.WorldToScreenPoint(transform.position);
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(playerUIPanel, new Vector2(screenPos.x, screenPos.y), uiCamera, out localPoint);

                //Debug.Log(screenPos);
                if(distance > distanceToGUISwap || screenPos.z < offsetFromCamera || playerUIPanel.rect.Contains(localPoint) == false)
                {
                    if (!guiMode)
                        SwapGfx(true);
                    if (screenPos.z < offsetFromCamera)
                    {
                        localPoint.x = localPoint.x > playerUIPanel.rect.width / 2f ? 0f : playerUIPanel.rect.width;
                        localPoint.y = playerUIPanel.rect.height - localPoint.y;
                    }

                    playerUI.anchoredPosition = new Vector2(Mathf.Clamp(localPoint.x, 0, playerUIPanel.rect.width),
                                                             Mathf.Clamp(localPoint.y, 0, playerUIPanel.rect.height));
                }
                else
                {
                    if (guiMode)
                        SwapGfx(false);

                    if(distance > minDistanceToScaleFrom)
                        transform.localScale =  Vector3.one * distance * scaleRatio;
                }
            }
            yield return null;
        }
    }

    private void SwapGfx(bool displayGUI)
    {
        if (guiMode == displayGUI)
            return;

        guiMode = displayGUI;
        playerUI.gameObject.SetActive(displayGUI);
        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
            rend.enabled = !displayGUI;
    }
}
