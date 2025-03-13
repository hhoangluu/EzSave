using UnityEngine;
using EzBoost.EzSave;
using System.IO;
using System;
using EzBoost.EzSave.Crypto;
using Random = UnityEngine.Random;

public class SaveLoadExample : MonoBehaviour
{
    // This class will be serialized and saved
    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public int score;
        public float health;
        public Vector3 position;
        public Quaternion rotation;
        public bool hasKey;
    }

    private PlayerData _playerData;
    private SaveSettings _saveSettings;
    private const string SaveFileName = "game_data.json";
    private const string PlayerDataKey = "player_data";
    private const string GameSettingsKey = "game_settings";
    private const string GameStatsKey = "game_stats";

    [System.Serializable]
    public class GameSettings
    {
        public float musicVolume = 0.75f;
        public float sfxVolume = 1.0f;
        public bool fullscreen = true;
        public int qualityLevel = 2;
    }

    [System.Serializable]
    public class GameStats
    {
        public int timesPlayed = 0;
        public float totalPlayTime = 0;
        public int enemiesDefeated = 0;
    }

    private GameSettings _gameSettings;
    private GameStats _gameStats;
    private float _sessionStartTime;

    private void Start()
    {
        _sessionStartTime = Time.time;

        // Create player data save settings
        _saveSettings = new SaveSettings(SaveFileName,"PlayerData" , false, EncryptionType.None);

        // Initialize default player data
        _playerData = new PlayerData
        {
            playerName = "Player1",
            score = 0,
            health = 100f,
            position = transform.position,
            rotation = transform.rotation,
            hasKey = false
        };

        // Initialize default game settings
        _gameSettings = new GameSettings();

        // Initialize or load game stats
        _gameStats = new GameStats();
        
        // Load existing data if available
        LoadAllData();
        
        // Update stats for this play session
        _gameStats.timesPlayed++;
        SaveGameStats();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 350, 950));
        
        GUILayout.Label("<b>PLAYER DATA</b>", GUI.skin.box, GUILayout.Height(25));
        GUILayout.Label($"Player: {_playerData.playerName}");
        GUILayout.Label($"Score: {_playerData.score}");
        GUILayout.Label($"Health: {_playerData.health}");
        GUILayout.Label($"Position: {_playerData.position}");
        GUILayout.Label($"Has Key: {_playerData.hasKey}");
        
        if (GUILayout.Button("Increase Score"))
        {
            _playerData.score += 10;
        }
        
        if (GUILayout.Button("Take Damage"))
        {
            _playerData.health -= 10f;
            if (_playerData.health < 0) _playerData.health = 0;
        }
        
        if (GUILayout.Button("Heal"))
        {
            _playerData.health += 10f;
            if (_playerData.health > 100) _playerData.health = 100;
        }
        
        if (GUILayout.Button("Toggle Key"))
        {
            _playerData.hasKey = !_playerData.hasKey;
        }
        
        if (GUILayout.Button("Update Position"))
        {
            _playerData.position = transform.position;
            _playerData.rotation = transform.rotation;
        }

        if (GUILayout.Button("Save Player Data"))
        {
            EzSave.Save(PlayerDataKey, _playerData, _saveSettings);
            Debug.Log("Player data saved!");
        }
        
        // if (GUILayout.Button("Save Player Data ONLY"))
        // {
        //     SavePlayerData();
        //     Debug.Log("Player data saved!");
        // }
        
        GUILayout.Space(10);
        
        GUILayout.Label("<b>GAME SETTINGS</b>", GUI.skin.box, GUILayout.Height(25));
        GUILayout.Label($"Music Volume: {_gameSettings.musicVolume:F2}");
        GUILayout.Label($"SFX Volume: {_gameSettings.sfxVolume:F2}");
        GUILayout.Label($"Fullscreen: {_gameSettings.fullscreen}");
        GUILayout.Label($"Quality Level: {_gameSettings.qualityLevel}");
        
        if (GUILayout.Button("Change Settings"))
        {
            _gameSettings.musicVolume = Random.Range(0f, 1f);
            _gameSettings.sfxVolume = Random.Range(0f, 1f);
            _gameSettings.fullscreen = !_gameSettings.fullscreen;
            _gameSettings.qualityLevel = Random.Range(0, 6);
        }

        if (GUILayout.Button("Save Settings"))
        {
            EzSave.Save(GameSettingsKey, _gameSettings, SaveFileName);
            Debug.Log("Game settings saved!");
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("<b>GAME STATS</b>", GUI.skin.box, GUILayout.Height(25));
        GUILayout.Label($"Times Played: {_gameStats.timesPlayed}");
        GUILayout.Label($"Total Play Time: {_gameStats.totalPlayTime + (Time.time - _sessionStartTime):F1} seconds");
        GUILayout.Label($"Enemies Defeated: {_gameStats.enemiesDefeated}");
        
        if (GUILayout.Button("Defeat Enemy"))
        {
            _gameStats.enemiesDefeated++;
            SaveGameStats();
        }

        if (GUILayout.Button("Save Stats"))
        {
            SaveGameStats();
            Debug.Log("Game stats saved!");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Save All Data"))
        {
            SaveAllData();
            Debug.Log("All data saved!");
        }
        
        if (GUILayout.Button("Load All Data"))
        {
            LoadAllData();
            Debug.Log("All data loaded!");
        }
        
        if (GUILayout.Button("Delete Player Data"))
        {
            if (EzSave.DeleteKey(PlayerDataKey, SaveFileName))
            {
                Debug.Log("Player data deleted!");
            }
            else
            {
                Debug.LogWarning("Could not delete player data!");
            }
        }
        
        if (GUILayout.Button("List All Keys"))
        {
            string[] keys = EzSave.GetKeys();
            Debug.Log($"Keys in save file: {string.Join(", ", keys)}");
            string[] keys2 = EzSave.GetKeys(_saveSettings);
            Debug.Log($"Keys in save file: {_saveSettings.SubFolder} {string.Join(", ", keys2)}");
        }
        
        if (GUILayout.Button("Delete All Data"))
        {
            EzSave.DeleteAllData();
            Debug.Log("All data deleted!");
        }
        
        GUILayout.EndArea();
    }

    private void SaveAllData()
    {
        // Update player position and rotation before saving
        _playerData.position = transform.position;
        _playerData.rotation = transform.rotation;
        
        // Update total play time in stats
        _gameStats.totalPlayTime += (Time.time - _sessionStartTime);
        _sessionStartTime = Time.time;
        
        // Debug log before saving
        Debug.Log($"SaveAllData: Saving player data with key '{PlayerDataKey}' to '{SaveFileName}'");
        
        // Try both ways to save to ensure at least one works:
        // Method 1: Using SaveSettings
        bool result1 = EzSave.Save(PlayerDataKey, _playerData, _saveSettings);
        Debug.Log($"Save using settings result: {result1}");
        
        // // Method 2: Direct save with key and filename
        // bool result2 = EzSave.Save(PlayerDataKey, _playerData, SaveFileName);
        // Debug.Log($"Save using key/filename result: {result2}");
        
        // Save game settings
        EzSave.Save(GameSettingsKey, _gameSettings, SaveFileName);
        
        // Save game stats
        SaveGameStats();
        
        // Verify that the file exists after saving
        bool fileExists = EzSave.FileExists(SaveFileName);
        Debug.Log($"After saving, file exists: {fileExists}");
        
        // Check if the key exists
        bool keyExists = EzSave.KeyExists(PlayerDataKey, SaveFileName);
        Debug.Log($"After saving, key exists: {keyExists}");
    }

    private void LoadAllData()
    {
        // Load player data if exists
        if (EzSave.KeyExists(PlayerDataKey, _saveSettings))
        {
            var data = EzSave.Load<PlayerData>(PlayerDataKey, _saveSettings);
            Debug.Log($"LoadAllData: Loaded player data: {data}");  
            if (data != null)
            {
                _playerData = data;
                transform.position = _playerData.position;
                transform.rotation = _playerData.rotation;
            }
        }
        
        // Load game settings if exists - using just key and filename
        if (EzSave.KeyExists(GameSettingsKey, SaveFileName))
        {
            _gameSettings = EzSave.Load<GameSettings>(GameSettingsKey, SaveFileName);
        }
        // Load game stats if exists - using key, default value, and filename

        if (EzSave.KeyExists(GameStatsKey, SaveFileName))
            _gameStats = EzSave.Load(GameStatsKey, new GameStats(), SaveFileName);
    }

    private void SaveGameStats()
    {
        // Simple approach using just key and filename
        EzSave.Save(GameStatsKey, _gameStats, SaveFileName);
    }

    private void OnApplicationQuit()
    {
        // Update play time and save before quitting
        _gameStats.totalPlayTime += (Time.time - _sessionStartTime);
        SaveAllData();
    }

    public void SavePlayerData()
    {
        Debug.Log("SavePlayerData: Button clicked");
        
        // Update position and rotation before saving
        _playerData.position = transform.position;
        _playerData.rotation = transform.rotation;
        
        // Directly save player data only
        bool saveResult = EzSave.Save(PlayerDataKey, _playerData, SaveFileName);
        Debug.Log($"Save player data result: {saveResult}");
    }
} 