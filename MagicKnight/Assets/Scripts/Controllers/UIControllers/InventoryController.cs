using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    // Activate children
    private GameObject itemsPanel;
    private GameObject optionsPanel;

    // Children
    private GameObject tabList;
    private GameObject nameList;
    private GameObject optionList;

    // Prefabs
    private GameObject nameListItem;

    private void Awake()
    {
        this.itemsPanel = this.transform.Find("Items").gameObject;
        this.optionsPanel = this.transform.Find("Options").gameObject;

        this.tabList = this.transform.Find("Tabs").gameObject;
        this.nameList = this.itemsPanel.transform.Find("Names").Find("Viewport").Find("Content").gameObject;
        this.optionList = this.optionsPanel.transform.Find("Content").gameObject;

        this.nameListItem = Resources.Load<GameObject>("Prefabs/UI/InventoryNameListItem");

        // Bind actions to buttons
        this.tabList.transform.Find("Consumable").GetComponent<Button>().onClick
            .AddListener(() => { this.SwitchTab(InventoryTab.CONSUMABLE); });
        this.tabList.transform.Find("NonConsumable").GetComponent<Button>().onClick
            .AddListener(() => { this.SwitchTab(InventoryTab.NON_CONSUMABLE); });
        this.tabList.transform.Find("Weapon").GetComponent<Button>().onClick
            .AddListener(() => { this.SwitchTab(InventoryTab.WEAPON); });
        this.tabList.transform.Find("Armor").GetComponent<Button>().onClick
            .AddListener(() => { this.SwitchTab(InventoryTab.ARMOR); });
        this.tabList.transform.Find("Options").GetComponent<Button>().onClick
            .AddListener(() => { this.SwitchTab(InventoryTab.OPTIONS); });

        // Start by showing the consumables
        this.tabList.transform.Find("Consumable").GetComponent<Button>().Select();
        this.SwitchTab(InventoryTab.CONSUMABLE);
    }

    private void SwitchTab(InventoryTab tab)
    {
        switch (tab)
        {
            case InventoryTab.CONSUMABLE:
                // Toggle visibility
                this.itemsPanel.SetActive(true);
                this.optionsPanel.SetActive(false);
                // Fill contents
                this.ShowCategory(ItemType.CONSUMABLE);
                break;
            case InventoryTab.NON_CONSUMABLE:
                // Toggle visibility
                this.itemsPanel.SetActive(true);
                this.optionsPanel.SetActive(false);
                // Fill contents
                this.ShowCategory(ItemType.NON_CONSUMABLE);
                break;
            case InventoryTab.WEAPON:
                // Toggle visibility
                this.itemsPanel.SetActive(false);
                this.optionsPanel.SetActive(false);
                break;
            case InventoryTab.ARMOR:
                // Toggle visibility
                this.itemsPanel.SetActive(false);
                this.optionsPanel.SetActive(false);
                break;
            case InventoryTab.OPTIONS:
                // Toggle visibility
                this.itemsPanel.SetActive(false);
                this.optionsPanel.SetActive(true);
                break;
        }
    }

    private void ShowCategory(ItemType type)
    {
        // Clear name list
        foreach (Transform child in this.nameList.transform) Object.Destroy(child.gameObject);

        // Find inventory info
        Inventory inventory = GameObject.Find("Player").GetComponent<PlayerController>().inventory;
        Dictionary<int, int> items = inventory.items[type];
        foreach (var pair in items)
        {
            Item item = new Item(pair.Key);
            GameObject itemObj = Object.Instantiate(this.nameListItem, this.nameList.transform);
            itemObj.transform.Find("Text").GetComponent<Text>().text = item.name;
            // TODO: save id and show count
        }
    }
}

public enum InventoryTab
{
    CONSUMABLE,
    NON_CONSUMABLE,
    WEAPON,
    ARMOR,
    OPTIONS,
    NULL
}
