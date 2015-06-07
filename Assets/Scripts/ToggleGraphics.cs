using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Toggle))]
[RequireComponent(typeof(Image))]
public class ToggleGraphics : MonoBehaviour
{
    public GizmoControllerCS.GIZMO_MODE mode;
    public Sprite onGfx;
    public Sprite offGfx;
    private Toggle toggle;
    private Image image;
    private GizmoControllerCS gizmoController;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();
        image.sprite = toggle.isOn ? onGfx : offGfx;
        gizmoController = FindObjectOfType<GizmoControllerCS>();
    }

    public void UpdateGfx()
    {
        image.sprite = toggle.isOn ? onGfx : offGfx;
        if(toggle.isOn)
        {
            gizmoController.SetMode(mode);
        }
    }
    
}
