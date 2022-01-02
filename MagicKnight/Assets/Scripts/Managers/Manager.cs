using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager instance { get; private set; }

    private Manager() { if (Manager.instance == null) Manager.instance = this; }

    private void Awake()
    {
        if (Manager.instance != this) Object.Destroy(this);
    }

    private void Start()
    {
        // Self
        Object.DontDestroyOnLoad(this.gameObject);
        // Settings
        Settings.instance.LoadSettings();
        // DataManager
        DataManager.instance.LoadGlobalSave();
        DataManager.instance.temp["items", true].LoadFromDisk("Assets/Setup/Items.json");
        // Menumanager
        MenuManager.instance.SwitchMenu(Menu.MAIN_MENU);
    }
}
