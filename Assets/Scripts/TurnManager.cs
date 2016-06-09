using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour {
    // Local variables
    private bool successfullCommand;

    // Commands
    private const int NO_COMMAND = 0;
    private const int DEFENDING = 1;
    private const int ATTACKING = 2;
    private const int USING_SKILL = 3;
    private const int USING_ITEM = 4;

    // SKILLS
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

    // leftGroupNeuralNetwork
    //public GameObject[] leftGroup = new GameObject[3];

    // rightGroupNeuralNetwork
    //public GameObject[] rightGroup = new GameObject[3];

    public GameObject[] members = new GameObject[6];

    class MembersManager {
        public int turnData;
        public Character member;

        public MembersManager(int turnInfo, Character script) {
            turnData = turnInfo;
            member = script;
        }
    }
    //MembersManager[] membersData = new MembersManager[6];
    List<MembersManager> membersData = new List<MembersManager>();

    //private Character[] membersData = new Character[6];
    //private int[] turnData = new int[6];

	void Start () {
        for (int i = 0; i < 6; i++) {
            MembersManager newMember = new MembersManager(0, members[i].GetComponent<Character>());
            membersData.Add(newMember);
        }

        // Begin after 1 second
        Invoke("SearchNext", 1);
	}

    //int count = -1;

    public void SearchNext() {
        //count++;

        while (membersData[0].turnData < 1000) { // Checking until someone have enough turnTiming

            // Running one time the UpdateTurn of the characters
            for (int i = 0; i < membersData.Count; i++) {
                membersData[i].turnData = membersData[i].member.UpdateTurn();
            }

            // Sorting according to turnTiming
            membersData.Sort((s1, s2) => s2.turnData.CompareTo(s1.turnData));

        };

        successfullCommand = false;

        do {
            successfullCommand = membersData[0].member.MyTurn(membersData[0].member.targets[Random.Range(0, 3)], Random.Range(2, 5), 1);
        } while (!successfullCommand);
        

        // Updating that received a turn
        membersData[0].turnData -= 1000;
        // Reordering
        membersData.Sort((s1, s2) => s2.turnData.CompareTo(s1.turnData));

        //print("Finished Search next! Count value: " + count);

        // Checking the list unsorted
        /*print("List unsorted:");
        for (int i = 0; i < membersData.Count; i++) {
            print(membersData[i].member.characterName + " turnTiming is: " + membersData[i].turnData);
        }*/

        // Checking the list sorted
        /*print("List sorted:");
        for (int i = 0; i < membersData.Count; i++) {
            print(membersData[i].member.characterName + " turnTiming is: " + membersData[i].turnData);
        }*/
    }
}
