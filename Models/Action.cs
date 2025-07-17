namespace Infonetica.WorkflowEngine.Models;

/// <summary>
/// Represents a transition from one state to another state(intended)
/// </summary>
public record Action(
    string Id,
    string Name,
    string? Description, // additional
    bool Enabled,
    List<string> FromStates,
    string ToState
);