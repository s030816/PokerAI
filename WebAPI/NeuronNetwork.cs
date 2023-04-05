using Microsoft.AspNetCore.HttpOverrides;
using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using MongoDB.Bson;

namespace WebAPI
{
    public class NeuronNetwork
    {

        public void test(ref double[][] inputs,ref double[][] outputs)
        {


            // Create the neural network
            var network = new ActivationNetwork(
                new SigmoidFunction(), // Activation function
                2, // Input neurons
                4, // Hidden neurons
                1); // Output neurons

            // Create the backpropagation learning algorithm
            var teacher = new BackPropagationLearning(network);

            // Train the neural network
            double error = double.PositiveInfinity;
            while (error > 0.1)
            {
                error = teacher.RunEpoch(inputs, outputs);
                Console.WriteLine(error);
            }
            //var json = network.ToJson();
            // Deserialize the network from JSON
            //var network = ActivationNetwork.FromJson(json);

            // Test the neural network
            double[] testInput = { 1, 0 };
            double[] output = network.Compute(testInput);
            Console.WriteLine(output[0]); // Expected output is 1
        }
    }
}
