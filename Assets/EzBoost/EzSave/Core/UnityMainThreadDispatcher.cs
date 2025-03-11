using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EzBoost.EzSave.Core
{
    /// <summary>
    /// Handles async operations and ensures actions are executed on the Unity main thread
    /// </summary>
    internal class EzSaveAsync : MonoBehaviour
    {
        private static EzSaveAsync _instance;
        private static readonly object _lock = new object();
        private static Thread _mainThread;
        private static SynchronizationContext _synchronizationContext;

        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        private readonly List<Action> _executionQueueCopy = new List<Action>();
        private bool _isExecuting = false;

        /// <summary>
        /// Gets the instance of the EzSaveAsync
        /// </summary>
        public static EzSaveAsync Instance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Create a new GameObject for the dispatcher
                        var go = new GameObject("EzSaveAsync");
                        _instance = go.AddComponent<EzSaveAsync>();
                        DontDestroyOnLoad(go);
                        
                        // Store the main thread and sync context
                        _mainThread = Thread.CurrentThread;
                        _synchronizationContext = SynchronizationContext.Current;
                    }
                }
            }
            return _instance;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Adds an action to the execution queue
        /// </summary>
        /// <param name="action">The action to execute on the main thread</param>
        public void Enqueue(Action action)
        {
            // If we're on the main thread already, just execute it
            if (Thread.CurrentThread == _mainThread)
            {
                action();
                return;
            }
            
            // Otherwise queue it for later execution
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            // Don't allow nested execution to avoid weird edge cases
            if (_isExecuting)
                return;

            _isExecuting = true;

            lock (_executionQueue)
            {
                // Copy the queue to a list to allow new items to be enqueued during execution
                _executionQueueCopy.Clear();
                while (_executionQueue.Count > 0)
                {
                    _executionQueueCopy.Add(_executionQueue.Dequeue());
                }
            }

            // Execute all the actions
            foreach (Action action in _executionQueueCopy)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Debug.LogError($"EzSaveAsync: Error executing action: {e.Message}");
                }
            }

            _isExecuting = false;
        }
    }
} 