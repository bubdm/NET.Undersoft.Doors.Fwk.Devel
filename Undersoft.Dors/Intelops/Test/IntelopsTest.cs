using System.Diagnostics;

namespace System.Dors.Intelops
{
    public class IntelopsTest
    {
        public IntelopsTest(int numInput, int numHidden, int numOutput)
        {
            RunTest();
        }

        public void RunTest()
        {
            try
            {
                Debug.WriteLine("\nBegin Neural Network training using Back-Propagation demo\n");

                Random rnd = new Random(1); // for random weights. not used.

                double[] xValues = new double[6] { -1.0, 2.0, 3.0, 2.0, 3.5, 1.0 }; // inputs
                double[] yValues; // outputs
                double[] tValues = new double[3] { 0.1234, 0.8766, 0.4356 }; // target values

                Debug.WriteLine("The fixed input xValues are:");
                ShowVector(xValues, 1, 8, true);

                Debug.WriteLine("The fixed target tValues are:");
                ShowVector(tValues, 4, 8, true);

                int numInput = 6;
                int numHidden = 8;
                int numOutput = 3;
                int numWeights = (numInput * numHidden) + (numHidden * numOutput) + (numHidden + numOutput);

                Debug.WriteLine("Creating a " + numInput + "-input, " + numHidden + "-hidden, " + numOutput + "-output neural network");
                Debug.WriteLine("Using hard-coded tanh function for hidden layer activation");
                Debug.WriteLine("Using hard-coded log-sigmoid function for output layer activation");

                Foresense prophet = new Foresense(numInput, numHidden, numOutput);

                Debug.WriteLine("\nGenerating random initial weights and bias values");
                double[] initWeights = new double[numWeights];
                for (int i = 0; i < initWeights.Length; ++i)
                    initWeights[i] = (0.1 - 0.01) * rnd.NextDouble() + 0.01;

                Debug.WriteLine("\nInitial weights and biases are:");
                ShowVector(initWeights, 3, 8, true);

                Debug.WriteLine("Loading neural network initial weights and biases into neural network");
                prophet.SetWeights(initWeights);

                double learnRate = 0.5;  // learning rate - controls the maginitude of the increase in the change in weights.
                double momentum = 0.1; // momentum - to discourage oscillation.
                Debug.WriteLine("Setting learning rate = " + learnRate.ToString("F2") + " and momentum = " + momentum.ToString("F2"));

                int maxEpochs = 10000;
                double errorThresh = 0.00001;
                Debug.WriteLine("\nSetting max epochs = " + maxEpochs + " and error threshold = " + errorThresh.ToString("F6"));

                int epoch = 0;
                double error = double.MaxValue;
                Debug.WriteLine("\nBeginning training using back-propagation\n");

                while (epoch < maxEpochs) // train
                {
                    if (epoch % 20 == 0) Console.WriteLine("epoch = " + epoch);

                    yValues = prophet.ComputeOutputs(xValues);
                    error = ErrorMargin(tValues, yValues);
                    if (error < errorThresh)
                    {
                        Console.WriteLine("Found weights and bias values that meet the error criterion at epoch " + epoch);
                        break;
                    }
                    prophet.UpdateWeights(tValues, learnRate, momentum);
                    ++epoch;
                } // train loop

                double[] finalWeights = prophet.GetWeights();
                Console.WriteLine("");
                Console.WriteLine("Final neural network weights and bias values are:");
                ShowVector(finalWeights, 5, 8, true);

                yValues = prophet.ComputeOutputs(xValues);
                Console.WriteLine("\nThe yValues using final weights are:");
                ShowVector(yValues, 8, 8, true);

                double finalError = ErrorMargin(tValues, yValues);
                Console.WriteLine("\nThe final error is " + finalError.ToString("F8"));
            
                yValues = prophet.ComputeOutputs(xValues);
                Console.WriteLine("\nThe yValues using final weights are:");
                ShowVector(yValues, 8, 8, true);

                finalError = ErrorMargin(tValues, yValues);
                Console.WriteLine("\nThe final error is " + finalError.ToString("F8"));

                Console.WriteLine("\nEnd Neural Network Back-Propagation demo\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal: " + ex.Message);
                Console.ReadLine();
            }
        }

        public static void ShowVector(double[] vector, int decimals, int valsPerLine, bool blankLine)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                if (i > 0 && i % valsPerLine == 0) // max of 12 values per row 
                    Console.WriteLine("");
                if (vector[i] >= 0.0) Console.Write(" ");
                Console.Write(vector[i].ToString("F" + decimals) + " "); // n decimals
            }
            if (blankLine) Console.WriteLine("\n");
        }

        public static void ShowMattab(double[][] matrix, int numRows, int decimals)
        {
            int ct = 0;
            if (numRows == -1) numRows = int.MaxValue; // if numRows == -1, show all rows
            for (int i = 0; i < matrix.Length && ct < numRows; ++i)
            {
                for (int j = 0; j < matrix[0].Length; ++j)
                {
                    if (matrix[i][j] >= 0.0) Console.Write(" "); // blank space instead of '+' sign
                    Console.Write(matrix[i][j].ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
                ++ct;
            }
            Console.WriteLine("");
        }

        public static double ErrorMargin(double[] tValues, double[] yValues)
        {
            double sum = 0.0;
            for (int i = 0; i < tValues.Length; ++i)
                sum += (tValues[i] - yValues[i]) * (tValues[i] - yValues[i]);
            return Math.Sqrt(sum);
        }
    }
}
