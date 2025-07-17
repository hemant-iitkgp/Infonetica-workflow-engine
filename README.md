# Configurable Workflow Engine API

This project implements a **configurable, state-machine-based Workflow Engine API**, created as a **Software Engineer Intern take-home exercise**.

The goal was to build a **clean, robust, and maintainable backend service** using **.NET 8 and ASP.NET Core Minimal API**. The result is a **minimal yet powerful engine** capable of defining and running any state-based workflow via a simple set of API endpoints.

---

## 🚀 Quick-Start and Running Instructions

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

---

### Step 1: Run the API Server

Navigate to the project's root directory in your terminal and run:

```bash
dotnet run
```

The server will start and listen on [http://localhost:5000](http://localhost:5000). You should see output confirming the application has started. Leave this terminal window running.

---

### Step 2: Testing the API

You can test the running service in two ways, with **Swagger UI recommended for ease of use**.

#### Option A: Using Swagger UI (Recommended)

Open your browser and navigate to:

```
http://localhost:5000/swagger
```

The **Swagger UI** provides an interactive interface for every endpoint. Expand any API call, click **Try it out**, fill in parameters, and click **Execute** to see live responses.

---

#### Option B: Using `curl` Commands

You can test the API directly via terminal.

1️⃣ **Create a Workflow Definition**

* Create a file in the project directory named `sample-workflow.json` containing valid workflow JSON.

Run:

```bash
# macOS/Linux
curl -X POST "http://localhost:5000/api/workflows" -H "Content-Type: application/json" -d "@sample-workflow.json"

# Windows PowerShell
curl.exe -X POST "http://localhost:5000/api/workflows" -H "Content-Type: application/json" -d "@sample-workflow.json"
```

Copy the `id` from the JSON response for the next step.

---

2️⃣ **Start a Workflow Instance**

Replace `{workflow-id}` with the ID you copied earlier:

```bash
# macOS/Linux
curl -X POST "http://localhost:5000/api/instances/start/{workflow-id}"

# Windows PowerShell
curl.exe -X POST "http://localhost:5000/api/instances/start/{workflow-id}"
```

Copy the `instance id` (a GUID) from the response for use in subsequent API calls.

---

## ✨ Features & Enhancements

The **core logic was implemented from scratch**. Inspired by real-world use cases, several enhancements were added beyond the base requirements to improve usability and robustness:

* **Enriched History Log**: Tracks `fromState`, `toState`, `actionId`, action, and timestamp for a **complete audit trail**.
* **"List All" Endpoints**:

  * `GET /api/workflows`
  * `GET /api/instances`

  for **better API discoverability**.
* **Available Actions Calculation**:

  `GET /api/instances/{id}` includes a dynamic `availableActions` list indicating which operations are legal from the current state, saving clients from re-implementing state-machine logic.

---

## Assumptions, Shortcuts, and Limitations

* **Persistence**: Data is stored in-memory using a singleton service. All created workflows and instances will be **lost on application shutdown**.
* **Concurrency**: Uses `ConcurrentDictionary` for **basic thread safety**. A production system would require **advanced concurrency controls** (e.g., optimistic locking with ETags or version numbers) to handle simultaneous requests safely.
* **Dependencies & Swagger**: The only external dependency is `Swashbuckle.AspNetCore` for **Swagger documentation and interactive testing**, aligning with the exercise’s goal of minimal dependencies. All **core state-machine logic is implemented from scratch**.
