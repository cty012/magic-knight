using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance { get; private set; }

    // Menu prefabs
    public Menu currentMenu { get; private set; }
    public GameObject currentMenuObject { get; private set; }
    private Dictionary<Menu, GameObject> menus;

    private MenuManager() { if (MenuManager.instance == null) MenuManager.instance = this; }

    private void Awake()
    {
        // Load resources
        this.menus = new Dictionary<Menu, GameObject>();
        this.menus[Menu.MAIN_MENU] = Resources.Load<GameObject>("Prefabs/UI/MainMenu");
        this.menus[Menu.NEW_GAME_MENU] = Resources.Load<GameObject>("Prefabs/UI/LoadMenu");
        this.menus[Menu.LOAD_MENU] = Resources.Load<GameObject>("Prefabs/UI/LoadMenu");
    }

    private void Start()
    {
        // Listen to scene change
        EventManager.instance.On("load-scene", (BaseEvent eventArgs) => { this.OnSceneLoaded((LoadSceneEvent)eventArgs); });
    }

    private void OnSceneLoaded(LoadSceneEvent eventArgs)
    {
        switch (eventArgs.type)
        {
            case LoadSceneEventType.LOAD_SCENE:
                this.ClearCanvas();
                if ("Menu".Equals(((LoadSceneEvent)eventArgs).scene.name)) this.SwitchMenu(Menu.MAIN_MENU);
                break;
            case LoadSceneEventType.LOAD_GAME_SCENE:
                this.ResetValues();
                break;
        }
    }

    public void ResetValues()
    {
        this.currentMenu = Menu.NULL;
        this.currentMenuObject = null;
    }

    // Close the sub-menu and clear everything else
    public void ClearCanvas()
    {
        // Reset values
        this.ResetValues();
        // Clear the canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        foreach (Transform transform in canvas.transform)
        {
            Destroy(transform.gameObject);
        }
    }

    // Switch to another sub-menu
    public void SwitchMenu(Menu menu)
    {
        // Make sure the operation is valid
        if (!this.menus.ContainsKey(menu)) return;
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        // Switch menu
        this.ClearCanvas();
        this.currentMenu = menu;
        this.currentMenuObject = Object.Instantiate(this.menus[menu], canvas.transform);
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
                else if ("slot".Equals(tag))
                {
                    DataManager.instance.save.CreateBasicSave();
                    DataManager.instance.SaveToDisk(button.GetComponent<SlotButtonController>().slotNumber);
                    GameManager.instance.LoadSave();
                }
                break;

            case Menu.LOAD_MENU:
                if ("back".Equals(tag))
                {
                    this.SwitchMenu(Menu.MAIN_MENU);
                }
                // Check if slot number is valid
                else if ("slot".Equals(tag))
                {
                    int slotNumber = button.GetComponent<SlotButtonController>().slotNumber;
                    if (DataManager.instance.SlotExists(slotNumber))
                    {
                        DataManager.instance.LoadFromDisk(slotNumber);
                        GameManager.instance.LoadSave();
                    }
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
