using UnityEngine;
using System.Collections;

public class SelectNameFieldOnStart : MonoBehaviour
{
    void Start()
    {
        GetComponent<UnityEngine.UI.InputField>().Select();
    }

}
