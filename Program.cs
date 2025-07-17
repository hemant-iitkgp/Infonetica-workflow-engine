using Infonetica.WorkflowEngine.Data;
using Infonetica.WorkflowEngine.Models;
using Infonetica.WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Dependency->
// Putting in-memory store as Singleton for data persists
builder.Services.AddSingleton<InMemoryDataStore>();

//  register the workflow service
builder.Services.AddScoped<WorkflowService>();

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Api Endpoints->

// organizing endpoints
var workflowApi = app.MapGroup("/api/workflows").WithTags("Workflow Configuration");
var instanceApi = app.MapGroup("/api/instances").WithTags("Runtime");

// Workflow endpoints

workflowApi.MapPost("/", (WorkflowDefinition definition, WorkflowService service) =>
{
    var (createdDef, error) = service.CreateWorkflowDefinition(definition);
    if (error != null)
    {
        return Results.BadRequest(new { message = error });
    }
    // Return a 201 Created response
    return Results.Created($"/api/workflows/{createdDef!.Id}", createdDef);
});

workflowApi.MapGet("/", (WorkflowService service) =>
{
    return Results.Ok(service.GetAllWorkflowDefinitions());
});

workflowApi.MapGet("/{id}", (string id, WorkflowService service) =>
{
    var definition = service.GetWorkflowDefinition(id);
    return definition is not null ? Results.Ok(definition) : Results.NotFound();
});

// Runtime endpoints->

instanceApi.MapPost("/start/{definitionId}", (string definitionId, WorkflowService service) =>
{
    var (instance, error) = service.StartWorkflowInstance(definitionId);
    if (error != null)
    {
        //if the definition doesn't exist(404)
        return Results.NotFound(new { message = error });
    }
    return Results.Created($"/instances/{instance!.Id}", instance);
});

instanceApi.MapGet("/", (WorkflowService service) =>
{
    return Results.Ok(service.GetAllWorkflowInstances());
});



instanceApi.MapGet("/{instanceId:guid}", (Guid instanceId, WorkflowService service) =>
{
    var (instanceDetails, error) = service.GetWorkflowInstanceDetails(instanceId);
    if (error != null){return Results.NotFound(new { message = error });}
    return Results.Ok(instanceDetails);
});

instanceApi.MapPost("/{instanceId:guid}/execute/{actionId}", (Guid instanceId, string actionId, WorkflowService service) =>
{
    var (instance, error) = service.ExecuteAction(instanceId, actionId);
    if (error != null)
    {
        // 400 for errors and 404 for missing entities.
        if (error.Contains("not found"))
            return Results.NotFound(new { message = error });
        else
            return Results.BadRequest(new { message = error });
    }
    return Results.Ok(instance);
});


app.Run();