using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBarController : MonoBehaviour
{
    private Text text;
    private GameObject backgorund;
    private GameObject fill;

    public bool dynamicLength;
    public float scale;

    private void Awake()
    {
        this.text = this.transform.Find("Text").GetComponent<Text>();
        this.backgorund = this.transform.Find("Bar").Find("Background").gameObject;
        this.fill = this.transform.Find("Bar").Find("Fill").gameObject;

        if (!dynamicLength) this.scale = ((RectTransform)this.backgorund.transform).rect.width / 100;
    }

    public void SetMaxValue(float maxValue)
    {
        RectTransform rt = (RectTransform)this.backgorund.transform;
        rt.sizeDelta = new Vector2(maxValue * scale, rt.sizeDelta.y);
    }

    public void SetValue(float value)
    {
        RectTransform rt = (RectTransform)this.fill.transform;
        rt.sizeDelta = new Vector2(value * scale, rt.sizeDelta.y);
    }
}
