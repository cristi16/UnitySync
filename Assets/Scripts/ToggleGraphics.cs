using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Toggle))]
[RequireComponent(typeof(Image))]
public class ToggleGraphics : MonoBehaviour
{
    public GizmoControllerCS.GIZMO_MODE mode;
    public Sprite onGfxWindows;
    public Sprite offGfxWindows;
    public Sprite onGfxMac;
    public Sprite offGfxMac;
    private Toggle toggle;
    private Image image;
    private GizmoControllerCS gizmoController;
    private Sprite onGfx;
    private Sprite offGfx;

    void Start()
    {
#if UNITY_STANDALONE_OSX
        onGfx = onGfxMac;
        offGfx = offGfxMac;
#else
        onGfx = onGfxWindows;
        offGfx = offGfxWindows;
#endif

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
