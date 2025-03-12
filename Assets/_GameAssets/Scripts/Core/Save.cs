using System.Collections.Generic;

namespace _GameAssets.Scripts.Core
{
    public class Save
    {
        private readonly ES3File _memoryFile;
        private readonly Dictionary<string, object> _localValues = new Dictionary<string, object>();
        private readonly string _path;

        public Save(string path)
        {
            _path = path;
            _memoryFile = new ES3File(_path);
        }
        
        /// <summary>
        /// Saves value 
        /// In editor and PC build immediately sync memory file 
        /// </summary>
        public void SaveValue<T>(string key, T value)
        {
            _localValues[key] = value;
            _memoryFile.Save<T>(key, value);
        }

        /// <summary>
        /// Saves value and immediately sync memory file 
        /// </summary>
        public void SaveValueAndSync<T>(string key, T value)
        {
            SaveValue(key, value);
            SyncMemoryFile();
        }

        private bool SaveFileExists()
        {
            return ES3.FileExists(_path);
        }
        
        /// <summary>
        /// Loads value with specified key or returns default if it's not found
        /// For string type value use LoadStringValue()
        /// </summary>
        public T LoadValue<T>(string key, T defaultValue = default)
        {
            if (_localValues.ContainsKey(key))
            {
                return (T)_localValues[key];
            }
            if (KeyExists(key))
            {
                var valueFromEs3Memory = _memoryFile.Load<T>(key);
                _localValues.Add(key, valueFromEs3Memory);
                return valueFromEs3Memory;
            }

            var initialValue = Equals(defaultValue, default(T)) ? default : defaultValue; 
            _localValues.Add(key, initialValue);
            return initialValue;
        }

        /// <summary>
        /// Loads string with specified key or returns default if it's not found
        /// </summary>
        public string LoadStringValue(string key, string defaultValue = default)
        {
            if (_localValues.ContainsKey(key))
            {
                return (string)_localValues[key];
            }
            if (KeyExists(key))
            {
                var valueFromEs3Memory = _memoryFile.Load<string>(key);
                _localValues.Add(key, valueFromEs3Memory);
                return valueFromEs3Memory;
            }
            _localValues.Add(key, defaultValue);
            return defaultValue;
        }
        
        public bool KeyExists(string key)
        {
            return _memoryFile.KeyExists(key);
        }

        public void SyncMemoryFile()
        {
            _memoryFile.Sync();
        }
    
        public void DeleteSaveFile()
        {
            if (!SaveFileExists()) return;
            ES3.DeleteFile(_path);
        } 
    }
}