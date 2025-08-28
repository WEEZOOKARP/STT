using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Item
{
    public string name;
    public string description;
    public int type; // 0 = melee, 1 = primitive, 2 = gun, 3 = other
    // Ammo is only relevant for guns and primitive weapons, for simplicity if it doesn't fire a projectile set to 0
    public int ammo;

    public Item(string name, string description, int type, int ammo)
    {
        this.name = name;
        this.description = description;
        this.type = type;
        this.ammo = ammo;
    }
}

public class Inventory : MonoBehaviour
{
    // The player's inventory is initialised here.
    public List<Item> items = new List<Item>();
    public int maxInventorySize = 3; // one slot for each weapon type

    // Add item
    // Example usage:
    // inventory.AddItem(new Item("Example Sword", "A basic sword.", 0, 0));
    public bool AddItem(Item newItem)
    {
        if (items.Count >= maxInventorySize)
        {
            Debug.Log("Inventory full! Cannot add " + newItem.name);
            return false;
        }

        items.Add(newItem);
        Debug.Log("Added " + newItem.name + " to inventory.");
        return true;
    }

    // Remove an item from the inventory by name
    public bool RemoveItem(string itemName)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].name == itemName)
            {
                Debug.Log("Removed " + items[i].name + " from inventory.");
                items.RemoveAt(i);
                return true;
            }
        }
        Debug.Log("Item " + itemName + " not found in inventory.");
        return false;
    }

    // List all items in the inventory
    public void ListItems()
    {
        Debug.Log("Inventory Contents:");
        foreach (Item item in items)
        {
            string itemInfo = $"Name: {item.name}, Type: {item.type}, Ammo: {item.ammo}, Desc: {item.description}";
            Debug.Log(itemInfo);
        }
    }
}
