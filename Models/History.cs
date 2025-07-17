namespace Infonetica.WorkflowEngine.Models;
/// <summary>
/// Represents the history of tasks done on an instance
/// </summary>
public record HistoryEntry(
    string ActionId,
    string FromStateId,
    string ToStateId,
    DateTime Timestamp
);