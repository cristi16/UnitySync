using UnityEngine;
using System.Collections;

public class SelectNameFieldOnStart : MonoBehaviour
{
    void Start()
    {
        GetComponent<UnityEngine.UI.InputField>().Select();
        GameObject goText = transform.FindChild("PlayerName").gameObject;
        UnityEngine.UI.Text txt = goText.GetComponent<UnityEngine.UI.Text>();
        txt.rectTransform.pivot = new Vector2(0, 1.2f);
    }

}
