using Accord.Neuro;
using Accord.Math;
using Accord.Neuro.Learning;
using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace WebAPI
{
    public class NeuronNetwork
    {
        private ActivationNetwork? network_;
        private string? collname_;
        public double error = double.PositiveInfinity;
        public NeuronNetwork(string gameState)
        {
            collname_= gameState;
            mongodb_import();
            if(network_==null)
            {
                network_ = new ActivationNetwork(
                //new GaussianFunction(), // Activation function
                new SigmoidFunction(),
                2, // Input neurons
                10, // Hidden neurons
                1); // Output neurons
            }

        }
        public double train(ref double[][] inputs,ref double[][] outputs)
        {

            // Create the backpropagation learning algorithm
            BackPropagationLearning teacher = new BackPropagationLearning(network_);

            // Train the neural network
            int i = 1000;

            while (i-- > 0)
            {
                error = teacher.RunEpoch(inputs, outputs);
                
            }
            mongodb_export();
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
            var json = JsonConvert.SerializeObject(network_, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
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
                network_ = JsonConvert.DeserializeObject<ActivationNetwork>
                (document.GetValue("network").AsString, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
            }
            else network_ = null;
        }
    }
}
