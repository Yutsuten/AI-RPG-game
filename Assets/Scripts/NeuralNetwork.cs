using UnityEngine;
using System.Collections;

public class NeuralNetwork : MonoBehaviour {

    // Constant values
    private const int NUMBER_INPUTS = 33;
    private const int NUMBER_OUTPUTS = 12;

    // Neural Network variables
    private double[] input = new double[NUMBER_INPUTS];
    private double[,] weight = new double[NUMBER_INPUTS, NUMBER_OUTPUTS];
    private double[] output = new double[NUMBER_OUTPUTS];

    // Resistance 'translation'
    private int[] resistValue = { 1000, 0, -1000 };

    public bool SetWeightMatrix(double[,] weightMatrix) {
        try {
            // Setting the weight matrix
            for (int i = 0; i < NUMBER_INPUTS; i++)
                for (int j = 0; j < NUMBER_OUTPUTS; j++)
                    weight[i, j] = weightMatrix[i, j];

            // Success
            return true;
        }
        catch (System.Exception e) {
            // Fail
            System.Console.Out.WriteLine("Failed to set the Weigth Matrix. Error: " + e.Message);
            return false;
        }
    }

    public bool SetInputArray(float targetHpLost, float targetMpLost, int targetAtk, int targetDef, int targetMag, int targetRes, int targetSpd, int[] targetElementResist, float selfMpLost, int selfAtk, int selfMag, int selfSpd, int selfWeakSkill, int selfStrongSkill, bool itemPhysicalAvailable, bool itemWaterAvailable, bool itemFireAvailable, bool itemEarthAvailable, bool itemWindAvailable, bool itemPotionAvailable, bool targetIsEnemie) {
        try {
            // Set the input array
            input[0] = targetHpLost;
            input[1] = targetMpLost;
            input[2] = targetAtk;
            input[3] = targetDef;
            input[4] = targetMag;
            input[5] = targetRes;
            input[6] = targetSpd;
            // Resistances - 0: weak (1000), 1: normal (0), 2: strong (-1000)
            for (int element = 0; element < targetElementResist.Length; element++) { // Setting from index 7 to 11
                input[7 + element] = resistValue[targetElementResist[element]];
            }
            input[12] = selfMpLost;
            input[13] = selfAtk;
            input[14] = selfMag;
            input[15] = selfSpd;
            // Skills - 0: physical, 1: water, 2: fire, 3: earth, 4: wind
            for (int element = 0; element < 5; element++) { // Setting from index 16 to 25
                // Weak Skill (from index 16 to 20)
                input[16 + element] = (element == selfWeakSkill) ? 1000 : 0;

                // Strong Skill (from index 21 to 25)
                input[21 + element] = (element == selfStrongSkill) ? 1000 : 0;
            }
            input[26] = itemPhysicalAvailable ? 1000 : 0;
            input[27] = itemWaterAvailable ? 1000 : 0;
            input[28] = itemFireAvailable ? 1000 : 0;
            input[29] = itemEarthAvailable ? 1000 : 0;
            input[30] = itemWindAvailable ? 1000 : 0;
            input[31] = itemPotionAvailable ? 1000 : 0;
            input[32] = targetIsEnemie ? 1000 : -1000;
            return true;
        }
        catch (System.Exception e) {
            System.Console.Out.WriteLine("Failed to set the input array. Error: ", e.Message);
            return false;
        }
    }

    public double[] RunNeuralNetwork() {
        try {
            // Clearing the output
            output = new double[NUMBER_OUTPUTS];
            // Calculating the output
            for (int i = 0; i < NUMBER_INPUTS; i++)
                for (int j = 0; j < NUMBER_OUTPUTS; j++)
                    output[j] += input[i] * weight[i, j];
            // Output function
            for (int j = 0; j < NUMBER_OUTPUTS; j++) {
                //print("Before sigmoid function: " + output[j]);
                output[j] = Sigmoid(output[j]);
                //print("After sigmoid function: " + output[j]);
            }
                
            // Return it
            return output;
        }
        catch (System.Exception e) {
            System.Console.Out.WriteLine("Failed to run the Neural Network. Error: ", e.Message);
            return new double[NUMBER_OUTPUTS];
        }
    }

    private double Sigmoid(double value) {
        double k = System.Math.Exp(value);
        return k / (1.0 + k);

        // Mathematically-equivalent to:
        // return 1.0 / (1.0 + (double) Math.Exp(-value));
    }

}
