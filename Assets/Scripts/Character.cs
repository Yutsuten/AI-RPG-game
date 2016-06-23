using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

    // Information of itself
    public System.String characterName;
    public byte characterID;
    public GameObject characterInfo;
    private GameObject consoleInfo;

    public bool leftTeam;
    private bool facingEnemies = true;

    private EditGui displayGui;
    private System.String diplayInfo;

    // TurnManager
    private TurnManager turnManager;

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

    // Use for turns determination variable
    private int turnTiming = 0;
    private bool fighting = true;

    // Auxiliable variables
    private SpriteRenderer spriteRenderer;
    private bool[] finishedAnimation = new bool[2]; // 0 is self, 1 is target
    private float damageAnimationTimeout = 0.0f;
    private bool damageAnimation = false;
    private bool healingAnimation = false;
    private bool unconsciousAnimation = false;

    private int state = 3; // 0: Move ahead - 1: wait - 2: move back - 3: do nothing
    private bool moving = false;
    private float moveDistance;
    private float waitTime = 1.0f;
    private bool dealedDamage = false;

    // Constant values (so I can change things faster)
    private const float MOVE_DIST = 0.8f;
    private const float MOVE_SPEED = 2f;
    private const float DAMAGE_ANIMATION_SPEED = 1.2f;

    // Commands
    private const int NO_COMMAND = 0;
    private const int ATTACKING = 1;
    private const int DEFENDING = 2;
    private const int USING_SKILL = 3;
    private const int USING_ITEM = 4;

    // SKILLS
    private const int SIMPLE_COMMAND = 0;
    private const int WEAK_SKILL = 1;
    private const int STRONG_SKILL = 2;
    private const int HEALING_SKILL = 3;

    // ITEMS
    private const int PHYSICAL_DAMAGE = 1;
    private const int WATER_DAMAGE = 2;
    private const int FIRE_DAMAGE = 3;
    private const int EARTH_DAMAGE = 4;
    private const int WIND_DAMAGE = 5;
    private const int HEAL = 6;
    private const int NUM_OF_ITEMS = 6;

    // SKILLS MP CONSUMPTION
    private const int MP_SIMPLE_COMMAND = 0;
    private const int MP_WEAK_SKILL = 50;
    private const int MP_STRONG_SKILL = 110;
    private const int MP_HEALING_SKILL = 80;

    // FITNESS CALCULATION
    private const float DAMAGE_POWER = 1.5f;
    private const float HEAL_POWER = 1.4f;

	void Start() {
        // Getting the inventory
        if (leftTeam)
            inventory = GameObject.Find("LeftTeam").GetComponent<Item>();
        else
            inventory = GameObject.Find("RightTeam").GetComponent<Item>();

        displayGui = characterInfo.GetComponent<EditGui>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();

        // Taking the console
        consoleInfo = GameObject.Find("Canvas/Console");

        // Taking the TurnManager script
        turnManager = GameObject.Find("Main Camera").GetComponent<TurnManager>();
	}

    void Update() {
        #region DAMAGE_ANIMATION
        if (damageAnimation) {
            damageAnimationTimeout -= Time.deltaTime * DAMAGE_ANIMATION_SPEED;
            if (damageAnimationTimeout <= 0) {
                damageAnimationTimeout = 0;
                damageAnimation = false;
                if (!unconsciousAnimation) // if there is no more animation to run
                    this.source.GetComponent<Character>().TakeDamageAnimationFinished();

                if (unconsciousAnimation) {
                    damageAnimationTimeout = 1.0f; // Reset for the next animation (unconscious)
                }
            }
            //print("Damage animation timeout: " + damageAnimationTimeout);
            //print(spriteRenderer.color);
            spriteRenderer.color = new Color(1, 1 - (damageAnimationTimeout), 1 - (damageAnimationTimeout), 1);
        }
        #endregion

        #region UNCONSCIOUS_ANIMATION
        if (unconsciousAnimation && !damageAnimation) {
            damageAnimationTimeout -= Time.deltaTime * DAMAGE_ANIMATION_SPEED;
            if (damageAnimationTimeout <= 0) {
                damageAnimationTimeout = 0;
                unconsciousAnimation = false;
                this.source.GetComponent<Character>().TakeDamageAnimationFinished();
            }
            spriteRenderer.color = new Color(1, 1, 1, damageAnimationTimeout);
        }
        #endregion

        #region HEALING_ANIMATION
        if (healingAnimation) {
            damageAnimationTimeout -= Time.deltaTime * DAMAGE_ANIMATION_SPEED;
            if (damageAnimationTimeout <= 0) {
                damageAnimationTimeout = 0;
                healingAnimation = false;
                this.source.GetComponent<Character>().TakeDamageAnimationFinished();
            }
            spriteRenderer.color = new Color(1 - (damageAnimationTimeout), 1, 1 - (damageAnimationTimeout), 1);
        }
        #endregion

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
                            //print("Both animations finished");
                            FinishedTurn();
                        }
                    }
                    break;
            }
        }
        

    }

    public void ResetStatus() {
        hp_current = hp_max;
        mp_current = mp_max;

        command = 0;
        subCommand = 0;
        turnTiming = 0;
        fighting = true;
        state = 3;
        moving = false;
        waitTime = 1.0f;
        dealedDamage = false;

        finishedAnimation = new bool[2];
        damageAnimationTimeout = 0.0f;
        damageAnimation = false;
        healingAnimation = false;
        unconsciousAnimation = false;

        // Default color
        spriteRenderer.color = new Color(1, 1, 1, 1);

        Update_HP_MP();
    }

    /// <summary>
    /// Update turn variable.
    /// </summary>
    /// <returns>The turn timing value</returns>
    public int UpdateTurn() {
        return turnTiming += speed + Random.Range(0, 10); // speed plus random 0~9
    }

    public bool OnGame() {
        return fighting;
    }

    public float HpLost() {
        return hp_max - hp_current;
    }

    public float MpLost() {
        return mp_max - mp_current;
    }

    public bool MyTurn(GameObject target, int command, int subCommand) {
        // Verify if have enough MP
        if (command == USING_SKILL) {
            switch (subCommand) {
                case WEAK_SKILL:
                    if (mp_current < MP_WEAK_SKILL) {
                        //print("Dont have enough mana for WEAK SKILL");
                        return false;
                    }
                    break;
                case STRONG_SKILL:
                    if (mp_current < MP_STRONG_SKILL) {
                        //print("Dont have enough mana for STRONG SKILL");
                        return false;
                    }
                    break;
                case HEALING_SKILL:
                    if (mp_current < MP_HEALING_SKILL) {
                        //print("Dont have enough mana for HEALING SKILL");
                        return false;
                    }
                    break;
            }
        } // Verify if have enough items
        else if (command == USING_ITEM) {
            if (inventory.ItemQuantity(subCommand) == 0) {
                //print("Dont have enough ITEMs");
                return false;
            }
        }

        finishedAnimation[0] = false;
        finishedAnimation[1] = false;

        turnTiming -= 1000;
        //print(this.characterName + " got turn. Turn value was " + (turnTiming + 1000) + ", now is " + turnTiming + ".");

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
            turnManager.UpdateDefeatBonus(this.characterID);
            Invoke("FinishedTurn", 0.5f);
        }
        return true;
    }

    private void FinishedTurn() {
        turnManager.SearchNext();
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
            this.mp_current -= MP_WEAK_SKILL;
        }
        else if (subCommand == STRONG_SKILL) {
            this.mp_current -= MP_STRONG_SKILL;
        }
        else if (subCommand == HEALING_SKILL) {
            this.mp_current -= MP_HEALING_SKILL;
        }
        Update_HP_MP();
    }

    private void UseItem() {
        // Using the item on the target
        inventory.UseItem(subCommand, turnTarget, this.gameObject);
    }

    public bool ItemAvailable(int itemId) {
        return (inventory.ItemQuantity(itemId) > 0);
    }

    private void Update_HP_MP() {
        diplayInfo = System.String.Concat(characterName, System.Environment.NewLine,
            "HP ", (int)hp_current, "/", hp_max, System.Environment.NewLine,
            "MP ", (int)mp_current, "/", mp_max);
        displayGui.ChangeText(diplayInfo);
    }

    private float CalculateDamage(float attackerDamagePower, float defenderResistPower) {
        float damageMultiplier = attackerDamagePower / (0.3f * attackerDamagePower + 1.2f * defenderResistPower);
        damageMultiplier = damageMultiplier > 0.2f ? damageMultiplier : 0.2f;
        float damage = (0.3f * attackerDamagePower) * damageMultiplier;
        return (damage + Random.Range(-0.15f, 0.15f) * damage);
    } 

    // CALLED FROM ATTACKER
    public void TakingAttack(int atkValue, GameObject source) {
        float damage;
        
        // Check if is defending
        if (this.command == DEFENDING)
            damage = CalculateDamage(atkValue, 1.5f * this.defense); // Damage calculation
        else // No defending - normal damage
            damage = CalculateDamage(atkValue, this.defense); // Damage calculation
        hp_current -= damage;

        // Attacker
        this.source = source;

        // Damage animation
        damageAnimationTimeout = 1.0f;
        damageAnimation = true;

        // Validation if still have HP
        if (hp_current <= 0) {
            damage += (int)hp_current;
            hp_current = 0;
            unconsciousAnimation = true;
            fighting = false;
        }

        Update_HP_MP();

        // Updating team's Fitness - DAMAGE Dealed
        Character sourceScript = source.GetComponent<Character>();
        float fitnessValue = (float)System.Math.Pow(damage, DAMAGE_POWER);
        turnManager.UpdateFitness(sourceScript.characterID, sourceScript.leftTeam, (sourceScript.leftTeam != this.leftTeam ? fitnessValue : -fitnessValue),
            (sourceScript.leftTeam != this.leftTeam ? !fighting : false));

        // Printing on Console
        consoleInfo.GetComponent<Console>().AddMessage(System.String.Format("{0} attacked {1}. Damage: {2:0.#}", this.source.GetComponent<Character>().characterName, this.characterName, damage));

        //print(this.gameObject + " taking damage");
        //print(this.source + " was attacker");
    }

    public void TakingSkill(int magicValue, int element, int subCommand, GameObject source) {
        // Attacker
        this.source = source;

        if (subCommand == STRONG_SKILL) {
            magicValue = (int) (magicValue * 1.2f);
        }
        if (subCommand != HEALING_SKILL) {
            // Checking advantage or disvantages
            if (elementResist[element] == 0)
                magicValue *= 2;
            else if (elementResist[element] == 2)
                magicValue /= 2;

            // Damage calculation
            float damage;
            if (this.command == DEFENDING)
                damage = CalculateDamage(magicValue, 1.5f * this.resistance);
            else // No defending - normal damage
                damage = CalculateDamage(magicValue, this.resistance);
            hp_current -= damage;

            // Damage animation
            damageAnimationTimeout = 1.0f;
            damageAnimation = true;

            // Validation if still have HP
            if (hp_current <= 0) {
                damage += (int)hp_current;
                hp_current = 0;
                unconsciousAnimation = true;
                fighting = false;
            }

            Update_HP_MP();

            // Updating team's Fitness - DAMAGE Dealed
            Character sourceScript = source.GetComponent<Character>();
            float fitnessValue = (float)System.Math.Pow(damage, DAMAGE_POWER);
            turnManager.UpdateFitness(sourceScript.characterID, sourceScript.leftTeam, (sourceScript.leftTeam != this.leftTeam ? fitnessValue : -fitnessValue),
                (sourceScript.leftTeam != this.leftTeam ? !fighting : false));

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(System.String.Format("{0} used skill on {1}. Damage: {2:0.#}", this.source.GetComponent<Character>().characterName, this.characterName, damage));
        }
        else { // Healing skill
            float healValue = (0.5f * magicValue < hp_max - hp_current) ? (0.5f * magicValue) : (hp_max - hp_current);
            hp_current += healValue;
            /*if (hp_current > hp_max)
                hp_current = hp_max;*/

            // Healing animation
            damageAnimationTimeout = 1.0f;
            healingAnimation = true;

            Update_HP_MP();

            // Updating team's Fitness - HEALING
            Character sourceScript = source.GetComponent<Character>();
            float fitnessValue = (float)System.Math.Pow(healValue, HEAL_POWER);
            turnManager.UpdateFitness(sourceScript.characterID, sourceScript.leftTeam, (sourceScript.leftTeam == this.leftTeam ? fitnessValue : -fitnessValue),
                false); //(sourceScript.leftTeam == this.leftTeam ? !fighting : false)

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(System.String.Format("{0} used skill on {1}. Healed: {2:0.#}", this.source.GetComponent<Character>().characterName, this.characterName, healValue));
        }
    }

    public void TakingItem(int itemType, int itemStrength, System.String itemName, GameObject source) {
        // Attacker
        this.source = source;

        if (itemType != HEAL) {
            // Checking advantage or disvantages
            if (elementResist[itemType - 1] == 0)
                itemStrength *= 2;
            else if (elementResist[itemType - 1] == 2)
                itemStrength /= 2;

            // Damage calculation
            float damage = CalculateDamage(itemStrength, this.resistance);
            hp_current -= damage;

            // Damage animation
            damageAnimationTimeout = 1.0f;
            damageAnimation = true;

            // Validation if still have HP
            if (hp_current <= 0) {
                damage += (int)hp_current;
                hp_current = 0;
                unconsciousAnimation = true;
                fighting = false;
            }

            Update_HP_MP();

            // Updating team's Fitness - DAMAGE Dealed
            Character sourceScript = source.GetComponent<Character>();
            float fitnessValue = (float)System.Math.Pow(damage, DAMAGE_POWER);
            turnManager.UpdateFitness(sourceScript.characterID, sourceScript.leftTeam, (sourceScript.leftTeam != this.leftTeam ? fitnessValue : -fitnessValue),
                (sourceScript.leftTeam != this.leftTeam ? !fighting : false)); // only give defeat bonus if is an enemy

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(System.String.Format("{0} used {1} on {2}. Damage: {3:0.#}", this.source.GetComponent<Character>().characterName, itemName, this.characterName, damage));
        }
        else { // Healing item
            float healValue = (itemStrength < hp_max - hp_current) ? itemStrength : (hp_max - hp_current);
            hp_current += healValue;

            // Healing animation
            damageAnimationTimeout = 1.0f;
            healingAnimation = true;

            Update_HP_MP();

            // Updating team's Fitness - HEALING
            Character sourceScript = source.GetComponent<Character>();
            float fitnessValue = (float)System.Math.Pow(healValue, HEAL_POWER);
            turnManager.UpdateFitness(sourceScript.characterID, sourceScript.leftTeam, (sourceScript.leftTeam == this.leftTeam ? fitnessValue : -fitnessValue),
                false);

            // Printing on Console
            consoleInfo.GetComponent<Console>().AddMessage(System.String.Format("{0} used {1} on {2}. Damage: {3:0.#}", this.source.GetComponent<Character>().characterName, itemName, this.characterName, healValue));
        }
    }

    public void TakeDamageAnimationFinished() {
        finishedAnimation[1] = true;

        if (finishedAnimation[0] && finishedAnimation[1]) {
            //print("Both animations finished");
            FinishedTurn();
        }
    }
}
