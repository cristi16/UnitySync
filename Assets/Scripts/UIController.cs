using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{

    private static UIController _instance;
    public static UIController instance
    {
        get
        {
            if (!_instance)
                _instance = FindObjectOfType<UIController>();
            return _instance;
        }
    }

    EventSystem eventSystem;

    void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public static bool OverUI()
    {
        return instance.eventSystem.IsPointerOverGameObject();
    }
}
