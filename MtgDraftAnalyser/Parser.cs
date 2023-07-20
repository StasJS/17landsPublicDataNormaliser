using System.Collections.Immutable;
using System.Data;
using Model;
using Sylvan.Data.Csv;

namespace IO;
public static class DraftCsvParser
{
    public static IList<DraftRow> Parse(string fullPath)
    {
        using CsvDataReader parser = CsvDataReader.Create(fullPath, new CsvDataReaderOptions
        {
            BufferSize = 20000
        });

        var columnIndexToProperty = new Dictionary<int, string>();
        var columnIndexToCardName = new Dictionary<int, string>();
        foreach (var columnData in parser.GetColumnSchema())
        {
            var columnName = columnData.ColumnName;
            if (columnName.StartsWith("pack_card_"))
            {
                var cardName = columnName.Replace("pack_card_", "");
                columnIndexToCardName.Add(columnData.ColumnOrdinal!.Value, cardName);
            }
            else if (DraftRow.CsvProperties.Contains(columnName))
            {
                columnIndexToProperty.Add(columnData.ColumnOrdinal!.Value, columnName);
            }
        }

        var parsedRows = new List<DraftRow>();

        while (parser.Read())
        {
            var cardsInPack = new HashSet<string>();
            var propertyValues = new Dictionary<string, string>();
            for (int i = 0; i < parser.RowFieldCount; i++)
            {
                if (parser.RowFieldCount != parser.FieldCount)
                {
                    throw new Exception($"Row #{parser.RowNumber}: RowFieldCount={parser.RowFieldCount}, but FieldCount={parser.FieldCount}");
                }
                var cell = parser.GetString(i);
                if (columnIndexToProperty.TryGetValue(i, out var property))
                {
                    propertyValues.Add(property, cell);
                }
                else if (columnIndexToCardName.TryGetValue(i, out var cardName) && cell == "1")
                {
                    cardsInPack.Add(cardName);
                }
            }
            var draftRow = new DraftRow(
                DraftId: propertyValues["draft_id"],
                DraftTime: DateTime.Parse(propertyValues["draft_time"]),
                Rank: propertyValues["rank"],
                EventMatchWins: int.Parse(propertyValues["event_match_wins"]),
                EventMatchLosses: int.Parse(propertyValues["event_match_losses"]),
                PackNumber: int.Parse(propertyValues["pack_number"]),
                PickNumber: int.Parse(propertyValues["pick_number"]),
                Pick: propertyValues["pick"],
                PickMaindeckRate: decimal.Parse(propertyValues["pick_maindeck_rate"]),
                PickSideboardInRate: decimal.Parse(propertyValues["pick_sideboard_in_rate"]),
                Pack: cardsInPack);

            parsedRows.Add(draftRow);

            if (parser.RowNumber % 100000 == 0)
            {
                Console.WriteLine($"Parsed {parser.RowNumber}");
            }
        }
        return parsedRows;
    }
}

public record DraftRow(
    string DraftId,
    DateTime DraftTime,
    string Rank,
    int EventMatchWins,
    int EventMatchLosses,
    int PackNumber,
    int PickNumber,
    string Pick,
    decimal PickMaindeckRate,
    decimal PickSideboardInRate,
    ISet<string> Pack
)
{
    public static ISet<string> CsvProperties => new[] {
            "draft_id",
            "draft_time",
            "rank",
            "event_match_wins",
            "event_match_losses",
            "pack_number",
            "pick_number",
            "pick",
            "pick_maindeck_rate",
            "pick_sideboard_in_rate"
        }.ToHashSet();

    public static Draft ToDraftModel(string draftId, IReadOnlyList<DraftRow> draftRows)
    {
        var picks = new List<Pick>();
        DraftResult? draftResult = null;
        DateTime? draftTime = null;
        foreach (var draftRow in draftRows.OrderBy(row => row.PackNumber).ThenBy(row => row.PickNumber))
        {
            draftResult ??= new DraftResult(draftRow.EventMatchWins, draftRow.EventMatchLosses);
            draftTime ??= draftRow.DraftTime;

            var cards = draftRow.Pack.Select(entry => new Card(entry)).ToImmutableHashSet();
            var pick = new Pick(draftRow.PackNumber, draftRow.PickNumber, new Pack(cards), new Card(draftRow.Pick));
            picks.Add(pick);
        }
        var draft = new Draft(draftId, draftTime!.Value, new Pool(picks), draftResult!.Value);
        return draft;
    }
}