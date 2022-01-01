using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager instance { get; private set; }

    private Manager() { Manager.instance = this; }

    private void Start()
    {
        // Self
        Object.DontDestroyOnLoad(this.gameObject);
        // Settings
        Settings.instance.LoadSettings();
        // DataManager
        DataManager.instance.LoadGlobalSave();
        // Menumanager
        MenuManager.instance.SwitchMenu(Menu.MAIN_MENU);
    }
}
