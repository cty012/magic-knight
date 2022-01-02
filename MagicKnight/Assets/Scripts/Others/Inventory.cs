using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public readonly Dictionary<ItemType, Dictionary<int, int>> items;

    public Inventory() : this(DataManager.instance.save["player"].Get<Dictionary<int, int>>("inventory")) { }

    public Inventory(Dictionary<int, int> idList)
    {
        this.items = new Dictionary<ItemType, Dictionary<int, int>>();
        this.items[ItemType.CONSUMABLE] = new Dictionary<int, int>();
        this.items[ItemType.NON_CONSUMABLE] = new Dictionary<int, int>();
        this.items[ItemType.WEAPON] = new Dictionary<int, int>();
        this.items[ItemType.ARMOR] = new Dictionary<int, int>();
        this.items[ItemType.NULL] = new Dictionary<int, int>();
        foreach (var pair in idList)
        {
            this.AddItem(pair.Key, pair.Value);
        }
    }

    public void AddItem(int id, int count = 1)
    {
        if (count <= 0) return;
        Item item = new Item(id);

        // Check if exist
        if (!this.items[item.type].ContainsKey(item.id)) this.items[item.type][item.id] = 0;

        // Add the items
        this.items[item.type][item.id] += count;
    }

    public int CountItem(int id)
    {
        Item item = new Item(id);

        // Count the items
        if (!this.items[item.type].ContainsKey(item.id)) return 0;
        return this.items[item.type][item.id];
    }

    public bool RemoveItem(int id, int count = 1)
    {
        if (count <= 0) return count == 0;
        Item item = new Item(id);

        // Check if have enough
        if (this.CountItem(id) < count) return false;

        // Remove the items
        this.items[item.type][item.id] -= count;

        // Check if empty
        if (this.items[item.type][item.id] == 0) this.items[item.type].Remove(item.id);
        return true;
    }
}

public class Item
{
    public readonly ItemType type;

    public readonly int id;
    public readonly string name;
    public readonly string imgPath;
    public readonly string description;

    public Item(int id)
    {
        this.id = id;
        string itemInfoString = DataManager.instance.temp["items"].Get<List<string>>("data")[id];
        string[] itemInfo = itemInfoString.Split(';');
        this.type = this.StringToItemType(itemInfo[0]);
        this.name = itemInfo[1];
        this.imgPath = itemInfo[2];
        this.description = itemInfo[3];
    }

    // Helper method
    private ItemType StringToItemType(string input)
    {
        if ("consumable".Equals(input)) return ItemType.CONSUMABLE;
        else if ("non-consumable".Equals(input)) return ItemType.NON_CONSUMABLE;
        else if ("weapon".Equals(input)) return ItemType.WEAPON;
        else if ("armor".Equals(input)) return ItemType.ARMOR;
        else return ItemType.NULL;
    }
}

public enum ItemType
{
    CONSUMABLE,
    NON_CONSUMABLE,
    WEAPON,
    ARMOR,
    NULL
}
