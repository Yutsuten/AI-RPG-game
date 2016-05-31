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

	// Use this for initialization
	void Start () {
        hp_current = hp_max;
        mp_current = mp_max;

        displayGui = characterInfo.GetComponent<EditGui>();
	}

    // PUBLIC FUNCTIONS
    public void InitializeCharacter()
    {
        // Initialize all variables (stats)
        return;
    }
	
	// Update is called once per frame
	void Update () {
        diplayInfo = System.String.Concat(characterName, System.Environment.NewLine,
            "HP ", hp_max, "/", hp_current, System.Environment.NewLine,
            "MP ", mp_max, "/", mp_current);
        displayGui.ChangeText(diplayInfo);
	}
}
