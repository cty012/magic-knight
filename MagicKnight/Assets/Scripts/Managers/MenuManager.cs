using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance { get; private set; }

    public GameObject canvas { get; private set; }

    // Menu prefabs
    public Menu currentMenu { get; private set; }
    public GameObject currentMenuObject { get; private set; }
    private Dictionary<Menu, GameObject> menus;

    private MenuManager() { MenuManager.instance = this; }

    private void Awake()
    {
        // Load resources
        this.canvas = GameObject.Find("Canvas");
        this.menus = new Dictionary<Menu, GameObject>();
        this.menus[Menu.MAIN_MENU] = Resources.Load<GameObject>("Prefabs/UI/MainMenu");
        this.menus[Menu.NEW_GAME_MENU] = Resources.Load<GameObject>("Prefabs/UI/LoadMenu");
        this.menus[Menu.LOAD_MENU] = Resources.Load<GameObject>("Prefabs/UI/LoadMenu");
    }

    private void Start()
    {
        // Load the main menu
        this.SwitchMenu(Menu.MAIN_MENU);
    }

    // Close the sub-menu and clear everything else
    public void ClearCanvas()
    {
        this.currentMenu = Menu.NULL;
        this.currentMenuObject = null;
        foreach (Transform transform in this.canvas.transform)
        {
            Destroy(transform.gameObject);
        }
    }

    // Switch to another sub-menu
    public void SwitchMenu(Menu menu)
    {
        if (!this.menus.ContainsKey(menu)) return;
        this.ClearCanvas();
        this.currentMenu = menu;
        this.currentMenuObject = Object.Instantiate(this.menus[menu], this.canvas.transform);
    }

    public void ButtonsOnClick(string tag, Button button)
    {
        if (tag.Length == 0) return;

        switch (this.currentMenu)
        {
            case Menu.MAIN_MENU:
                if ("continue".Equals(tag))
                {
                    DataManager.instance.LoadFromDisk(button.GetComponent<ContinueButtonController>().slotNumber);
                    GameManager.instance.LoadSave();
                }
                else if ("new-game".Equals(tag))
                {
                    this.SwitchMenu(Menu.NEW_GAME_MENU);
                    this.currentMenuObject.transform.Find("Title").GetComponent<Text>().text = "Select a Slot to Save";
                }
                else if ("load-game".Equals(tag))
                {
                    this.SwitchMenu(Menu.LOAD_MENU);
                    this.currentMenuObject.transform.Find("Title").GetComponent<Text>().text = "Select a Slot to Load";
                }
                else if ("settings".Equals(tag))
                {
                    // TODO: load the settings sub-menu
                }
                else if ("exit".Equals(tag))
                {
                    Application.Quit();
                }
                break;

            case Menu.NEW_GAME_MENU:
                if ("back".Equals(tag))
                {
                    this.SwitchMenu(Menu.MAIN_MENU);
                }
                // Check if slot number is valid
                else if (int.TryParse(tag, out int slotNumber))
                {
                    DataManager.instance.LoadFromDisk(slotNumber);
                    GameManager.instance.LoadSave();
                }
                break;

            case Menu.LOAD_MENU:
                if ("back".Equals(tag))
                {
                    this.SwitchMenu(Menu.MAIN_MENU);
                }
                // Check if slot number is valid
                else if (int.TryParse(tag, out int slotNumber) && DataManager.instance.SlotExists(slotNumber))
                {
                    DataManager.instance.LoadFromDisk(slotNumber);
                    GameManager.instance.LoadSave();
                }
                break;
        }
    }
}

public enum Menu
{
    MAIN_MENU,
    NEW_GAME_MENU,
    LOAD_MENU,
    NULL
}
