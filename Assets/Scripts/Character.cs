using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

    // Information of itself
    public System.String characterName;
    public GameObject characterInfo;
    private GameObject consoleInfo;

    public bool leftTeam;
    private bool facingEnemies = true;

    private EditGui displayGui;
    private System.String diplayInfo;

    // Stats
    public int hp_max = 300, mp_max = 100,
        attack = 100, defense = 50, magicPower = 100,
        resistance = 50, speed = 80;
    private float hp_current, mp_current;

    // Resistances - 0: weak, 1: normal, 2: strong
    public int[] elementResist = new int[5];

    // Skills - 0: physical, 1: water, 2: fire, 3: earth, 4: wind
    public int weakSkill, strongSkill;

    // Items
    private Item inventory;

    // Targets link
    public GameObject[] targets = new GameObject[3];
    public GameObject[] allys = new GameObject[2];
    private GameObject source;
    private GameObject turnTarget;

    // Command variable
    private int command = 0; // See commands on const values
    private int subCommand = 0; // May be a skill or item ID

    // Auxiliable variables
    private SpriteRenderer spriteRenderer;
    private bool[] finishedAnimation = new bool[2]; // 0 is self, 1 is target
    private float damageAnimationTimeout = 0.0f;
    private bool damageAnimation = false;
    private bool healingAnimation = false;

    private int state = 3; // 0: Move ahead - 1: wait - 2: move back - 3: do nothing
    private bool moving = false;
    private float moveDistance;
    private float waitTime = 1.0f;
    private bool dealedDamage = false;

    // Constant values (so I can change things faster)
    private const float MOVE_DIST = 0.8f;
    private const float MOVE_SPEED = 2f;
    private const float DAMAGE_ANIMATION_SPEED = 1.2f;

    private const int NO_COMMAND = 0;
    private const int DEFENDING = 1;
    private const int ATTACKING = 2;
    private const int USING_SKILL = 3;
    private const int USING_ITEM = 4;

    private const int SIMPLE_COMMAND = 0;
    private const int WEAK_SKILL = 1;
    private const int STRONG_SKILL = 2;
    private const int HEALING_SKILL = 3;

    // ITEMS
    private const int PHYSICAL_DAMAGE = 0;
    private const int WATER_DAMAGE = 1;
    private const int FIRE_DAMAGE = 2;
    private const int EARTH_DAMAGE = 3;
    private const int WIND_DAMAGE = 4;
    private const int HEAL = 5;
    private const int NUM_OF_ITEMS = 6;

	void Start () {
        hp_current = hp_max;
        mp_current = mp_max;

        // Getting the inventory
        if (leftTeam)
            inventory = GameObject.Find("LeftTeam").GetComponent<Item>();
        else
            inventory = GameObject.Find("RightTeam").GetComponent<Item>();

        displayGui = characterInfo.GetComponent<EditGui>();

        if (this.characterName == "Tomato B") {
            MyTurn(targets[0], USING_ITEM, EARTH_DAMAGE);
        }

        spriteRenderer = this.GetComponent<SpriteRenderer>();

        // Taking the console
        consoleInfo = GameObject.Find("Canvas/Console");
	}

    void Update() {
        diplayInfo = System.String.Concat(characterName, System.Environment.NewLine,
            "HP ", hp_current, "/", hp_max, System.Environment.NewLine,
            "MP ", mp_current, "/", mp_max);
        displayGui.ChangeText(diplayInfo);

        // Damage animation
        if (damageAnimation) {
            damageAnimationTimeout -= Time.deltaTime * DAMAGE_ANIMATION_SPEED;
            if (damageAnimationTimeout <= 0) {
                damageAnimationTimeout = 0;
                damageAnimation = false;
                this.source.GetComponent<Character>().TakeDamageAnimationFinished();
            }
            //print("Damage animation timeout: " + damageAnimationTimeout);
            //print(spriteRenderer.color);
            spriteRenderer.color = new Color(1, 1 - (damageAnimationTimeout), 1 - (damageAnimationTimeout), 1);
        }

        // Healing animation
        if (healingAnimation) {
            damageAnimationTimeout -= Time.deltaTime * DAMAGE_ANIMATION_SPEED;
            if (damageAnimationTimeout <= 0) {
                damageAnimationTimeout = 0;
                healingAnimation = false;
                this.source.GetComponent<Character>().TakeDamageAnimationFinished();
            }
            spriteRenderer.color = new Color(1 - (damageAnimationTimeout), 1, 1 - (damageAnimationTimeout), 1);
        }

        if (state < 3) { // If must do something

            if (moving) { // If is moving
                float frameMove = Time.deltaTime * MOVE_SPEED;
                moveDistance -= frameMove;
                if (moveDistance < 0) { // in case of moving more than expected
                    frameMove += moveDistance;
                    moving = false;
                }

                if (leftTeam == facingEnemies) { // Move to right
                    this.transform.position += new Vector3(frameMove, 0f, 0f);
                }
                else { // Move to left
                    this.transform.position -= new Vector3(frameMove, 0f, 0f);
                }

            }

            switch (state) { // Check if will change state
                case 0: // Move ahead
                    if (!moving) { //finished moving
                        state++;
                        waitTime = 0.8f;
                    }
                    break;
                case 1: // Wait and execute command
                    waitTime -= Time.deltaTime;
                    if (!dealedDamage && waitTime <= 0.5f) {
                        // Select the command to execute
                        switch (command) {
                            case DEFENDING:
                                Defend();
                                break;
                            case ATTACKING:
                                Attack();
                                break;
                            case USING_SKILL:
                                UseSkill();
                                break;
                            case USING_ITEM:
                                UseItem();
                                break;
                        }
                        // finished command
                        dealedDamage = true;
                        this.command = NO_COMMAND;
                    } 
                    if (waitTime <= 0) {
                        state++;
                        facingEnemies = false;
                        this.gameObject.GetComponent<SpriteRenderer>().flipX = !(leftTeam == facingEnemies);
                        moving = true;
                        moveDistance = MOVE_DIST;
                    }
                    break;
                case 2: // Move back
                    if (!moving) { //finished moving
                        state++;
                        moveDistance = MOVE_DIST;
                        facingEnemies = true;
                        this.gameObject.GetComponent<SpriteRenderer>().flipX = !(leftTeam == facingEnemies);
                        finishedAnimation[0] = true;

                        if (finishedAnimation[0] && finishedAnimation[1]) {
                            print("Both animations finished");
                        }
                    }
                    break;
            }
        }
        

    }

    public void MyTurn(GameObject target, int command, int subCommand) {
        finishedAnimation[0] = false;
        finishedAnimation[1] = false;

        // Memorizing the target, the damage will be dealt on update()
        turnTarget = target;
        this.command = command;
        this.subCommand = subCommand;

        if (this.command != DEFENDING) { // Doing damage or heal
            state = 0;
            moveDistance = MOVE_DIST;
            dealedDamage = false;
            moving = true;
        }
        else { // is defending
            consoleInfo.GetComponent<Console>().AddMessage(this.characterName + " is defending.");
            //print(this.characterName + " is defending.");
            // call next!!
        }
    }

    private void Attack() {
        // Attacking the target
        Character targetInfo = turnTarget.GetComponent<Character>();
        //print(this.gameObject + " attacking");
        targetInfo.TakingAttack(this.attack, this.gameObject);
    }

    private void Defend() {

    }

    private void UseSkill() {
        // Attacking the target
        Character targetInfo = turnTarget.GetComponent<Character>();
        //print(this.gameObject + " attacking");
        targetInfo.TakingSkill(this.magicPower, this.weakSkill, this.subCommand, this.gameObject);
        
        // Removing MP
        if (subCommand == WEAK_SKILL) {
            this.mp_current -= 50;
        }
        else if (subCommand == STRONG_SKILL) {
            this.mp_current -= 110;
        }
        else if (subCommand == HEALING_SKILL) {
            this.mp_current -= 80;
        }
    }

    private void UseItem() {
        // Using the item on the target
        inventory.UseItem(subCommand, turnTarget, this.gameObject);
    }

    // CALLED FROM ATTACKER
    public void TakingAttack(int atkValue, GameObject source) {
        int damage;

        // Check if is defending
        if (this.command == DEFENDING)
            damage = (atkValue - this.defense * 2) > 0 ? (atkValue - this.defense * 2) : 0; // Damage calculation
        else // No defending - normal damage
            damage = (atkValue - this.defense) > 0 ? (atkValue - this.defense) : 0; // Damage calculation
        hp_current -= damage;

        // Attacker
        this.source = source;

        // Damage animation
        damageAnimationTimeout = 1.0f;
        damageAnimation = true;

        // Printing on Console
        consoleInfo.GetComponent<Console>().AddMessage(this.source.GetComponent<Character>().characterName +
            " attacked " + this.characterName + ". Damage: " + damage);

        //print(this.gameObject + " taking damage");
        //print(this.source + " was attacker");
    }

    public void TakingSkill(int magicValue, int element, int subCommand, GameObject source) {
        // Attacker
        this.source = source;

        if (subCommand == STRONG_SKILL) {
            magicValue *= 2;
        }
        if (subCommand != HEALING_SKILL) {
            // Checking advantage or disvantages
            if (elementResist[element] == 0)
                magicValue *= 2;
            else if (elementResist[element] == 2)
                magicValue /= 2;

            // Damage calculation
            int damage;
            if (this.command == DEFENDING)
                damage = (magicValue - this.resistance * 2) > 0 ? (magicValue - this.resistance * 2) : 0;
            else // No defending - normal damage
                damage = (magicValue - this.resistance) > 0 ? (magicValue - this.resistance) : 0;
            hp_current -= damage;

            // Damage animation
            damageAnimationTimeout = 1.0f;
            damageAnimation = true;

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(this.source.GetComponent<Character>().characterName +
                " used skill on " + this.characterName + ". Damage: " + damage);
        }
        else { // Healing skill
            int healValue = magicValue / 2;
            hp_current += healValue;
            /*if (hp_current > hp_max)
                hp_current = hp_max;*/

            // Healing animation
            damageAnimationTimeout = 1.0f;
            healingAnimation = true;

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(this.source.GetComponent<Character>().characterName +
                " used skill on " + this.characterName + ". Healed: " + healValue);
        }
    }

    public void TakingItem(int itemType, int itemStrength, System.String itemName, GameObject source) {
        // Attacker
        this.source = source;

        if (itemType != HEAL) {
            // Checking advantage or disvantages
            if (elementResist[itemType] == 0)
                itemStrength *= 2;
            else if (elementResist[itemType] == 2)
                itemStrength /= 2;

            // Damage calculation
            int damage = itemStrength;
            hp_current -= damage;

            // Damage animation
            damageAnimationTimeout = 1.0f;
            damageAnimation = true;

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(this.source.GetComponent<Character>().characterName +
                " used " + itemName + " on " + this.characterName + ". Damage: " + damage);
        }
        else { // Healing item
            int healValue = itemStrength;
            hp_current += healValue;

            // Healing animation
            damageAnimationTimeout = 1.0f;
            healingAnimation = true;

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(this.source.GetComponent<Character>().characterName +
                " used " + itemName + " on " + this.characterName + ". Heal: " + healValue);
        }
    }

    public void TakeDamageAnimationFinished() {
        finishedAnimation[1] = true;

        if (finishedAnimation[0] && finishedAnimation[1]) {
            print("Both animations finished");
        }
    }
}
