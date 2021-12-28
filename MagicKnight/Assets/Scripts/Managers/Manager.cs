using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    void Awake()
    {
        // Self
        Object.DontDestroyOnLoad(this.gameObject);
        // Settings
        Settings.instance.LoadSettings();
        // DataManager TODO testing
        DataManager.instance.save.CreateBasicSave();
    }
}
