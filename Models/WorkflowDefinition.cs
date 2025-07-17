namespace Infonetica.WorkflowEngine.Models;

/// <summary>
/// Definition of the structure of a workflow, containing all possible states and actions
/// </summary>
public record WorkflowDefinition(
    string Id,
    string Name,
    string? Description, // additional
    List<State> States,
    List<Action> Actions
);