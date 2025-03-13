using UnityEngine;
using EzBoost.EzSave.Crypto;
using EzBoost.EzSave.Storage;

namespace EzBoost.EzSave
{
        public class EzSaveDefaultSetting : ScriptableObject
    {
        [SerializeField] 
        private SaveSettings _defaultSaveSetting;

        public SaveSettings defaultSaveSetting => _defaultSaveSetting;
    }
} 