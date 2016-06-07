using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour {

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

    void SearchNext() {
        print("Beggining of my turn manager!");

        // Running one time the UpdateTurn of the characters
        for (int i = 0; i < membersData.Count; i++) {
            membersData[i].turnData = membersData[i].member.UpdateTurn();
        }

        // Checking the list unsorted
        print("List unsorted:");
        for (int i = 0; i < membersData.Count; i++) {
            print(membersData[i].member.characterName + " turnTiming is: " + membersData[i].turnData);
        }

        // Sorting according to turnTiming
        membersData.Sort((s1, s2) => s2.turnData.CompareTo(s1.turnData));

        // Checking the list sorted
        print("List sorted:");
        for (int i = 0; i < membersData.Count; i++) {
            print(membersData[i].member.characterName + " turnTiming is: " + membersData[i].turnData);
        }
    }
}
