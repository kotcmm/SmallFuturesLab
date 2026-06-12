namespace SmallFuturesLab.Core.Selection;

public interface IInstrumentCollector
{
    IReadOnlyList<InstrumentRawInfo> Collect();
}
