using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotButtonController : MenuButtonController
{
    public int slotNumber;
    private Text text;

    protected override void Awake()
    {
        base.Awake();
        this.text = this.transform.Find("Text").GetComponent<Text>();
        this.Refresh();
    }

    private void Refresh()
    {
        this.text.text = DataManager.instance.SlotExists(this.slotNumber) ? "Used Slot" : "New SLot";
    }
}
