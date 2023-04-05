using Microsoft.AspNetCore.HttpOverrides;
using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using MongoDB.Bson;

namespace WebAPI
{
    public class NeuronNetwork
    {

        public string test(ref double[][] inputs,ref double[][] outputs)
        {


            // Create the neural network
            var network = new ActivationNetwork(
                new SigmoidFunction(), // Activation function
                2, // Input neurons
                5, // Hidden neurons
                1); // Output neurons

            // Create the backpropagation learning algorithm
            var teacher = new BackPropagationLearning(network);

            // Train the neural network
            double error = double.PositiveInfinity;
            int i = 10000;
            string debug = "";
            while (i-- > 0)
            {
                error = teacher.RunEpoch(inputs, outputs);
                //debug += error.ToString() + " ";
            }
            return error.ToString();
            // Deserialize the network from JSON
            //var network = ActivationNetwork.FromJson(json);

            // Test the neural network
            //double[] testInput = { 1, 0 };
            //double[] output = network.Compute(testInput);
            //Console.WriteLine(output[0]); // Expected output is 1
        }
    }
}
