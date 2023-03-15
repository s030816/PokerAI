using WebAPI.Models;

namespace WebAPI
{
    public class Game
    {
        public List<string> cards_ = new List<string>();
        public Game() 
        {
            for(var i = 0; i < 4; ++i)
            {
                for(var j = 1; j < 14; ++j) 
                {
                    cards_.Add(String.Format("{0}-{1}",i,j));
                }
            }
            
        }
        public List<string> get_random_cards(int list_size)
        {
            var cards = new List<string>();
            var indexes = new HashSet<int>();
            Random rnd = new Random();
            for(int i = 0; i < list_size;++i)
            {
                int tmp = -1;
                while(indexes.Contains(tmp = rnd.Next(52)));
                indexes.Add(tmp);
                cards.Add(cards_[tmp]);
            }

            return cards;
        }

        public GameState new_game()
        {
            var state = new GameState();
            state.state = 5;
            var deck = this.get_random_cards(7);
            state.player_hand = new List<string>();
            state.deck = new List<string>();
            state.player_hand.Add(deck[0]);
            state.player_hand.Add(deck[1]);
            for(var i = 2; i < 7; ++i)
            {
                state.deck.Add(deck[i]);
            }
            state.opponent_bank = 150;
            state.player_bank = 150;
            return state;
        }
    }
}
