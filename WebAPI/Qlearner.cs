

namespace WebAPI
{
    public class Qlearner
    {
        private double[,]? qTable;
        private Random random = new Random();
        private double learningRate;
        private double discountFactor;
        private double randomization;

        public Qlearner(int states, int actions,
            double learningRate, double discountFactor, double randomization)
        {
            this.learningRate = learningRate;
            this.discountFactor = discountFactor;
            this.randomization = randomization;
            this.qTable = new double[actions, states];

        }

        public Tuple<int, double> GetMaxQValueForState(int state)
        {
            double maxQValue = double.MinValue;
            int index = -1;
            for (int i = 0; i < qTable.GetLength(0); i++)
            {
                double qValue = qTable[i, state];
                if (qValue > maxQValue)
                {
                    index = i;
                    maxQValue = qValue;
                }
            }
            return new Tuple<int, double>(index, maxQValue);
        }

        public int GetAction(int state)
        {
            if (random.NextDouble() < randomization)
            {
                // Take a random action with probability randomization
                return random.Next(qTable.GetLength(0));
            }
            else
            {
                // Take the action with highest Q value
                return GetMaxQValueForState(state).Item1;
            }
        }

        public void UpdateQValue(int state, int action, int nextState, double reward)
        {
            double oldValue = qTable[action, state];

            double newValue = oldValue + learningRate * (reward + discountFactor * GetMaxQValueForState(nextState).Item2 - oldValue);
            qTable[action, state] = newValue;
        }


    }
}
