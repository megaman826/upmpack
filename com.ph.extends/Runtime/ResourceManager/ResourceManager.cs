using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ph.package
{
    public class ResourceManager
    {
        private Dictionary<string, Object> loadedAssetDict = new Dictionary<string, Object>();
        public delegate void LoadAssetComplete(bool isSuccess);
        public delegate void UnloadAssetComplete(bool isSuccess);
        
        private int _remainLoadCount;
        private bool _loadAssetsSuccess;
        private List<string> _loadFailAssetKey = new List<string>();
        private LoadAssetComplete _loadAssetComplete;
    
        public T GetAsset<T>(string key) where T : Object
        {
            if (!loadedAssetDict.ContainsKey(key))
            {
                Debug.Log($"Load Asset First! : {key}");
                return null;
            }

            return loadedAssetDict[key] as T;
        }

        public void UnloadAsset(string key)
        {
            if (loadedAssetDict.ContainsKey(key))
            {
                Addressables.Release(loadedAssetDict[key]);
                loadedAssetDict.Remove(key);
                Debug.Log($"Unload Asset Complete! : {key}");
            }
        }
    
        public void UnloadAssets(IList<string> keys)
        {
            foreach (string key in keys)
                UnloadAsset(key);
        }
    
        public void UnloadAssetsAsync(string label, UnloadAssetComplete unloadAssetComplete)
        {
            StartUnloadAssets(label, unloadAssetComplete);
        }
    
        public void LoadAssetAsync<T>(string key, LoadAssetComplete loadAssetComplete) where T : Object
        {
            if (loadedAssetDict.ContainsKey(key))
            {
                Debug.Log($"Already Loaded Asset : {key}");
                loadAssetComplete(true);
                return;
            }
            
            StartLoadAsset<T>(key, loadAssetComplete);
        }
    
        public void LoadAssetsAsync<T>(IList<string> keys, LoadAssetComplete loadAssetComplete) where T : Object
        {
            IList<string> intersectKeys = keys.Intersect(loadedAssetDict.Keys).ToList();
            Debug.Log($"Already Loaded Asset : {string.Join(",", intersectKeys)}");
            
            IList<string> remainKeys = keys.Except(loadedAssetDict.Keys).ToList();
            StartLoadAssets<T>(remainKeys, loadAssetComplete);
        }
    
        public void LoadAssetsAsync<T>(string label, LoadAssetComplete loadAssetComplete) where T : Object
        {
            StartLoadAssets<T>(label, loadAssetComplete);
        }
    
        private void StartLoadAsset<T>(string key, LoadAssetComplete loadAssetComplete) where T : Object
        {
            Addressables.LoadAssetAsync<T>(key).Completed += op =>
            {
                bool isSuccess = op.Status == AsyncOperationStatus.Succeeded;
                if(isSuccess)
                    loadedAssetDict.Add(key, op.Result);
            
                Debug.Log($"Load Asset Complete {isSuccess} : {key}");
                loadAssetComplete(isSuccess);
            };
        }

        private void StartLoadAssets<T>(IList<string> keys, LoadAssetComplete loadAssetComplete) where T : Object
        {
            Debug.Log($"Start Load Assets : {string.Join(",", keys)}");
            
            _loadAssetsSuccess = true;
            _loadAssetComplete = loadAssetComplete;
            _remainLoadCount = keys.Count;
            
            foreach (string key in keys)
            {
                LoadAssetAsync<T>(key, (success) =>
                {
                    if (!success)
                    {
                        _loadFailAssetKey.Add(key);
                        _loadAssetsSuccess = false;
                    }
                    
                    CompleteLoadAsset();
                });
            }
        }

        private void CompleteLoadAsset()
        {
            _remainLoadCount--;

            if (_remainLoadCount > 0)
                return;
            
            if(!_loadAssetsSuccess)
                Debug.Log($"Load Fail Assets : {string.Join(",", _loadFailAssetKey)}");

            Debug.Log($"Load Assets Complete");

            _loadAssetComplete?.Invoke(true);
            _remainLoadCount = 0;
            _loadFailAssetKey.Clear();
            _loadAssetComplete = null;
        }

        private void StartLoadAssets<T>(string label, LoadAssetComplete loadAssetComplete) where T : Object
        {
            Debug.Log($"Start Load ResourceLocation : {label}");
            Addressables.LoadResourceLocationsAsync(label).Completed += op =>
            {
                bool isSuccess = op.Status == AsyncOperationStatus.Succeeded;
                if (isSuccess)
                {
                    List<string> keys = op.Result.Select(e => e.PrimaryKey).ToList();
                    StartLoadAssets<T>(keys, loadAssetComplete);
                }
                else
                {
                    Debug.Log($"Load Assets Failed");
                    loadAssetComplete(false);
                }
                    
                Addressables.Release(op.Result);
            };
        }
        
        private void StartUnloadAssets(string label, UnloadAssetComplete unloadAssetComplete)
        {
            Debug.Log($"Start Load ResourceLocation : {label}");
            Addressables.LoadResourceLocationsAsync(label).Completed += op =>
            {
                bool isSuccess = op.Status == AsyncOperationStatus.Succeeded;
                if (isSuccess)
                {
                    List<string> keys = op.Result.Select(e => e.PrimaryKey).ToList();
                    UnloadAssets(keys);
                }
                Addressables.Release(op.Result);

                unloadAssetComplete(isSuccess);
            };
        }
    }
}