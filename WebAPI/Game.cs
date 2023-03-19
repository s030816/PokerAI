using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using System.Diagnostics;
using WebAPI.Models;

namespace WebAPI
{
    public class Game
    {
        public List<string> cards_ = new List<string>();

        private ulong flush_ =              0b000000001;
        private ulong straight_ =           0b000000010;
        private ulong four_of_a_kind_ =     0b000000100;
        private ulong three_of_a_kind_ =    0b000001000;
        private ulong two_pairs_ =          0b000010000;
        private ulong one_pair_ =           0b000100000;
        private ulong full_house_ =         0b001000000;
        private ulong straight_flush_ =     0b010000000;
        private ulong royal_flush_ =        0b100000000;

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



        // A K K Q J 10 2
        // A K Q J 10 2
        private void mark_hands(List<Tuple<int, int>> deck, ref ulong flags, ref List<Tuple<int,int>> count_deck)
        {
            var tmp_deck = new List<int>();
            for(var i = 0; i < 7; ++i) 
            {
                tmp_deck.Add(deck[i].Item2);
            }

            count_deck = tmp_deck.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => new Tuple<int,int>(y.Key, y.Count() ))
              .ToList();

            foreach(var item in count_deck)
            {
                if (item.Item2 == 4) flags |= four_of_a_kind_;
                if (item.Item2 == 3) 
                {
                    if ((flags & one_pair_) == one_pair_) flags |= full_house_;
                    flags |= three_of_a_kind_;
                } 
                if (item.Item2 == 2) 
                {
                    if((flags & one_pair_) == one_pair_) flags |= two_pairs_;
                    if((flags & three_of_a_kind_) == three_of_a_kind_) flags |= full_house_;
                    flags |= one_pair_;
                } 
            }

            tmp_deck = tmp_deck.Distinct().ToList();
            if (tmp_deck.Count < 5) return;
            tmp_deck.Sort((a, b) => b.CompareTo(a));
            int high_card = 0;
            this.is_flush(ref deck, ref flags, ref high_card);

            // Implement special case "the wheel"
            // Straight flush logic is broken. Check if all cards are of the same type
            for (var j = 0; j < tmp_deck.Count - 4; ++j)
            {
                if (tmp_deck[j] == tmp_deck[j + 4] + 4) 
                {
                    if (tmp_deck[j] == high_card)
                    {
                        flags |= straight_flush_;
                        if(high_card == 14) flags |= royal_flush_;
                    }
                    
                    flags |= straight_;
                    return;
                }
            }
            

        }
        private void is_flush(ref List<Tuple<int, int>> deck, ref ulong flags, ref int high_card)
        {
            int[] tmp_deck = new int[4];

            for (var i = 0; i < 7; ++i)
            {
                ++tmp_deck[deck[i].Item1];
            }
            for(var i = 0; i < 4; ++i)
            {
                if (tmp_deck[i] >= 5)
                {
                    foreach(var j in deck)
                    {
                        if(j.Item1 == i && j.Item2 > high_card) high_card = j.Item2;
                    }
                    flags |= flush_;
                    return;
                } 
            }
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
            var dec_cpy = new List<Tuple<int, int>>(extracted_deck);
            extract(phand,dec_cpy);
            extract(ohand,extracted_deck);

            /*
            dec_cpy.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
            {
                if (x.Item2 == y.Item2) return 0;
                else if (x.Item2 < y.Item2) return 1;
                else return -1;
            });
            extracted_deck.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
            {
                if (x.Item2 == y.Item2) return 0;
                else if (x.Item2 < y.Item2) return 1;
                else return -1;
            });
            */

            return new Tuple<List<Tuple<int, int>>, List<Tuple<int, int>>>(dec_cpy,extracted_deck);
        }

        /**
         * TODO:
         * Implement winning hand export
         * Implement high card counter
         * Implement The wheel straingt
         * Straight flush logic is broken. 
         */

        public int check_winner(GameState current)
        {
            var decs = this.extract_card(current.deck, current.player_hand, current.opponent_hand);
            if (current.state != 5) return -1;

            foreach (var i in decs.Item1)
            {
                System.Diagnostics.Debug.Write(String.Format("{0}-{1};",i.Item1.ToString(), i.Item2.ToString()));
            }

            System.Diagnostics.Debug.WriteLine("");
            foreach (var i in decs.Item2)
            {
                System.Diagnostics.Debug.Write(String.Format("{0}-{1};", i.Item1.ToString(), i.Item2.ToString()));
            }
            System.Diagnostics.Debug.WriteLine("");

            ulong op_flags = 0;
            ulong pl_flags = 0;
            var counting_deck = new List<Tuple<int, int>>();
            var counting_deck_op = new List<Tuple<int, int>>();

            mark_hands(decs.Item1, ref pl_flags,ref counting_deck);
            mark_hands(decs.Item2, ref op_flags, ref counting_deck_op);

            Debug.WriteLine(Convert.ToString((long)pl_flags, 2));
            Debug.WriteLine(Convert.ToString((long)op_flags, 2));
            var tmp_pl = new List<string>();
            var tmp_op = new List<string>();


            // Royal Flush
            if ((pl_flags & royal_flush_) == royal_flush_) tmp_pl.Add("Royal Flush");
            if ((op_flags & royal_flush_) == royal_flush_) tmp_op.Add("Royal Flush");
            // Straight flush
            if ((pl_flags & straight_flush_) == straight_flush_) tmp_pl.Add("Straight flush");
            if ((op_flags & straight_flush_) == straight_flush_) tmp_op.Add("Straight flush");
            // Four of a kind
            if ((pl_flags & four_of_a_kind_) == four_of_a_kind_) tmp_pl.Add("Four of a kind");
            if ((op_flags & four_of_a_kind_) == four_of_a_kind_) tmp_op.Add("Four of a kind");
            // Full house
            if ((pl_flags & full_house_) == full_house_) tmp_pl.Add("Full house");
            if ((op_flags & full_house_) == full_house_) tmp_op.Add("Full house");
            // Flush
            if ((pl_flags & flush_) == flush_) tmp_pl.Add("Flush");
            if ((op_flags & flush_) == flush_) tmp_op.Add("Flush");
            // Straight
            if ((pl_flags & straight_) == straight_) tmp_pl.Add("Straight");
            if ((op_flags & straight_) == straight_) tmp_op.Add("Straight");
            // Three of a kind
            if ((pl_flags & three_of_a_kind_) == three_of_a_kind_) tmp_pl.Add("Three of a kind");
            if ((op_flags & three_of_a_kind_) == three_of_a_kind_) tmp_op.Add("Three of a kind");
            // Two Pair
            if ((pl_flags & two_pairs_) == two_pairs_) tmp_pl.Add("Two Pair");
            if ((op_flags & two_pairs_) == two_pairs_) tmp_op.Add("Two Pair");
            // Pair
            if ((pl_flags & one_pair_) == one_pair_) tmp_pl.Add("One Pair");
            if ((op_flags & one_pair_) == one_pair_) tmp_op.Add("One Pair");
            // High card
            /*
            for (var i = 0; i < 5; ++i)
            {
                if (decs.Item1[i].Item2 > decs.Item2[i].Item2) return 1;
                else if (decs.Item1[i].Item2 < decs.Item2[i].Item2) return 2;
            }
            */
            Debug.Write("Player: ");
            foreach(var i in tmp_pl)
                Debug.Write("{0};",i);
            Debug.Write("\nOpponent: ");
            foreach (var i in tmp_op)
                Debug.Write("{0};", i);
            Debug.WriteLine("");

            if ((pl_flags & straight_flush_) == straight_flush_) return 1;
            if ((op_flags & straight_flush_) == straight_flush_) return 1;
            return 0;
        }

    }
}
