using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContinueButtonController : MonoBehaviour
{
    public int slotNumber { get; private set; }

    private void Awake()
    {
        this.GetComponent<Button>().interactable = DataManager.instance.globalSave.Get<int>("slot", out int slotNumber);
        this.slotNumber = slotNumber;
    }
}
