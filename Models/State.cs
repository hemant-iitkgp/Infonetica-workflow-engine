namespace Infonetica.WorkflowEngine.Models;

/// <summary>
/// Represents a single state in a workflow
/// Using value-based equality
/// </summary>
public record State(
    string Id,
    string Name,
    string? Description, //additional
    bool IsInitial,
    bool IsFinal,
    bool Enabled
);