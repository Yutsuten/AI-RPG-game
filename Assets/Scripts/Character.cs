using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

    public System.String characterName;
    public GameObject characterInfo;

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

    // Auxiliable variables
    private SpriteRenderer spriteRenderer;
    private bool[] finishedAnimation = new bool[2]; // 0 is self, 1 is target
    private float damageAnimationTimeout = 0.0f;
    private bool damageAnimation = false;

	void Start () {
        hp_current = hp_max;
        mp_current = mp_max;

        displayGui = characterInfo.GetComponent<EditGui>();

        if (this.characterName == "Tomato A") {
            MyTurn(targets[0], 0);
        }

        spriteRenderer = this.GetComponent<SpriteRenderer>();
	}

    void Update() {
        diplayInfo = System.String.Concat(characterName, System.Environment.NewLine,
            "HP ", hp_current, "/", hp_max, System.Environment.NewLine,
            "MP ", mp_current, "/", mp_max);
        displayGui.ChangeText(diplayInfo);

        // Damage animation
        spriteRenderer.color = new Color(1, 1 - (damageAnimationTimeout), 1 - (damageAnimationTimeout), 1);
        if (damageAnimation) {
            damageAnimationTimeout -= Time.deltaTime * 1.2f;
            if (damageAnimationTimeout <= 0) {
                damageAnimationTimeout = 0;
                damageAnimation = false;
                this.source.GetComponent<Character>().TakeDamageAnimationFinished();
            }
            //print("Damage animation timeout: " + damageAnimationTimeout);
            //print(spriteRenderer.color);
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
        // Attacking the target
        Character targetInfo = target.GetComponent<Character>();
        //print(this.gameObject + " attacking");
        targetInfo.TakingAttack(this.attack, this.gameObject);

        finishedAnimation[0] = true;
    }
	
}
