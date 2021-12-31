using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    private Button button;
    public string buttonTag;

    private void Awake()
    {
        this.button = this.GetComponent<Button>();
        if (this.button != null)
        {
            this.button.onClick.AddListener(() => { MenuManager.instance.ButtonsOnClick(this.buttonTag, this.button); });
        }
    }
}
