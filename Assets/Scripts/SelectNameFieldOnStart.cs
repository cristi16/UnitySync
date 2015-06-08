using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SelectNameFieldOnStart : MonoBehaviour
{
    InputField inputField;

    void Start()
    {
        inputField = GetComponent<InputField>();
        inputField.Select();
        //GameObject goText = transform.FindChild("PlayerName").gameObject;
        //UnityEngine.UI.Text txt = goText.GetComponent<UnityEngine.UI.Text>();
        //txt.rectTransform.pivot = new Vector2(0, 1.2f);
    }

    void Update()
    {
        if (inputField.text != "" && Input.GetKeyDown(KeyCode.Return))
            FindObjectOfType<GameController>().Connect();
    }

}
