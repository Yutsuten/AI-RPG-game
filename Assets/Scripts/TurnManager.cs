using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour {
    // Battle info
    private const int NUMBER_OF_CHARACTERS = 6;

    private const int DEFEAT_BONUS = 10000;

    // Aux info
    private const int ZERO_PROBABILITY = -1;
    private const int NUMBER_OUTPUTS = 12; // of the neural network
    private const int NUMBER_COMMANDS = NUMBER_OUTPUTS - 1;

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

    class TargetCommandResult {
        public int target;
        public int command;
        public int subCommand;
        public double probability;

        public TargetCommandResult(int target, double targetProbability, int rawCommand, double commandProbability) {
            this.target = target;
            if (rawCommand <= 2) { // Attack or defend
                command = rawCommand;
                subCommand = 0;
            }
            else if (rawCommand <= 5) { // Skill
                command = 3;
                subCommand = rawCommand - 2;
            }
            else { // item
                command = 4;
                subCommand = rawCommand - 5;
            }
            probability = 1.5 * targetProbability + commandProbability;
        }
    }

    // List of members that exist on the battle
    class MembersManager {
        public int turnData;
        public Character member;

        public MembersManager(int turnInfo, Character script) {
            turnData = turnInfo;
            member = script;
        }
    }

    // Local variables
    private bool successfullCommand;
    private double[] neuralNetworkOutput = new double[NUMBER_OUTPUTS];
    private int indexBiggestProbability;

    private float leftTeamFitness;
    private float rightTeamFitness;
    private float defeatBonus;
    private bool updateDefeatBonus;
    private bool[] alreadyAttacked;

    // Members objects (set on Unity Editor)
    public GameObject[] members = new GameObject[NUMBER_OF_CHARACTERS];
    private Character[] character = new Character[NUMBER_OF_CHARACTERS];

    // Scripts
    private Item leftTeamInventory;
    private Item righTeamInventory;
    private EditGui leftTeamFitnessUI;
    private EditGui rightTeamFitnessUI;

    // Neural Network Component
    private NeuralNetwork neuralNetwork;
    private NeuralNetwork leftTeamNeuralNetwork;
    private NeuralNetwork rightTeamNeuralNetwork;

    // Genetic Algorithm Component
    private GeneticAlgorithm geneticAlgorithm;

    private Console console;

    // Creating the list of members with the turnTiming information
    private List<MembersManager> membersData = new List<MembersManager>();

    // Variable for the results of the NN
    private List<TargetCommandResult> targetCommandResult = new List<TargetCommandResult>();

	void Start() {
        // Getting and settings the characters info
        for (int i = 0; i < NUMBER_OF_CHARACTERS; i++) {
            // Getting the character script
            character[i] = members[i].GetComponent<Character>();
            // Putting on the turn structure
            MembersManager newMember = new MembersManager(0, character[i]);
            membersData.Add(newMember);
        }

        // Getting the Neural Network script
        GameObject teamObject = GameObject.Find("LeftTeam");
        leftTeamNeuralNetwork = teamObject.GetComponent<NeuralNetwork>();
        leftTeamInventory = teamObject.GetComponent<Item>();

        teamObject = GameObject.Find("RightTeam");
        rightTeamNeuralNetwork = teamObject.GetComponent<NeuralNetwork>();
        righTeamInventory = teamObject.GetComponent<Item>();

        leftTeamFitnessUI = GameObject.Find("Canvas/LeftFitnessInfo").GetComponent<EditGui>();
        rightTeamFitnessUI = GameObject.Find("Canvas/RightFitnessInfo").GetComponent<EditGui>();

        console = GameObject.Find("Canvas/Console").GetComponent<Console>();

        geneticAlgorithm = this.gameObject.GetComponent<GeneticAlgorithm>();
	}

    public void UpdateFitness(byte id, bool leftTeam, float addValue, bool targetDefeated, bool friendlyFire) {
        // Add the fitness value, check if the target is still 'on game'  (giving the bonus), and update the UI
        if (leftTeam) {
            if (friendlyFire)
                leftTeamFitness -= 100;
            else
                leftTeamFitness += 50;
            leftTeamFitness += addValue + (targetDefeated ? defeatBonus : 0);
            leftTeamFitnessUI.ChangeText(System.String.Format("Fitness{0}{1:0.0}", System.Environment.NewLine, leftTeamFitness));
        }

        else { // right team
            if (friendlyFire)
                rightTeamFitness -= 100;
            else
                rightTeamFitness += 50;
            rightTeamFitness += addValue + (targetDefeated ? defeatBonus : 0);
            rightTeamFitnessUI.ChangeText(System.String.Format("Fitness{0}{1:0.0}", System.Environment.NewLine, rightTeamFitness));
        }

        UpdateDefeatBonus(id);
    }

    private void UpdateDefeatBonus(byte id) {
        // Update the defeated target bonus on fitness
        if (updateDefeatBonus) {
            defeatBonus *= 0.985f;
        }
        else { // Everyone's first turn
            if (alreadyAttacked[id]) {
                updateDefeatBonus = true;
                defeatBonus *= 0.985f;
            }
            else
                alreadyAttacked[id] = true;
        }

        // For curiosity
        //print(System.String.Format("Defeat enemy bonus: {0:0.0}", defeatBonus));
    }

    public void ResetGame() {
        for (int i = 0; i < NUMBER_OF_CHARACTERS; i++) {
            membersData[i].turnData = 0;
            membersData[i].member.ResetStatus();
        }
        leftTeamInventory.ResetItems();
        righTeamInventory.ResetItems();

        leftTeamFitness = 0;
        rightTeamFitness = 0;
        defeatBonus = DEFEAT_BONUS;
        updateDefeatBonus = false;
        alreadyAttacked = new bool[NUMBER_OF_CHARACTERS]; // Default is FALSE

        // Reset Fitness UI
        leftTeamFitnessUI.ChangeText(System.String.Format("Fitness{0}{1}", System.Environment.NewLine, leftTeamFitness));
        rightTeamFitnessUI.ChangeText(System.String.Format("Fitness{0}{1}", System.Environment.NewLine, rightTeamFitness));

        console.ClearMessages();
    }

    public void BeginGame() {
        Invoke("SearchNext", 1.0f);
    }

    private void EndBattle() {
        geneticAlgorithm.Evaluated(leftTeamFitness, rightTeamFitness);
    }

    public void SetNeuralNetworkWeights(double[,] leftTeamNN_Weights, double[,] rightTeamNN_Weights) {
        // Setting weights for both teams
        leftTeamNeuralNetwork.SetWeightMatrix(leftTeamNN_Weights);
        rightTeamNeuralNetwork.SetWeightMatrix(rightTeamNN_Weights);
    }

    public void SearchNext() {
        while (membersData[0].turnData < 1000) { // Checking until someone have enough turnTiming

            // Running one time the UpdateTurn of the characters
            for (int i = 0; i < membersData.Count; i++) {
                if (membersData[i].member.OnGame()) // Only get turns if is alive
                    membersData[i].turnData = membersData[i].member.UpdateTurn();
            }

            // Sorting according to turnTiming
            membersData.Sort((s1, s2) => s2.turnData.CompareTo(s1.turnData));

        };

        // Checking if there is a valid target
        if (!membersData[0].member.targets[0].GetComponent<Character>().OnGame() &&
                !membersData[0].member.targets[1].GetComponent<Character>().OnGame() &&
                !membersData[0].member.targets[2].GetComponent<Character>().OnGame()) {
            // If entered here, there is a winner
            if (membersData[0].member.leftTeam) {
                print(System.String.Format("End of battle. Left team wins! Fitness {0:0.00} x {1:0.00}", leftTeamFitness, rightTeamFitness));
                console.AddMessage("End of battle. Left team wins!");
            }
            else {
                print(System.String.Format("End of battle. Right team wins! Fitness {0:0.00} x {1:0.00}", leftTeamFitness, rightTeamFitness));
                console.AddMessage("End of battle. Right team wins!");
            }
            EndBattle();
            return;
        }

        // Checking if happened a loop
        if (defeatBonus < 1000f) {
            print(System.String.Format("End of battle. This is a draw. Fitness {0:0.00} x {1:0.00}", leftTeamFitness, rightTeamFitness));
            console.AddMessage("End of battle. This is a draw.");
            EndBattle();
            return;
        }

        successfullCommand = false;

        //print(membersData[0].member.characterName + " will try the Neural Network.");
        // Setting the weights of the Neural Network with the character's team
        neuralNetwork = membersData[0].member.leftTeam ? leftTeamNeuralNetwork : rightTeamNeuralNetwork;

        // Reseting the structure that can sort the values of the output
        targetCommandResult.Clear();

        // Running the Neural Network and get the data about what to do
        for (int i = 0; i < NUMBER_OF_CHARACTERS; i++) {
            if (character[i].OnGame()) { // Only calculate if is alive
                // Setting input on the Neural Network
                bool successInput;
                successInput = neuralNetwork.SetInputArray(character[i].HpLost(), character[i].MpLost(), character[i].attack, character[i].defense, character[i].magicPower, character[i].resistance, character[i].speed, character[i].IsDefending(), character[i].elementResist, membersData[0].member.attack, membersData[0].member.magicPower, membersData[0].member.speed, membersData[0].member.weakSkill, membersData[0].member.strongSkill, membersData[0].member.leftTeam != character[i].leftTeam);

                if (!successInput)
                    print("Input failed");

                // Getting the output
                neuralNetworkOutput = neuralNetwork.RunNeuralNetwork();

                for (int commandValue = 1; commandValue < NUMBER_OUTPUTS; commandValue++) {
                    TargetCommandResult result = new TargetCommandResult(i, neuralNetworkOutput[0], commandValue, neuralNetworkOutput[commandValue]);
                    targetCommandResult.Add(result);

                    //print("Target " + result.target + " probability: " + result.probability + ". Command " + result.command + "-" + result.subCommand);
                }

                /* targetResult.Clear();
                TargetResult tgtRes = new TargetResult();
                tgtRes.targetProbability = neuralNetworkOutput[0];
                for (int j = 0; j < NUMBER_COMMANDS; j++) {
                    tgtRes.commandResult.commandProbability[j] = neuralNetworkOutput[j + 1];
                }
                targetResult.Add(tgtRes);*/

                /*print("Neural Network on " + character[i].characterName + ". Output: " + neuralNetworkOutput[0] + "; " + neuralNetworkOutput[1] + "; " + neuralNetworkOutput[2] + "; " + neuralNetworkOutput[3] + "; " + neuralNetworkOutput[4] + "; " + neuralNetworkOutput[5] + "; " + neuralNetworkOutput[6] + "; " + neuralNetworkOutput[7] + "; " + neuralNetworkOutput[8] + "; " + neuralNetworkOutput[9] + "; " + neuralNetworkOutput[10] + "; ");*/
            }
            
        }

        // Sort the targetResult to have the best target and command chosen
        targetCommandResult.Sort((s1, s2) => s2.probability.CompareTo(s1.probability));
        indexBiggestProbability = -1;

        // for testing
        /*for (int i = 0; i < 6; i++) {
            print(i + " option: " + );
        }*/

        do {
            indexBiggestProbability++;
            // for testing :3
            /*if (targetCommandResult[indexBiggestProbability].command == DEFENDING)
                continue;*/

            successfullCommand = membersData[0].member.MyTurn(members[targetCommandResult[indexBiggestProbability].target], targetCommandResult[indexBiggestProbability].command, targetCommandResult[indexBiggestProbability].subCommand);
        } while (!successfullCommand);
        

        // Updating that received a turn
        membersData[0].turnData -= 1000;
        // Reordering
        membersData.Sort((s1, s2) => s2.turnData.CompareTo(s1.turnData));

    }
}
