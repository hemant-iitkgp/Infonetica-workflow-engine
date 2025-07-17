using Infonetica.WorkflowEngine.Data;
using Infonetica.WorkflowEngine.Models;
using System.Linq;

namespace Infonetica.WorkflowEngine.Services;

/// A rich response model for a workflow instance
public record WorkflowInstanceResponse(
    Guid Id,
    string DefinitionId,
    string CurrentStateId,
    List<HistoryEntry> History,
    List<AvailableAction> AvailableActions
);

/// Represents a single action that is available to be executed from the current state.
public record AvailableAction(string Id, string Name, string? Description);


/// Contains the core business logic for creating and managing workflows.

public class WorkflowService
{
    private readonly InMemoryDataStore _store;

    public WorkflowService(InMemoryDataStore store)
    {
        _store = store;
    }


public (WorkflowDefinition? definition, string? error) CreateWorkflowDefinition(WorkflowDefinition definition)
{
    //For non repetitive workflow names
    if (_store.WorkflowDefinitions.Values.Any(d => d.Name.Equals(definition.Name, StringComparison.OrdinalIgnoreCase)))
    {
        return (null, $"A workflow with the name '{definition.Name}' already exists.");
    }

    //For a single intial of a new workflow
    if (definition.States.Count(s => s.IsInitial) != 1)
        return (null, "Workflow definition must have exactly one initial state.");

    //For unique state Ids
    if (definition.States.GroupBy(s => s.Id).Any(g => g.Count() > 1))
        return (null, "Duplicate state IDs are not allowed.");
    
    //Foor unique action ids
    if (definition.Actions.GroupBy(a => a.Id).Any(g => g.Count() > 1))
        return (null, "Duplicate action IDs are not allowed.");

    var stateIds = definition.States.Select(s => s.Id).ToHashSet();
    foreach (var action in definition.Actions)
    {
        if (!stateIds.Contains(action.ToState))
            return (null, $"Action '{action.Id}' has an invalid toState '{action.ToState}'.");
        
        foreach (var fromState in action.FromStates)
        {
            if (!stateIds.Contains(fromState))
                return (null, $"Action '{action.Id}' has an invalid fromState '{fromState}'.");
        }
    }

    // This is critical, implemeting server-generated Id.
    var definitionToStore = definition with { Id = Guid.NewGuid().ToString() };
    _store.WorkflowDefinitions.TryAdd(definitionToStore.Id, definitionToStore);
    return (definitionToStore, null);
}

    public WorkflowDefinition? GetWorkflowDefinition(string id)
    {
        _store.WorkflowDefinitions.TryGetValue(id, out var definition);
        return definition;
    }
    
    public ICollection<WorkflowDefinition> GetAllWorkflowDefinitions()
    {
        return _store.WorkflowDefinitions.Values;
    }

    // Instance methods->

    public (WorkflowInstance? instance, string? error) StartWorkflowInstance(string definitionId)
    {
        if (!_store.WorkflowDefinitions.TryGetValue(definitionId, out var definition))
            return (null, "Workflow definition not found.");

        var initialState = definition.States.Single(s => s.IsInitial);
        
        var instance = new WorkflowInstance(definitionId, initialState.Id);
        _store.WorkflowInstances.TryAdd(instance.Id, instance);

        return (instance, null);
    }
    
    public ICollection<WorkflowInstance> GetAllWorkflowInstances()
    {
        return _store.WorkflowInstances.Values;
    }



    /// Get instance details. The functionalities are same as mentioned in the message
    public (WorkflowInstanceResponse? instanceResponse, string? error) GetWorkflowInstanceDetails(Guid instanceId)
    {
        if (!_store.WorkflowInstances.TryGetValue(instanceId, out var instance))
            return (null, "Workflow instance not found.");

        if (!_store.WorkflowDefinitions.TryGetValue(instance.DefinitionId, out var definition))
            return (null, "Internal error: Workflow definition not found for this instance.");
        
        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
        if (currentState is { IsFinal: true })
        {
            // If current is a final state, no available actions.
            var finalResponse = new WorkflowInstanceResponse(instance.Id, instance.DefinitionId, instance.CurrentStateId, instance.History, new List<AvailableAction>());
            return (finalResponse, null);
        }

        // Available actions
        var availableActions = definition.Actions
            .Where(action => action.Enabled && action.FromStates.Contains(instance.CurrentStateId))
            .Select(action => new AvailableAction(action.Id, action.Name, action.Description))
            .ToList();

        var response = new WorkflowInstanceResponse(instance.Id, instance.DefinitionId, instance.CurrentStateId, instance.History, availableActions);

        return (response, null);
    }

    public (WorkflowInstance? instance, string? error) ExecuteAction(Guid instanceId, string actionId)
    {
        if (!_store.WorkflowInstances.TryGetValue(instanceId, out var instance))
            return (null, "Workflow instance not found.");
        if (!_store.WorkflowDefinitions.TryGetValue(instance.DefinitionId, out var definition))
            return (null, "Internal error: Workflow definition not found for this instance.");

        var action = definition.Actions.FirstOrDefault(a => a.Id == actionId);
        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);

        if (action == null) return (null, "Action not found in this workflow's definition.");
        if (currentState == null) return (null, "Internal error: Current state not found in definition.");
        if (!action.Enabled) return (null, "Action is disabled.");
        if (!action.FromStates.Contains(currentState.Id)) return (null, $"Action '{action.Name}' cannot be executed from the current state '{currentState.Name}'.");
        if (currentState.IsFinal) return (null, "Cannot execute action on a final state.");



        var fromStateId = instance.CurrentStateId; // getting state before changing it
        instance.CurrentStateId = action.ToState;

        var historyEntry = new HistoryEntry(actionId, fromStateId, action.ToState, DateTime.UtcNow);
        instance.History.Add(historyEntry);
        
        return (instance, null);
    }
}