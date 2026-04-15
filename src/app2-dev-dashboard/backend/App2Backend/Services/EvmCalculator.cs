namespace App2Backend.Services;

public record EvmMetrics(
    decimal PlannedValue,
    decimal EarnedValue,
    decimal ActualCost,
    decimal Budget,
    decimal Cpi,
    decimal Spi,
    decimal Eac,
    decimal Vac,
    decimal ScheduleVariance,
    decimal CostVariance
);

public static class EvmCalculator
{
    /// <summary>
    /// EVM指標を計算します。
    /// </summary>
    /// <param name="budget">BAC (Budget at Completion)</param>
    /// <param name="plannedProgressRate">計画進捗率 (0〜1)</param>
    /// <param name="actualProgressRate">実績進捗率 (0〜1)</param>
    /// <param name="actualCost">実績原価 (AC)</param>
    public static EvmMetrics Calculate(
        decimal budget,
        decimal plannedProgressRate,
        decimal actualProgressRate,
        decimal actualCost)
    {
        var pv = Math.Round(budget * plannedProgressRate, 2);
        var ev = Math.Round(budget * actualProgressRate, 2);
        var ac = actualCost;

        var cpi = ac > 0 ? Math.Round(ev / ac, 4) : 1m;
        var spi = pv > 0 ? Math.Round(ev / pv, 4) : 1m;
        var eac = cpi > 0 ? Math.Round(budget / cpi, 2) : budget;
        var vac = Math.Round(budget - eac, 2);
        var sv  = Math.Round(ev - pv, 2);
        var cv  = Math.Round(ev - ac, 2);

        return new EvmMetrics(pv, ev, ac, budget, cpi, spi, eac, vac, sv, cv);
    }
}
