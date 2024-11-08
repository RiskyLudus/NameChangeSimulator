using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AnarchyConstructFramework.Core.Common
{
    public class ConstructLoader : MonoBehaviour
    {

        // Load and instantiate a prefab asynchronously
        public static void LoadPrefabConstructAsync(string prefabAddress, System.Action<GameObject> onPrefabLoaded = null)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefabAddress);
            handle.Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    onPrefabLoaded?.Invoke(handle.Result);
                    Debug.Log($"Prefab '{prefabAddress}' loaded and instantiated successfully.");
                }
                else
                {
                    Debug.LogError($"Failed to load and instantiate prefab '{prefabAddress}'. Error: {operationHandle.OperationException}");
                }
            };
        }

        // Unload a prefab
        public static void UnloadPrefabConstruct(GameObject prefab)
        {
            Addressables.ReleaseInstance(prefab);
            Debug.Log($"Prefab '{prefab.name}' unloaded successfully.");
        }


        // Load a scene additively asynchronously
        public static void LoadSceneConstructAsync(string sceneAddress, System.Action<string> onSceneLoaded = null)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive);
            handle.Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    onSceneLoaded?.Invoke(sceneAddress);
                    Debug.Log($"Scene '{sceneAddress}' loaded successfully.");
                }
                else
                {
                    Debug.LogError($"Failed to load scene '{sceneAddress}'. Error: {operationHandle.OperationException}");
                }
            };
        }

        // Unload a scene
        public static void UnloadSceneConstruct(string sceneAddress)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive);
            handle.Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Addressables.UnloadSceneAsync(handle, true);
                    Debug.Log($"Scene '{sceneAddress}' unloaded successfully.");
                }
                else
                {
                    Debug.LogError($"Failed to unload scene '{sceneAddress}'. Error: {operationHandle.OperationException}");
                }
            };
        }
    }
}
