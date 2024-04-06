namespace Model
{
    public record struct DraftResult(int Wins, int Losses)
    {

        public readonly int NetWins => Wins - Losses;
    }
    public record class Draft(string DraftId, DateTime DraftTime, Pool Pool, DraftResult DraftResult);

    public record class Pool(IReadOnlyList<Pick> Picks)
    {

        public IReadOnlyList<Card> Cards => Picks.Select(p => p.Selection).ToList();

        public IReadOnlyDictionary<Card, int> UniqueCards => Picks.GroupBy(p => p.Selection).ToDictionary(g => g.Key, g => g.Count());
    }

    public record class Pack(ISet<Card> Cards)
    {
    }

    public record struct Pick(int PackNumber, int PickNumber, Pack Pack, Card Selection);

    public record struct Card(string CardName);

}
