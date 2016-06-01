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
    public int physicalResist,
        waterResist, fireResist,
        earthResist, windResist;

    // Skills - 0: physical, 1: water, 2: fire, 3: earth, 4: wind
    public int weakSkill, strongSkill;

    // Targets link
    public GameObject[] targets = new GameObject[3];
    public GameObject[] allys = new GameObject[2];
    private GameObject source;
    private GameObject turnTarget;

    // Auxiliable variables
    private SpriteRenderer spriteRenderer;
    private bool[] finishedAnimation = new bool[2]; // 0 is self, 1 is target
    private float damageAnimationTimeout = 0.0f;
    private bool damageAnimation = false;

    private int state = 3; // 0: Move ahead - 1: wait - 2: move back - 3: do nothing
    private bool moving = false;
    private float moveDistance;
    private float waitTime = 1.0f;
    private bool dealedDamage = false;


    // Constant values (so I can change things faster)
    private const float MOVE_DIST = 0.8f;
    private const float MOVE_SPEED = 2f;
    private const float DAMAGE_ANIMATION_SPEED = 1.2f;

	void Start () {
        hp_current = hp_max;
        mp_current = mp_max;

        displayGui = characterInfo.GetComponent<EditGui>();

        if (this.characterName == "Tomato B") {
            MyTurn(targets[0], 0);
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
        spriteRenderer.color = new Color(1, 1 - (damageAnimationTimeout), 1 - (damageAnimationTimeout), 1);
        if (damageAnimation) {
            damageAnimationTimeout -= Time.deltaTime * DAMAGE_ANIMATION_SPEED;
            if (damageAnimationTimeout <= 0) {
                damageAnimationTimeout = 0;
                damageAnimation = false;
                this.source.GetComponent<Character>().TakeDamageAnimationFinished();
            }
            //print("Damage animation timeout: " + damageAnimationTimeout);
            //print(spriteRenderer.color);
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
                        // Attacking the target
                        Character targetInfo = turnTarget.GetComponent<Character>();
                        //print(this.gameObject + " attacking");
                        targetInfo.TakingAttack(this.attack, this.gameObject);
                        // finished damage
                        dealedDamage = true;
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

    // PUBLIC FUNCTIONS
    public void InitializeCharacter() {
        // Initialize all variables (stats)
        return;
    }

    public void TakingAttack(int atkValue, GameObject source) {
        // Damage calculation
        int damage = (atkValue - this.defense) > 0 ? (atkValue - this.defense) : 0;
        hp_current -= damage;

        // Attacker
        this.source = source;

        // Damage animation
        damageAnimationTimeout = 1.0f;
        damageAnimation = true;

        // Printing on Console
        consoleInfo.GetComponent<EditGui>().AddText(this.source.GetComponent<Character>().characterName +
            " attacked " + this.characterName + ". Damage: " + damage + System.Environment.NewLine);

        //print(this.gameObject + " taking damage");
        //print(this.source + " was attacker");
    }

    public void TakeDamageAnimationFinished() {
        finishedAnimation[1] = true;

        if (finishedAnimation[0] && finishedAnimation[1]) {
            print("Both animations finished");
        }
    }

    public void MyTurn(GameObject target, int command) {
        finishedAnimation[0] = false;
        finishedAnimation[1] = false;

        // Memorizing the target, the damage will be dealt on update()
        turnTarget = target;

        state = 0;
        moveDistance = MOVE_DIST;
        dealedDamage = false;
        moving = true;
    }
	
}
