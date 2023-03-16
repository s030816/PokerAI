using Amazon.Auth.AccessControlPolicy;
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
            var deck = this.get_random_cards(9);
            state.player_hand = new List<string>();
            state.opponent_hand = new List<string>();
            state.deck = new List<string>();
            state.player_hand.Add(deck[0]);
            state.player_hand.Add(deck[1]);
            state.opponent_hand.Add(deck[2]);
            state.opponent_hand.Add(deck[3]);
            for (var i = 4; i < 9; ++i)
            {
                state.deck.Add(deck[i]);
            }
            state.opponent_bank = 150;
            state.player_bank = 150;
            return state;
        }

        public GameState advance_ingame(GameState current)
        {
            switch(current.state)
            {
                case 0: 
                    current.state = 3;
                    break;
                case 3:
                    current.state = 4;
                    break;
                case 4:
                    current.state = 5;
                    break;
                case 5:
                    current.state = 0;
                    break;
                default:
                    return null;
            }
            return current;
        }

        private Tuple<List<Tuple<int,int>>, List<Tuple<int, int>>> extract_card(List<string> deck, List<string> phand, List<string> ohand)
        {
            var extracted_deck = new List<Tuple<int,int>>();
            Action<List<string>, List<Tuple<int, int>>> extract = (deckx,ext) => 
            {
                foreach (var card in deckx)
                {
                    var parts = card.Split('-');
                    if (parts[1] == "1") parts[1] = "14";
                    ext.Add(new Tuple<int, int>(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1])));
                }
            };

            extract(deck,extracted_deck);
            var dec_cpy = extracted_deck;
            extract(phand,dec_cpy);
            extract(ohand,extracted_deck);


            dec_cpy.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
            {
                if (x.Item2 == y.Item2) return 0;
                else if (x.Item2 < y.Item2) return -1;
                else return 1;
            });
            extracted_deck.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
            {
                if (x.Item2 == y.Item2) return 0;
                else if (x.Item2 < y.Item2) return -1;
                else return 1;
            });


            return new Tuple<List<Tuple<int, int>>, List<Tuple<int, int>>>(dec_cpy,extracted_deck);
        }

        public int check_winner(GameState current)
        {
            var decs = this.extract_card(current.deck, current.player_hand, current.opponent_hand);
            if (current.state != 5) return -1;

            // Royal Flush

        }

    }
}
