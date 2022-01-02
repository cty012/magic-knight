using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    private Button button;
    public string buttonTag;

    protected virtual void Awake()
    {
        this.button = this.GetComponent<Button>();
        if (this.button != null)
        {
            this.button.onClick.AddListener(() => { UIManager.instance.ButtonsOnClick(this.buttonTag, this.button); });
        }
    }
}
