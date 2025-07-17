using Infonetica.WorkflowEngine.Models;
using System.Collections.Concurrent;

namespace Infonetica.WorkflowEngine.Data;

/// <summary>
/// A simple in-memory data store using thread safe dictionaries
/// This will be registered as a singleton, hence instance is shared across the entire application
/// </summary>
public class InMemoryDataStore
{
    public ConcurrentDictionary<string, WorkflowDefinition> WorkflowDefinitions { get; } = new();
    public ConcurrentDictionary<Guid, WorkflowInstance> WorkflowInstances { get; } = new();
}