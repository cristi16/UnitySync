using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Toggle))]
[RequireComponent(typeof(Image))]
public class ToggleGraphics : MonoBehaviour
{
    public Sprite onGfx;
    public Sprite offGfx;
    private Toggle toggle;
    private Image image;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();
        image.sprite = toggle.isOn ? onGfx : offGfx;
    }

    public void UpdateGfx()
    {
        image.sprite = toggle.isOn ? onGfx : offGfx;
    }
    
}
