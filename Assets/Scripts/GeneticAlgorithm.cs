using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GeneticAlgorithm : MonoBehaviour {

    // Constant values
    private const int NUMBER_INPUTS = 33;
    private const int NUMBER_OUTPUTS = 12;

    private const int POPULATION_SIZE = 30;


    // Other scripts
    TurnManager turnManager;

    class Unit {
        public double[,] chromosome;
        public bool tested = false;
        public int fitness = 0;

        public Unit(double[,] chromosomeMatrix) {
            chromosome = chromosomeMatrix;
        }

        public Unit(double[,] chromosomeMatrix, bool testedValue, int fitnessValue) {
            chromosome = chromosomeMatrix;
            tested = testedValue;
            fitness = fitnessValue;
        }
    }

    List<Unit> gamePopulation = new List<Unit>();

    // Auxiliar variables
    int i, j;
    int actualIndex = 0;

    void Start() {
        // Loading the population from disk
        bool gamePopupationLoaded = LoadPopulation(gamePopulation, "population.data");

        // If failed, create
        if (!gamePopupationLoaded)
            if (CreatePopulation(gamePopulation))
                SavePopulation(gamePopulation, "population.data");

        turnManager = GameObject.Find("Main Camera").GetComponent<TurnManager>();

        // Begin the game with this
        Invoke("Evaluation", 0.3f);
    }

    private bool LoadPopulation(List<Unit> population, System.String fileName) {
        try {
            System.String fileDirectory = @"Assets\SavedData\" + fileName;
            if (File.Exists(fileDirectory)) { // Checking if the file exists

                // Opening the file
                using (var filestream = File.Open(fileDirectory, FileMode.Open))
                using (var binaryStream = new BinaryReader(filestream)) {
                    double[,] chromosomeToLoad = new double[NUMBER_INPUTS, NUMBER_OUTPUTS];
                    bool testedValue;
                    int fitnessValue;
                    // Looking for all units
                    for (int unit = 0; unit < POPULATION_SIZE; unit++) {
                        // Taking the chromosome matrix
                        for (i = 0; i < NUMBER_INPUTS; i++)
                            for (j = 0; j < NUMBER_OUTPUTS; j++)
                                chromosomeToLoad[i, j] = binaryStream.ReadDouble(); // Probably there is a more efficient way to load the matrix
                        testedValue = binaryStream.ReadBoolean();
                        fitnessValue = binaryStream.ReadInt32();

                        Unit loadedUnit = new Unit(chromosomeToLoad, testedValue, fitnessValue);
                        population.Add(loadedUnit);
                    }
                }
                return true; // Success

            }
            else { // File don't exist
                print("File don't exist. Must create.");
                return false;
            }
        }
        catch(System.Exception e) {
            print("Exception when reading population. Error: " + e.Message);
            return false;
        }
    }

    private bool SavePopulation(List<Unit> population, System.String fileName) {
        try {
            System.String fileDirectory = @"Assets\SavedData\" + fileName;
            using (var filestream = File.Create(fileDirectory))
            using (var binarystream = new BinaryWriter(filestream)) {
                // Saving all units
                for (int unit = 0; unit < POPULATION_SIZE; unit++) {
                    // Saving the chromosome matrix
                    for (i = 0; i < NUMBER_INPUTS; i++)
                        for (j = 0; j < NUMBER_OUTPUTS; j++)
                            binarystream.Write(population[unit].chromosome[i, j]);
                    binarystream.Write(population[unit].tested);
                    binarystream.Write(population[unit].fitness);
                }
            }
            return true; // Success
        }
        catch (System.Exception e) {
            print("WARNING: Failed to save the population data on disk. Error: " + e.Message);
            return false;
        }
    }

    private bool CreatePopulation(List<Unit> population) {
        try {
            // Certify that the population is empty
            population.Clear();

            // Instantiating the chomossomes for both teams
            double[,] chromosome = new double[NUMBER_INPUTS, NUMBER_OUTPUTS];
            for (int unit = 0; unit < POPULATION_SIZE; unit++) {

                // Creating the chromosomes 
                for (i = 0; i < NUMBER_INPUTS; i++)
                    for (j = 0; j < NUMBER_OUTPUTS; j++) {
                        // Chromosome for the left team
                        chromosome[i, j] = Random.Range(-1.0f, 1.0f);
                    }

                // Creating the 'complete' unit
                Unit newUnit = new Unit(chromosome);

                // Add them to their population
                population.Add(newUnit);

            }
            return true; // Success
        }
        catch (System.Exception e) {
            print("Failed to create the population. Error: " + e.Message);
            return false;
        }
        
    }

    private void Evaluation() {
        // Reset game values
        turnManager.ResetGame();

        // Setting the NN weight
        for (; actualIndex < POPULATION_SIZE / 2; actualIndex++) { // Looking for the first to be evaluated
            if (gamePopulation[2 * actualIndex].tested)
                continue;

            // If runs here, found the chromosomes to be used this time
            turnManager.SetNeuralNetworkWeights(gamePopulation[2 * actualIndex].chromosome, gamePopulation[2 * actualIndex + 1].chromosome);
            break;
        }

        // Check if finished evaluating the population
        if (actualIndex == POPULATION_SIZE / 2) {
            PrepareNextGeneration();
            return;
        }

        // If is here, there are still chromosomes to be evaluated, and it is already setted on the NN
        turnManager.BeginGame();

    }

    public void Evaluated(int leftTeamFitness, int rightTeamFitness) {
        // Setting the fitness
        gamePopulation[2 * actualIndex].fitness = leftTeamFitness;
        gamePopulation[2 * actualIndex + 1].fitness = rightTeamFitness;

        // Marking as evaluated
        gamePopulation[2 * actualIndex].tested = gamePopulation[2 * actualIndex + 1].tested = true;

        // nextIndex
        actualIndex++;
    }

    private void PrepareNextGeneration() {
        print("Prepare next generation");
    }

}
