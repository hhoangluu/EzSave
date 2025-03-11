using UnityEngine;
using EzBoost.EzSave.Crypto;
using EzBoost.EzSave.Storage;

namespace EzBoost.EzSave
{
    /// <summary>
    /// ScriptableObject that stores default settings for EzSave
    /// This allows users to configure default settings in the Unity Editor
    /// </summary>
    public class EzSaveDefaultSetting : ScriptableObject
    {
        [SerializeField] 
        private SaveSettings _defaultSaveSetting;

        public SaveSettings defaultSaveSetting => _defaultSaveSetting;
    }
} 