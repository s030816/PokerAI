using Microsoft.AspNetCore.HttpOverrides;
using Accord.Neuro;
using Accord.Math;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using MongoDB.Bson;
using Newtonsoft.Json;
using Accord;

namespace WebAPI
{
    public class NeuronNetwork
    {
        private ActivationNetwork network_;
        private BackPropagationLearning teacher_;
        public string test(ref double[][] inputs,ref double[][] outputs)
        {


            // Create the neural network
            network_ = new ActivationNetwork(
                //new GaussianFunction(), // Activation function
                new SigmoidFunction(),
                2, // Input neurons
                10, // Hidden neurons
                1); // Output neurons

            // Create the backpropagation learning algorithm
            teacher_ = new BackPropagationLearning(network_);

            // Train the neural network
            double error = double.PositiveInfinity;
            int i = 1000;
            string debug = "";
            while (i-- > 0)
            {
                error = teacher_.RunEpoch(inputs, outputs);
                
            }
            System.Diagnostics.Debug.WriteLine(error.ToString());
            string json = JsonConvert.SerializeObject(network_);
            return json;
            // // Deserialize the JSON string to a neural network object
            //ActivationNetwork deserializedNetwork = JsonConvert.DeserializeObject<ActivationNetwork>(json);

        }
        // TODO: save and extract data to mongodb, implement continous training
        public double predict(double[] testInput)
        {
            return network_.Compute(testInput)[0];
        }
    }
}
