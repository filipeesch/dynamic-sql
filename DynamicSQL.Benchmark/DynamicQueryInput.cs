namespace DynamicSQL.Benchmark;

public record DynamicQueryInput(
    bool IncludeName,
    bool IncludeBirthDate,
    IEnumerable<int>? Ids,
    int? Count);
