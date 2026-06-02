namespace FinanceiroApi.Domain.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    public int TotalDays => End.DayNumber - Start.DayNumber + 1;

    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.");

        Start = start;
        End = end;
    }

    public static DateRange ForMonth(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new DateRange(start, end);
    }

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public bool Overlaps(DateRange other) => Start <= other.End && End >= other.Start;

    public bool Equals(DateRange? other) =>
        other is not null && Start == other.Start && End == other.End;

    public override bool Equals(object? obj) => Equals(obj as DateRange);
    public override int GetHashCode() => HashCode.Combine(Start, End);
    public override string ToString() => $"{Start:dd/MM/yyyy} - {End:dd/MM/yyyy}";
}
