# EzSave Async Sample

This sample demonstrates how to use the asynchronous save and load functionality in EzSave to improve game performance.

## Features Demonstrated

- Synchronous vs Asynchronous saving and loading
- Task-based async operations with await/yield
- Event-based async operations
- Performance comparisons between methods
- Ensuring callbacks run on the main thread

## How to Use

1. Open the `EzSaveAsyncSample.unity` scene
2. Press Play to run the sample
3. Use the interface buttons to test different save/load methods
4. Modify data using the buttons in the "Modify Data" section
5. Compare performance between synchronous and asynchronous operations

## Code Examples

### Task-based Async Save/Load

```csharp
// Save data asynchronously
SaveOperation saveOp = EzSave.SaveAsync("playerData", playerData);
await saveOp.Task; // Can be awaited

// Load data asynchronously with default value
LoadOperation<PlayerData> loadOp = EzSave.LoadAsync<PlayerData>("playerData", new PlayerData());
PlayerData data = await loadOp.Task; // Can be awaited
```

### Event-based Async Save/Load

```csharp
// Save data asynchronously with event subscription
SaveOperation saveOp = EzSave.SaveAsync("playerData", playerData);
saveOp.OnComplete += (success) => {
    // This event handler runs on the main thread after the save is complete
    Debug.Log($"Save completed with result: {success}");
};

// Load data asynchronously with event subscription
LoadOperation<PlayerData> loadOp = EzSave.LoadAsync<PlayerData>("playerData", new PlayerData());
loadOp.OnComplete += (loadedData) => {
    // This event handler runs on the main thread with the loaded data
    playerData = loadedData;
    UpdateUI();
};
```

## Combining Task and Event Approaches

```csharp
// You can use both task and event approaches together
SaveOperation saveOp = EzSave.SaveAsync("playerData", playerData);

// Subscribe to completion event
saveOp.OnComplete += (success) => {
    Debug.Log("Operation completed via event!");
};

// Also await the task
await saveOp.Task;
Debug.Log("Operation completed via await!");
```

## Performance Considerations

- The async methods significantly reduce main thread stalls during I/O operations
- With large data sets, the performance improvement becomes more noticeable
- Compression and encryption operations are performed off the main thread

## Implementation Details

The asynchronous functionality is handled by the `EzSaveAsync` class, which:

1. Manages a thread-safe queue of operations
2. Ensures event handlers execute on the Unity main thread
3. Handles threading synchronization automatically

For additional examples and advanced usage, see the main EzSave documentation. 