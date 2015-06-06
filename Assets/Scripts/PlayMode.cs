using UnityEngine;
using System.Collections;

public class PlayMode : MonoBehaviour
{
    public GameObject firstPersonController;

    private GameObject mainCamera;
    private GizmoControllerCS gizmoController;
    public static bool isPlayMode = false;
    private UnityEngine.UI.Text buttonText;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        buttonText = GetComponentInChildren<UnityEngine.UI.Text>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        gizmoController = GameObject.FindGameObjectWithTag("Gizmo").GetComponent<GizmoControllerCS>();
        initialPosition = firstPersonController.transform.position;
        initialRotation = firstPersonController.transform.rotation;
        firstPersonController.SetActive(false);
        buttonText.text = isPlayMode ? "Stop" : "Play";
    }

    public void TogglePlayMode()
    {
        isPlayMode = !isPlayMode;

        buttonText.text = isPlayMode ? "Stop" : "Play";
        mainCamera.SetActive(!isPlayMode);
        firstPersonController.SetActive(isPlayMode);

        GameController.IsPlayMode = isPlayMode;

        if(isPlayMode)
        {
            gizmoController.Hide();
        }
        else
        {
            firstPersonController.transform.position = initialPosition;
            firstPersonController.transform.rotation = initialRotation;
        }
    }
}
