using Accord.Neuro;
using Accord.Math;
using Accord.Neuro.Learning;
using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace WebAPI
{
    public class NeuronNetwork
    {
        private ActivationNetwork? network_;
        private string? collname_;
        public double error = double.PositiveInfinity;
        public NeuronNetwork(string gameState, int inputN, int hiddenN, int outputN)
        {
            collname_= gameState;
            //mongodb_import();
            if(network_==null)
            {
                network_ = new ActivationNetwork(
                //new GaussianFunction(), // Activation function
                new SigmoidFunction(),
                inputN, // Input neurons
                hiddenN, // Hidden neurons
                outputN); // Output neurons
            }

        }
        public double train(double[][] inputs,ref double[][] outputs, int iteration)
        {
            // Create the backpropagation learning algorithm

            var teacher = new ResilientBackpropagationLearning(network_);

            while (iteration-- > 0)
            {
                error = teacher.RunEpoch(inputs, outputs);
            }
            //mongodb_export();
            return error;

        }
        // TODO: save and extract data to mongodb, implement continous training
        public double predict(double[] testInput)
        {
            return network_.Compute(testInput)[0];
        }
        public void mongodb_export()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("CardGame");
            var json = JsonConvert.SerializeObject(network_, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            }); ;
            var document = new BsonDocument
            {
                { "network", BsonString.Create(json) }
            };
            var collection = database.GetCollection<BsonDocument>(collname_); 
            collection.InsertOne(document); 
        }
        public void mongodb_import()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("CardGame");
            var collection = database.GetCollection<BsonDocument>(collname_);
            var document = collection.Find(new BsonDocument()).FirstOrDefault();
            if (document != null)
            {
                var tmp = document.GetValue("network").AsString;
                System.Diagnostics.Debug.WriteLine(tmp);
                try
                {
                    network_ = JsonConvert.DeserializeObject<ActivationNetwork>
                    (tmp, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error during deserialization: " + ex.Message);
                    // Additional debug information
                    if (network_ == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: deserialized network is null");
                    }
                    else if (network_.Layers == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: deserialized network Layers is null");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Network successfully deserialized");
                    }
                }
            }
            else network_ = null;
        }
    }
}
