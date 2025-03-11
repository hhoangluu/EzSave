using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzBoost.EzSave;
using System.Threading.Tasks;
using UnityEngine.Serialization;

namespace EzBoost.EzSave.Samples
{
    /// <summary>
    /// Sample demonstrating the use of EzSave's async functionality
    /// </summary>
    public class EzSaveAsyncSample : MonoBehaviour
    {
        // Sample player data to save and load
        [System.Serializable]
        public class PlayerData
        {
            public string playerName = "Player";
            public int level = 1;
            public int score = 0;
            public Vector3 position = Vector3.zero;
            public List<string> inventory = new List<string>();
            
            public PlayerData()
            {
                // Initialize with some items
                inventory = new List<string>() { "Sword", "Shield", "Potion" };
            }
            
            public override string ToString()
            {
                return $"Name: {playerName}, Level: {level}, Score: {score}, Position: {position}, Items: {inventory.Count}";
            }
        }
        
        // Keys for saving data
        private const string PLAYER_DATA_KEY = "playerData";
        private const string SETTINGS_KEY = "gameSettings";
        
        // Sample player data
        private PlayerData _playerData;
        
        // Sample settings
        [System.Serializable]
        public class GameSettings
        {
            public float musicVolume = 0.75f;
            public float sfxVolume = 1.0f;
            public bool fullscreen = true;
            public int qualityLevel = 2;
            
            public override string ToString()
            {
                return $"Music: {musicVolume}, SFX: {sfxVolume}, Fullscreen: {fullscreen}, Quality: {qualityLevel}";
            }
        }
        [FormerlySerializedAs("_saveSettings")] public SaveSettings _playerDataSaveSettings;
        private GameSettings _gameSettings;
        
        // GUI layout options
        private Vector2 _scrollPosition;
        private string _status = "Ready";
        private bool _isBusy = false;
        private float _operationProgress = 0f;
        private string _operationName = "";
        
        // Performance tracking
        private float _lastOperationTime = 0f;
        private int _saveCount = 0;
        private int _loadCount = 0;
        
        private void Start()
        {
            // Initialize sample data
            _playerData = new PlayerData();
            _gameSettings = new GameSettings();
            
            Debug.Log("EzSave Async Sample started!");
        }

        private void OnGUI()
        {
            // Set up GUI skin and styles for better visibility
            GUI.skin.label.fontSize = 14;
            GUI.skin.button.fontSize = 14;
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
            
            // Title
            GUILayout.Label("EzSave Async Sample", GUI.skin.box, GUILayout.Height(30));
            
            // Status area
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Status: {_status}");
            
            // Show progress bar if operation is in progress
            if (_isBusy)
            {
                GUILayout.Label($"Operation: {_operationName}");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Progress: ", GUILayout.Width(100));
                GUI.enabled = false;
                GUILayout.HorizontalSlider(_operationProgress, 0f, 1f);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Label($"Last operation time: {_lastOperationTime.ToString("F3")} seconds");
            GUILayout.Label($"Save count: {_saveCount}, Load count: {_loadCount}");
            GUILayout.EndVertical();
            
            // Scrollable content area
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUI.skin.box);
            
            // Current data display
            GUILayout.Label("Current Data:", GUI.skin.box);
            GUILayout.Label($"Player Data: {_playerData.ToString()}");
            GUILayout.Label($"Game Settings: {_gameSettings.ToString()}");
            
            GUILayout.Space(10);
            
            // Sync operation buttons
            GUILayout.Label("Synchronous Operations:", GUI.skin.box);
            GUILayout.BeginHorizontal();
            
            GUI.enabled = !_isBusy;
            if (GUILayout.Button("Save Sync"))
            {
                PerformSyncSave();
            }
            
            if (GUILayout.Button("Load Sync"))
            {
                PerformSyncLoad();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Async operation buttons
            GUILayout.Label("Asynchronous Operations (Task-based):", GUI.skin.box);
            GUILayout.BeginHorizontal();
            
            GUI.enabled = !_isBusy;
            if (GUILayout.Button("Save Async (Task)"))
            {
                StartCoroutine(PerformAsyncSaveWithTask());
            }
            
            if (GUILayout.Button("Load Async (Task)"))
            {
                StartCoroutine(PerformAsyncLoadWithTask());
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Callback-based async operation buttons
            GUILayout.Label("Asynchronous Operations (Callback-based):", GUI.skin.box);
            GUILayout.BeginHorizontal();
            
            GUI.enabled = !_isBusy;
            if (GUILayout.Button("Save Async (Callback)"))
            {
                PerformAsyncSaveWithCallback();
            }
            
            if (GUILayout.Button("Load Async (Callback)"))
            {
                PerformAsyncLoadWithCallback();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Data modification buttons
            GUILayout.Label("Modify Data:", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            GUI.enabled = !_isBusy;
            if (GUILayout.Button("Increase Level"))
            {
                _playerData.level++;
                _status = $"Increased player level to {_playerData.level}";
            }
            
            if (GUILayout.Button("Add Score"))
            {
                _playerData.score += 100;
                _status = $"Added 100 points, score is now {_playerData.score}";
            }
            
            if (GUILayout.Button("Move Player"))
            {
                _playerData.position = new Vector3(
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f),
                    UnityEngine.Random.Range(-10f, 10f)
                );
                _status = $"Moved player to {_playerData.position}";
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUI.enabled = !_isBusy;
            if (GUILayout.Button("Add Item"))
            {
                string[] possibleItems = {"Sword", "Shield", "Potion", "Bow", "Arrow", "Helmet", "Armor", "Ring", "Amulet", "Gem"};
                string newItem = possibleItems[UnityEngine.Random.Range(0, possibleItems.Length)];
                _playerData.inventory.Add(newItem);
                _status = $"Added item: {newItem}";
            }
            
            if (GUILayout.Button("Change Settings"))
            {
                _gameSettings.musicVolume = UnityEngine.Random.Range(0f, 1f);
                _gameSettings.sfxVolume = UnityEngine.Random.Range(0f, 1f);
                _gameSettings.fullscreen = !_gameSettings.fullscreen;
                _gameSettings.qualityLevel = UnityEngine.Random.Range(0, 4);
                _status = "Changed game settings";
            }
            
            if (GUILayout.Button("Reset Data"))
            {
                _playerData = new PlayerData();
                _gameSettings = new GameSettings();
                _status = "Reset all data to default values";
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        #region Synchronous Operations
        private void PerformSyncSave()
        {
            try
            {
                _status = "Saving data synchronously...";
                _isBusy = true;
                _operationName = "Sync Save";
                _operationProgress = 0.5f;
                
                float startTime = Time.realtimeSinceStartup;
                
                // Perform the save operations on the main thread (blocking)
                bool playerSaved = EzSave.Save(PLAYER_DATA_KEY, _playerData);
                _operationProgress = 0.75f;
                
                bool settingsSaved = EzSave.Save(SETTINGS_KEY, _gameSettings);
                _operationProgress = 1.0f;
                
                float endTime = Time.realtimeSinceStartup;
                _lastOperationTime = endTime - startTime;
                _saveCount++;
                
                if (playerSaved && settingsSaved)
                {
                    _status = $"Data saved successfully in {_lastOperationTime.ToString("F3")} seconds";
                }
                else
                {
                    _status = "Failed to save some data";
                }
            }
            catch (Exception ex)
            {
                _status = $"Error during sync save: {ex.Message}";
            }
            finally
            {
                _isBusy = false;
            }
        }
        
        private void PerformSyncLoad()
        {
            try
            {
                _status = "Loading data synchronously...";
                _isBusy = true;
                _operationName = "Sync Load";
                _operationProgress = 0.5f;
                
                float startTime = Time.realtimeSinceStartup;
                
                // Perform the load operations on the main thread (blocking)
                _playerData = EzSave.Load<PlayerData>(PLAYER_DATA_KEY, new PlayerData(), _playerDataSaveSettings);
                _operationProgress = 0.75f;
                
                _gameSettings = EzSave.Load<GameSettings>(SETTINGS_KEY, new GameSettings());
                _operationProgress = 1.0f;
                
                float endTime = Time.realtimeSinceStartup;
                _lastOperationTime = endTime - startTime;
                _loadCount++;
                
                _status = $"Data loaded successfully in {_lastOperationTime.ToString("F3")} seconds";
            }
            catch (Exception ex)
            {
                _status = $"Error during sync load: {ex.Message}";
            }
            finally
            {
                _isBusy = false;
            }
        }
        #endregion
        
        #region Task-based Async Operations
        private IEnumerator PerformAsyncSaveWithTask()
        {
            _status = "Saving data asynchronously (Task)...";
            _isBusy = true;
            _operationName = "Async Save (Task)";
            _operationProgress = 0.25f;
            
            float startTime = Time.realtimeSinceStartup;
            
            SaveOperation playerSaveOperation = null;
            SaveOperation settingsSaveOperation = null;
            
            try
            {
                // Use the new operation object
                playerSaveOperation = EzSave.SaveAsync(PLAYER_DATA_KEY, _playerData, _playerDataSaveSettings);
            }
            catch (Exception ex)
            {
                _status = $"Error starting async save: {ex.Message}";
                _isBusy = false;
                yield break;
            }
            
            // Wait while the async operation is running, allowing the UI to update
            while (!playerSaveOperation.Task.IsCompleted)
            {
                _operationProgress = 0.4f;
                yield return null;
            }
            
            _operationProgress = 0.5f;
            
            try
            {
                // Use the new operation object
                settingsSaveOperation = EzSave.SaveAsync(SETTINGS_KEY, _gameSettings);
            }
            catch (Exception ex)
            {
                _status = $"Error starting second async save: {ex.Message}";
                _isBusy = false;
                yield break;
            }
            
            // Wait while the async operation is running, allowing the UI to update
            while (!settingsSaveOperation.Task.IsCompleted)
            {
                _operationProgress = 0.75f;
                yield return null;
            }
            
            _operationProgress = 1.0f;
            
            float endTime = Time.realtimeSinceStartup;
            _lastOperationTime = endTime - startTime;
            _saveCount++;
            
            bool success = false;
            try
            {
                success = playerSaveOperation.Task.Result && settingsSaveOperation.Task.Result;
            }
            catch (Exception ex)
            {
                _status = $"Error getting task results: {ex.Message}";
                _isBusy = false;
                yield break;
            }
            
            if (success)
            {
                _status = $"Data saved async successfully in {_lastOperationTime.ToString("F3")} seconds";
            }
            else
            {
                _status = "Failed to save some data asynchronously";
            }
            
            _isBusy = false;
        }
        
        private IEnumerator PerformAsyncLoadWithTask()
        {
            _status = "Loading data asynchronously (Task)...";
            _isBusy = true;
            _operationName = "Async Load (Task)";
            _operationProgress = 0.25f;
            
            float startTime = Time.realtimeSinceStartup;
            
            LoadOperation<PlayerData> playerLoadOperation = null;
            LoadOperation<GameSettings> settingsLoadOperation = null;
            
            try
            {
                // Use the new operation object
                playerLoadOperation = EzSave.LoadAsync<PlayerData>(PLAYER_DATA_KEY, new PlayerData(), _playerDataSaveSettings);
            }
            catch (Exception ex)
            {
                _status = $"Error starting async load: {ex.Message}";
                _isBusy = false;
                yield break;
            }
            
            // Wait while the async operation is running, allowing the UI to update
            while (!playerLoadOperation.Task.IsCompleted)
            {
                _operationProgress = 0.4f;
                yield return null;
            }
            
            try
            {
                _playerData = playerLoadOperation.Task.Result;
            }
            catch (Exception ex)
            {
                _status = $"Error getting player data result: {ex.Message}";
                _isBusy = false;
                yield break;
            }
            
            _operationProgress = 0.5f;
            
            try
            {
                // Use the new operation object
                settingsLoadOperation = EzSave.LoadAsync<GameSettings>(SETTINGS_KEY, new GameSettings());
            }
            catch (Exception ex)
            {
                _status = $"Error starting settings load: {ex.Message}";
                _isBusy = false;
                yield break;
            }
            
            // Wait while the async operation is running, allowing the UI to update
            while (!settingsLoadOperation.Task.IsCompleted)
            {
                _operationProgress = 0.75f;
                yield return null;
            }
            
            try
            {
                _gameSettings = settingsLoadOperation.Task.Result;
            }
            catch (Exception ex)
            {
                _status = $"Error getting settings result: {ex.Message}";
                _isBusy = false;
                yield break;
            }
            
            _operationProgress = 1.0f;
            
            float endTime = Time.realtimeSinceStartup;
            _lastOperationTime = endTime - startTime;
            _loadCount++;
            
            _status = $"Data loaded async successfully in {_lastOperationTime.ToString("F3")} seconds";
            _isBusy = false;
        }
        #endregion
        
        #region Callback-based Async Operations
        private void PerformAsyncSaveWithCallback()
        {
            try
            {
                _status = "Saving data asynchronously (Callback)...";
                _isBusy = true;
                _operationName = "Async Save (Callback)";
                _operationProgress = 0.25f;
                
                float startTime = Time.realtimeSinceStartup;
                
                // First save operation with event subscription
                var playerSaveOperation = EzSave.SaveAsync(PLAYER_DATA_KEY, _playerData, _playerDataSaveSettings);
                playerSaveOperation.OnComplete += (success) => {
                    // This callback runs on the main thread after the player data is saved
                    _operationProgress = 0.5f;
                    
                    // Second save operation with event subscription
                    var settingsSaveOperation = EzSave.SaveAsync(SETTINGS_KEY, _gameSettings);
                    settingsSaveOperation.OnComplete += (success2) => {
                        // This callback runs on the main thread after the settings are saved
                        float endTime = Time.realtimeSinceStartup;
                        _lastOperationTime = endTime - startTime;
                        _saveCount++;
                        
                        _status = $"Data saved async (callback) successfully in {_lastOperationTime.ToString("F3")} seconds";
                        _operationProgress = 1.0f;
                        _isBusy = false;
                    };
                };
            }
            catch (Exception ex)
            {
                _status = $"Error during async save with callback: {ex.Message}";
                _isBusy = false;
            }
        }
        
        private void PerformAsyncLoadWithCallback()
        {
            try
            {
                _status = "Loading data asynchronously (Callback)...";
                _isBusy = true;
                _operationName = "Async Load (Callback)";
                _operationProgress = 0.25f;
                
                float startTime = Time.realtimeSinceStartup;
                
                // First load operation with event subscription
                var playerLoadOperation = EzSave.LoadAsync<PlayerData>(PLAYER_DATA_KEY, new PlayerData(), _playerDataSaveSettings);
                playerLoadOperation.OnComplete += (playerData) => {
                    // This callback runs on the main thread after the player data is loaded
                    _playerData = playerData;
                    _operationProgress = 0.5f;
                    
                    // Second load operation with event subscription
                    var settingsLoadOperation = EzSave.LoadAsync<GameSettings>(SETTINGS_KEY, new GameSettings());
                    settingsLoadOperation.OnComplete += (settings) => {
                        // This callback runs on the main thread after the settings are loaded
                        _gameSettings = settings;
                        
                        float endTime = Time.realtimeSinceStartup;
                        _lastOperationTime = endTime - startTime;
                        _loadCount++;
                        
                        _status = $"Data loaded async (callback) successfully in {_lastOperationTime.ToString("F3")} seconds";
                        _operationProgress = 1.0f;
                        _isBusy = false;
                    };
                };
            }
            catch (Exception ex)
            {
                _status = $"Error during async load with callback: {ex.Message}";
                _isBusy = false;
            }
        }
        #endregion
    }
} 