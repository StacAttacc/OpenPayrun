using OpenPayrun.Domain.Enums;

namespace OpenPayrun.Domain.Entities;

public class TaxBracket
{
    public int Id { get; set; }
    public int TaxRateSetId { get; set; }
    public TaxRateSet TaxRateSet { get; set; } = null!;
    public BracketType BracketType { get; set; }
    public decimal? UpperBound { get; set; } // null = no upper bound (last bracket)
    public decimal Rate { get; set; }
    public int SortOrder { get; set; }
}
