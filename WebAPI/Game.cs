using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using System;
using System.Diagnostics;
using WebAPI.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WebAPI
{
    public class Game
    {
        public List<string> cards_ = new List<string>();
        public string winning_combination = "High Card";
        private double[]? hand_vector_preflop_;
        private double[]? hand_vector_flop_;
        private double[]? hand_vector_turn_;
        private double[]? hand_vector_river_;

        public static NeuronNetwork? pre_flop_;
        public static NeuronNetwork? flop_train_;
        public static NeuronNetwork? turn_train_;
        public static NeuronNetwork? river_train_;




        private ulong flush_ =              0b000000001;
        private ulong straight_ =           0b000000010;
        private ulong four_of_a_kind_ =     0b000000100;
        private ulong three_of_a_kind_ =    0b000001000;
        private ulong two_pairs_ =          0b000010000;
        private ulong one_pair_ =           0b000100000;
        private ulong full_house_ =         0b001000000;
        private ulong straight_flush_ =     0b010000000;
        private ulong royal_flush_ =        0b100000000;

        private Tuple<int,int> high_card_;


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

            hand_vector_preflop_ = new double[52];
            hand_vector_flop_ = new double[52];
            hand_vector_turn_ = new double[52];
            hand_vector_river_ = new double[52];

            for(int i = 0; i < list_size;++i)
            {
                int tmp = -1;
                while(indexes.Contains(tmp = rnd.Next(52)));
                indexes.Add(tmp);

                if (i > 1)
                {
                    // Case opponent hand
                    if(i < 4) hand_vector_preflop_[tmp] = 1.0;
                    // Case flop
                    if (i < 4 + 3) hand_vector_flop_[tmp] = 1.0;
                    // Case turn
                    if (i < 4 + 4) hand_vector_turn_[tmp] = 1.0;

                    hand_vector_river_[tmp] = 1.0;
                }
                cards.Add(cards_[tmp]);
            }

            return cards;
        }

        public GameState new_game()
        {
            var state = new GameState();
            var deck = this.get_random_cards(9);
            state.state = 0;
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
            state.winning_hand = "None";
            state.who_won = -1;
            return state;
        }

        private double[] mark_vector(List<string> deck, List<string> hand, int state)
        {
            var returns = new double[52];
            returns[cards_.IndexOf(hand[0])] = 1;
            returns[cards_.IndexOf(hand[1])] = 1;
            if (deck != null)
            {
                for (var i = 0; i < state; ++i)
                {
                    returns[cards_.IndexOf(deck[i])] = 1;
                }
            }
            return returns;
        }

        public string make_decision(GameState current)
        {
            var inputs = this.mark_vector(current.deck,current.opponent_hand, (int)current.state);
            switch (current.state)
            {
                case 0:
                    return pre_flop_.predict(inputs).ToString();
                case 3:
                    return flop_train_.predict(inputs).ToString();
                case 4:
                    return turn_train_.predict(inputs).ToString();
                case 5:
                    return river_train_.predict(inputs).ToString();
                default:
                    return null;
            }
        }

        public string simulate(int data_size, int iterations, int neuron_c1, int neuron_c2)
        {
            var inputs = new List<double[][]>();
            for(var i = 0; i < 4; ++i) inputs.Add(new double[data_size][]);

            double[][] outputs = new double[data_size][];

            for (var i = 0; i < data_size; ++i)
            {
                var tmp = this.new_game();

                inputs[0][i] = hand_vector_preflop_;
                inputs[1][i] = hand_vector_flop_;
                inputs[2][i] = hand_vector_turn_;
                inputs[3][i] = hand_vector_river_;

                //.Copy(hand_vector_, inputs_flop[i], hand_vector_.Length);
                outputs[i] = new double[1];

  
                outputs[i][0] = this.check_winner(tmp) == 2? 1:0;
                //System.Diagnostics.Debug.WriteLine(String.Format("{0} {1} - {2}", inputs[i][0], inputs[i][1], outputs[i][0]));
            }
            pre_flop_ = new NeuronNetwork("preflop",52, neuron_c1, 1);
            flop_train_ = new NeuronNetwork("flop", 52, neuron_c2, 1);
            turn_train_ = new NeuronNetwork("turn", 52, neuron_c2, 1);
            river_train_ = new NeuronNetwork("river", 52, neuron_c2, 1);

            // TODO: Check  inputs
            System.Diagnostics.Debug.WriteLine("Starting................................");
            var pre = pre_flop_.train(inputs[0], ref outputs, iterations).ToString();
            var flop = flop_train_.train(inputs[1], ref outputs, iterations).ToString();
            var turn = turn_train_.train(inputs[2], ref outputs, iterations).ToString();
            var river = river_train_.train(inputs[3], ref outputs, iterations).ToString();
            return pre + " " + flop + " " + turn + " " + river;

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
                    current.who_won = this.check_winner(current);
                    current.winning_hand = this.winning_combination;
                    current.state = 5;
                    break;
                case 5:
                    var tmp = current._id;
                    current = this.new_game();
                    current._id= tmp;
                    break;
                default:
                    return null;
            }
            return current;
        }



        private void mark_hands(List<Tuple<int, int>> deck, ref ulong flags, 
            ref List<Tuple<int,int>> count_deck)
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

            var flush_hand = this.is_flush(ref deck, ref flags);


            for (var j = 0; j < tmp_deck.Count - 4; ++j)
            {
                if (tmp_deck[j] == tmp_deck[j + 4] + 4) 
                {
                    if(flush_hand != null)
                    {
                        var i = 0;
                        for (; i < 5; ++i)
                        {
                            if (tmp_deck[j + i] != flush_hand[i].Item2)
                            {
                                i = -1;
                                break;
                            }
                        }
                        if(i != -1)
                        {
                            flags |= straight_flush_;
                            if (flush_hand[0].Item2 == 14) 
                                flags |= royal_flush_;
                        }
                    }

                    flags |= straight_;
                    return;
                }
                else if (tmp_deck[j+1] == tmp_deck[j + 4] + 3 && tmp_deck[0] == 14 && 
                    tmp_deck[j + 4] == 2)
                {
                    if (flush_hand != null)
                    {
                        var i = 0;
                        for (; i < 4; ++i)
                        {
                            if (tmp_deck[j+1 + i] != flush_hand[1+i].Item2)
                            {
                                i = -1;
                                break;
                            }
                        }
                        if (i != -1) flags |= straight_flush_;
                    }
                    flags |= straight_;
                    return;
                }
            }
            

        }
        private List<Tuple<int, int>> is_flush(ref List<Tuple<int, int>> deck, ref ulong flags)
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
                    List<Tuple<int, int>> flush_hand = new List<Tuple<int, int>>();
                    foreach(var j in deck)
                    {
                        if(j.Item1 == i) flush_hand.Add(j);
                    }
                    flush_hand.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
                    {
                        if (x.Item2 == y.Item2) return 0;
                        else if (x.Item2 < y.Item2) return 1;
                        else return -1;
                    });
                    flags |= flush_;
                    return flush_hand;
                } 
            }
            return null;
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

            return new Tuple<List<Tuple<int, int>>, List<Tuple<int, int>>>(dec_cpy,extracted_deck);
        }

        /**
         * TODO:
         * Implement high card counter
         */
        private int high_card(ref Tuple<List<Tuple<int, int>>, List<Tuple<int, int>>> decs)
        {
            /*
            decs.Item1.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
            {
                if (x.Item2 == y.Item2) return 0;
                else if (x.Item2 < y.Item2) return 1;
                else return -1;
            });
            decs.Item2.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
            {
                if (x.Item2 == y.Item2) return 0;
                else if (x.Item2 < y.Item2) return 1;
                else return -1;
            });
            
            for (var i = 0; i < 5; ++i)
            {
                if (decs.Item1[i].Item2 > decs.Item2[i].Item2) return 1;
                else if (decs.Item1[i].Item2 < decs.Item2[i].Item2) return 2;
            }
            */
            if (decs.Item1[5].Item2 < decs.Item1[6].Item2)
            {
                var tmp = decs.Item1[5];
                decs.Item1[5] = decs.Item1[6];
                decs.Item1[6] = tmp;
            }
            if (decs.Item2[5].Item2 < decs.Item2[6].Item2)
            {
                var tmp = decs.Item2[5];
                decs.Item2[5] = decs.Item2[6];
                decs.Item2[6] = tmp;
            }

            if (decs.Item1[5].Item2 > decs.Item2[5].Item2) return 1;
            else if (decs.Item1[5].Item2 < decs.Item2[5].Item2) return 2;
            if (decs.Item1[6].Item2 > decs.Item2[6].Item2) return 1;
            else if (decs.Item1[6].Item2 < decs.Item2[6].Item2) return 2;
            return 0;
        }

        public int check_winner(GameState current)
        {
            var decs = this.extract_card(current.deck, current.player_hand, current.opponent_hand);
            /*
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
            */
            ulong op_flags = 0;
            ulong pl_flags = 0;
            var counting_deck = new List<Tuple<int, int>>();
            var counting_deck_op = new List<Tuple<int, int>>();

            mark_hands(decs.Item1, ref pl_flags,ref counting_deck);
            mark_hands(decs.Item2, ref op_flags, ref counting_deck_op);

            //Debug.WriteLine(Convert.ToString((long)pl_flags, 2));
            //Debug.WriteLine(Convert.ToString((long)op_flags, 2));



            // Royal Flush
            if ((pl_flags & royal_flush_) == royal_flush_)
            {
                this.winning_combination = "Royal Flush";
                if ((op_flags & royal_flush_) == royal_flush_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & royal_flush_) == royal_flush_) 
            {
                this.winning_combination = "Royal Flush";
                return 2;
            }
            // Straight flush
            // In the event of a tie: Highest rank at the top of the sequence wins.
            if ((pl_flags & straight_flush_) == straight_flush_) 
            {
                this.winning_combination = "Straight flush";
                if ((op_flags & straight_flush_) == straight_flush_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & straight_flush_) == straight_flush_) 
            { 
                this.winning_combination = "Straight flush";
                return 2;
            }
            // Four of a kind
            // In the event of a tie: Highest four of a kind wins
            /*
             In community card games where players have the same four of a kind, 
            the highest fifth side card ('kicker') wins.
             */
            if ((pl_flags & four_of_a_kind_) == four_of_a_kind_)
            {
                this.winning_combination = "Four of a kind";
                if ((op_flags & four_of_a_kind_) == four_of_a_kind_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & four_of_a_kind_) == four_of_a_kind_)
            { 
                this.winning_combination = "Four of a kind";
                return 2;
            }
            // Full house
            // In the event of a tie: Highest three matching cards wins the pot.
            /*
                In community card games where players have the same three matching cards, 
                the highest value of the two matching cards wins.
            */
            if ((pl_flags & full_house_) == full_house_)
            {
                this.winning_combination = "Full house";
                if ((op_flags & full_house_) == full_house_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & full_house_) == full_house_)
            {
                this.winning_combination = "Full house";
                return 2;
            }
            // Flush
            /*
             In the event of a tie: The player holding the highest ranked card wins. 
            If necessary, the second-highest, third-highest, fourth-highest, 
            and fifth-highest cards can be used to break the tie. 
            If all five cards are the same ranks, the pot is split. 
            The suit itself is never used to break a tie in poker.
             */
            if ((pl_flags & flush_) == flush_)
            {
                this.winning_combination = "Flush";
                if ((op_flags & flush_) == flush_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & flush_) == flush_)
            {
                this.winning_combination = "Flush";
                return 2;
            }
            // Straight
            // In the event of a tie: Highest ranking card at the top of the sequence wins.
            if ((pl_flags & straight_) == straight_)
            {
                this.winning_combination = "Straight";
                if ((op_flags & straight_) == straight_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & straight_) == straight_)
            {
                this.winning_combination = "Straight";
                return 2;
            }
            // Three of a kind
            /*
             In the event of a tie: Highest ranking three of a kind wins. 
            In community card games where players have the same three of a kind, 
            the highest side card, and if necessary, the second-highest side card wins.
             */
            if ((pl_flags & three_of_a_kind_) == three_of_a_kind_)
            {
                this.winning_combination = "Three of a kind";
                if ((op_flags & three_of_a_kind_) == three_of_a_kind_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & three_of_a_kind_) == three_of_a_kind_)
            {
                this.winning_combination = "Three of a kind";
                return 2;
            }
            // Two Pair
            /*
                In the event of a tie: Highest pair wins. 
            If players have the same highest pair, highest second pair wins. 
            If both players have two identical pairs, highest side card wins.
                */
            if ((pl_flags & two_pairs_) == two_pairs_)
            {
                this.winning_combination = "Two Pair";
                if ((op_flags & two_pairs_) == two_pairs_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & two_pairs_) == two_pairs_)
            {
                this.winning_combination = "Two Pair";
                return 2;
            }
            // Pair
            /*
             In the event of a tie: Highest pair wins. 
            If players have the same pair, the highest side card wins, and if necessary, 
            the second-highest and third-highest side card can be used to break the tie.
             */
            if ((pl_flags & one_pair_) == one_pair_)
            {
                this.winning_combination = "One Pair";
                if ((op_flags & one_pair_) == one_pair_) return high_card(ref decs);
                return 1;
            }
            else if ((op_flags & one_pair_) == one_pair_)
            {
                this.winning_combination = "One Pair";
                return 2;
            }
            // High card




            return high_card(ref decs);
        }

    }
}
