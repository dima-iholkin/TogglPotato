using System.Diagnostics.Metrics;

namespace TogglPotato.WebAPI.Observability;

public class Metrics
{
    public Metrics(Meter meter)
    {
        timeEntriesModified = meter.CreateCounter<int>(
            "timeentries_modified.count",
            description: "Count the number of time entries modified."
        );
        timeEntriesNotModified = meter.CreateCounter<int>(
            "timeentries_not_modified.count",
            description: "Count the number of time entries not requiring modification and left in the present state."
        );
    }

    public Counter<int> timeEntriesModified { get; init; }

    public Counter<int> timeEntriesNotModified { get; init; }
}