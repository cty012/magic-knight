using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: inventory
public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    private GameObject uiPlayerStatusPrefab;
    private GameObject uiInventoryPrefab;

    public GameObject uiPlayerStatus { get; private set; }
    public GameObject uiInventory { get; private set; }

    private UIManager() { if (UIManager.instance == null) UIManager.instance = this; }

    private void Awake()
    {
        this.uiPlayerStatusPrefab = Resources.Load<GameObject>("Prefabs/UI/PlayerStatus");
        this.uiInventoryPrefab = Resources.Load<GameObject>("Prefabs/UI/Inventory");
        this.uiInventoryPrefab.SetActive(false);
    }

    private void Start()
    {
        // Listen to scene change
        EventManager.instance.On("load-scene", (BaseEvent eventArgs) => { this.OnSceneLoaded((LoadSceneEvent)eventArgs); });
        // Listen to status change
        EventManager.instance.On("hp-change", (BaseEvent eventArgs) => { this.OnPlayerStatusChange((PlayerStatusChangeEvent)eventArgs); });
        EventManager.instance.On("mp-change", (BaseEvent eventArgs) => { this.OnPlayerStatusChange((PlayerStatusChangeEvent)eventArgs); });
    }

    private void OnSceneLoaded(LoadSceneEvent eventArgs)
    {
        switch (eventArgs.type)
        {
            case LoadSceneEventType.LOAD_SCENE:
                break;
            case LoadSceneEventType.LOAD_GAME_SCENE:
                GameObject canvas = GameObject.Find("Canvas");
                this.uiPlayerStatus = Object.Instantiate(this.uiPlayerStatusPrefab, canvas.transform);
                this.uiInventory = Object.Instantiate(this.uiInventoryPrefab, canvas.transform);
                break;
        }
    }

    // Update player status when it changes
    private void OnPlayerStatusChange(PlayerStatusChangeEvent eventArgs)
    {
        if (this.uiPlayerStatus == null) return;
        switch (eventArgs.type)
        {
            case PlayerStatusChangeEventType.HP_CHANGE:
                StatusBarController hpBar = this.uiPlayerStatus.transform.Find("HP").GetComponent<StatusBarController>();
                hpBar.SetMaxValue(eventArgs.maxValue);
                hpBar.SetValue(eventArgs.value);
                break;
            case PlayerStatusChangeEventType.MP_CHANGE:
                StatusBarController mpBar = this.uiPlayerStatus.transform.Find("MP").GetComponent<StatusBarController>();
                mpBar.SetMaxValue(eventArgs.maxValue);
                mpBar.SetValue(eventArgs.value);
                break;
        }
    }

    public void ButtonsOnClick(string tag, Button button)
    {
        if (tag.Length == 0) return;

        if ("toggle-pause".Equals(tag))
        {
            GameManager.instance.TogglePause();
        }
        else if ("menu".Equals(tag))
        {
            GameManager.instance.LoadScene("Menu");
        }
        else
        {
            Application.Quit();
        }
    }
}
