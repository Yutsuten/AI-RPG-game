using UnityEngine;
using System.Collections;

public class NeuralNetwork : MonoBehaviour {

    // Constant values
    private const int NUMBER_INPUTS = 27;
    private const int NUMBER_OUTPUTS = 12;

    // Sigmoid constant
    private const double SIGMOID_ALPHA = 0.002;

    // Neural Network variables
    private double[] input = new double[NUMBER_INPUTS];
    private double[,] weight = new double[NUMBER_INPUTS, NUMBER_OUTPUTS];
    private double[] output = new double[NUMBER_OUTPUTS];

    // Resistance 'translation'
    private int[] resistValue = { -1000, 0, 1000 };

    public bool SetWeightMatrix(double[,] weightMatrix) {
        try {
            // Setting the weight matrix
            /*for (int i = 0; i < NUMBER_INPUTS; i++)
                for (int j = 0; j < NUMBER_OUTPUTS; j++)
                    weight[i, j] = weightMatrix[i, j];*/
            weight = weightMatrix;
            /*print("Setting weight matrix");
            for (int i = 0; i < NUMBER_INPUTS; i++)
                for (int j = 0; j < NUMBER_OUTPUTS; j++)
                    print(System.String.Format("[{0}; {1}] = {2:0.00}", i, j, weight[i, j]));*/

            // Success
            return true;
        }
        catch (System.Exception e) {
            // Fail
            print("Failed to set the Weigth Matrix. Error: " + e.Message);
            return false;
        }
    }

    public bool SetInputArray(float targetHpLost, float targetMpLost, int targetAtk, int targetDef, int targetMag, int targetRes, int targetSpd, bool targetDefending, int[] targetElementResist, int selfAtk, int selfMag, int selfSpd, int selfWeakSkill, int selfStrongSkill, bool targetIsEnemie)
    {
        try {
            // Set the input array
            input[0] = (targetHpLost - 500) * 2;
            input[1] = (targetMpLost - 500) * 2;
            input[2] = (targetAtk - 500) * 2;
            input[3] = (targetDef - 500) * 2;
            input[4] = (targetMag - 500) * 2;
            input[5] = (targetRes - 500) * 2;
            input[6] = (targetSpd - 500) * 2;
            // Resistances - 0: weak (-1000), 1: normal (0), 2: strong (1000)
            for (int element = 0; element < targetElementResist.Length; element++) { // Setting from index 7 to 11
                input[7 + element] = resistValue[targetElementResist[element]];
            }
            input[12] = selfAtk;
            input[13] = selfMag;
            input[14] = selfSpd;

            // Skills - 0: physical, 1: water, 2: fire, 3: earth, 4: wind
            for (int element = 0; element < 5; element++) { // Setting from index 15 to 24
                // Weak Skill (from index 15 to 19)
                input[15 + element] = (element == selfWeakSkill) ? 1000 : -1000;

                // Strong Skill (from index 20 to 24)
                input[20 + element] = (element == selfStrongSkill) ? 1000 : -1000;
            }
            input[25] = targetDefending ? 1000 : -1000;
            input[26] = targetIsEnemie ? 1000 : -1000;

            // Reducing the range of the input from -1000 ~ 1000 to: value * SIGMOID_ALPHA
            for (int i = 0; i < NUMBER_INPUTS; i++)
                input[i] *= SIGMOID_ALPHA;
            return true;
        }
        catch (System.Exception e) {
            print("Failed to set the input array. Error: " + e.Message);
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
            print("Failed to run the Neural Network. Error: " + e.Message);
            return new double[NUMBER_OUTPUTS];
        }
    }

    private double Sigmoid(double value) {
        double k = System.Math.Exp(value);
        return k / (1.0 + k);

        // Mathematically-equivalent to:
        //return 1.0 / (1.0 + (double)System.Math.Exp(-value));
    }

}
