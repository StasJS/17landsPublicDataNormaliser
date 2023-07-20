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

        public ILookup<Card, int> UniqueCards => Picks.GroupBy(p => p.Selection).ToLookup(g => g.Key, g => g.Count());
    }

    public record class Pack(ISet<Card> Cards)
    {
        public int Distance(Pack other)
        {
            if (Cards.Count != other.Cards.Count)
            {
                throw new Exception("TODO");
            }
            return Cards.Count - Cards.Intersect(other.Cards).Count();
        }
    }

    public record struct Pick(int PackNumber, int PickNumber, Pack Pack, Card Selection);

    public record struct Card(string CardName);

}
