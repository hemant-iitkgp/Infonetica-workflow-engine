namespace Infonetica.WorkflowEngine.Models;

/// <summary>
/// Represents a running instance of a WorkflowDefinition.
/// This is a class with mutable properties (like CurrentStateId and History)
/// </summary>
public class WorkflowInstance
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DefinitionId { get; init; }
    public string CurrentStateId { get; set; }
    public List<HistoryEntry> History { get; set; } = new();

    // Constructor to initialize the instance at the starting state of a workflow.
    public WorkflowInstance(string definitionId, string initialStateId)
    {
        DefinitionId = definitionId;
        CurrentStateId = initialStateId;
    }
}