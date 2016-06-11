using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {

    struct ItemInfo {
        public System.String name;
        public int quantity;
        public int type; // 0: physical, 1: water, 2: fire, 3: earth, 4: wind, 5: heal
        public int power; // like magic power value

        public ItemInfo(System.String itemName, int itemQuantity, int itemType, int itemPower) {
            name = itemName;
            quantity = itemQuantity;
            type = itemType;
            power = itemPower;
        }
    }

    private ItemInfo[] inventory;

    // CONST VALUES
    private const int PHYSICAL_DAMAGE = 1;
    private const int WATER_DAMAGE = 2;
    private const int FIRE_DAMAGE = 3;
    private const int EARTH_DAMAGE = 4;
    private const int WIND_DAMAGE = 5;
    private const int HEAL = 6;
    private const int NUM_OF_ITEMS = 6;

	void Start () {
        // Creating the inventory
        inventory = new ItemInfo[NUM_OF_ITEMS];

        // Creating the items on inventory
        for (int i = 0; i < inventory.Length; i++) {
            inventory[i].quantity = 1;
            inventory[i].type = i + 1;
            inventory[i].power = 40;
        }

        inventory[0].name = "Stick";
        inventory[1].name = "Ice";
        inventory[2].name = "Bomb";
        inventory[3].name = "Stone";
        inventory[4].name = "Balloon";
        inventory[5].name = "Potion";
	}

    public void UseItem(int itemId, GameObject target, GameObject source) {
        inventory[--itemId].quantity--;
        target.GetComponent<Character>().TakingItem(inventory[itemId].type, inventory[itemId].power, inventory[itemId].name, source);
    }

    public int ItemQuantity(int itemId) {
        return inventory[--itemId].quantity;
    }
}
