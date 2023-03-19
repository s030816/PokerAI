
namespace CardGame.Pages.Shared.Components
{
    public record GameState
    {
        public string? _id { get; set; }
        public int? state { get; set; }
        public List<string>? deck { get; set; }
        public List<string>? player_hand { get; set; }
        public List<string>? opponent_hand { get; set; }
        public int opponent_bank { get; set; }
        public int player_bank { get; set; }
        public int opponent_bet { get; set; }
        public int player_bet { get; set; }
        public string? winning_hand { get; set; }
        public int who_won { get; set; }


    }
}
