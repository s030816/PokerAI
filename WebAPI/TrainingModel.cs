using Accord.MachineLearning;

namespace WebAPI
{
    public class TrainingModel
    {
        public double[]? hand_vector_preflop_;
        public double[]? hand_vector_flop_;
        public double[]? hand_vector_turn_;
        public double[]? hand_vector_river_;
        /*
        public NeuralNetwork? pre_flop_;
        public NeuralNetwork? flop_train_;
        public NeuralNetwork? turn_train_;
        public NeuralNetwork? river_train_;
        */
        public List<NeuralNetwork>? nn_list_;

        public List<Qlearner>? ql_list_;


        public List<double[][]> inputs = new List<double[][]>();
        public double[][]? outputs;
        private int data_size;
        private int iterations;
        private int neuron_c1;
        private int neuron_c2;

        public TrainingModel(int data_size, int iterations, int neuron_c1, int neuron_c2)
        {
            this.data_size = data_size;
            this.iterations = iterations;
            this.neuron_c1= neuron_c1;
            this.neuron_c2= neuron_c2;
            for (var i = 0; i < 4; ++i) inputs.Add(new double[data_size][]);
            outputs = new double[data_size][];
        }

        public void init_vectors(ref List<int> vector, int winner, int index)
        {
            hand_vector_preflop_ = new double[52];
            hand_vector_flop_ = new double[52];
            hand_vector_turn_ = new double[52];
            hand_vector_river_ = new double[52];

            for (var i = 0; i < vector.Count; ++i)
            {
                // Case opponent hand
                if (i < 2) hand_vector_preflop_[vector[i]] = 1.0;
                // Case flop
                if (i < 2 + 3) hand_vector_flop_[vector[i]] = 1.0;
                // Case turn
                if (i < 2 + 4) hand_vector_turn_[vector[i]] = 1.0;

                hand_vector_river_[vector[i]] = 1.0;
            }
            this.train_step(winner, index);
        }

        private void train_step(int winner, int index)
        {

            inputs[0][index] = hand_vector_preflop_;
            inputs[1][index] = hand_vector_flop_;
            inputs[2][index] = hand_vector_turn_;
            inputs[3][index] = hand_vector_river_;
            outputs[index] = new double[1];
            outputs[index][0] = winner;
        }

        public string train_model()
        {
            ql_list_ = new List<Qlearner>();
            for (var i = 0; i < 4; ++i)
            {
                ql_list_.Add(new Qlearner(
    states: 10,
    actions: 3,
    learningRate: 0.5,
    discountFactor: 0.7, // 0 - accute, 1 - long term goal
    randomization: 0.5));
            }
            nn_list_ = new List<NeuralNetwork>
            {
                new NeuralNetwork("preflop", 52, neuron_c1, 1),
                new NeuralNetwork("flop", 52, neuron_c2, 1),
                new NeuralNetwork("turn", 52, neuron_c2, 1),
                new NeuralNetwork("river", 52, neuron_c2, 1)
            };

            // TODO: Check  inputs
            //System.Diagnostics.Debug.WriteLine("Starting................................");
            var pre = nn_list_[0].train(inputs[0], ref outputs, iterations).ToString();
            var flop = nn_list_[1].train(inputs[1], ref outputs, iterations).ToString();
            var turn = nn_list_[2].train(inputs[2], ref outputs, iterations).ToString();
            var river = nn_list_[3].train(inputs[3], ref outputs, iterations).ToString();
            return pre + " " + flop + " " + turn + " " + river;
        }
    }
}
