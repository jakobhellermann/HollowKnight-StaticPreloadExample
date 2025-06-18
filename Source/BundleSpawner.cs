using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace StaticPreloadExample;

internal class BundleSpawner : MonoBehaviour {
    private AssetBundle? bundle;
    private CancellationTokenSource? cts;
    private List<GameObject> loadedObjects = [];

    private void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            LoadAssetBundle();
        }
    }

    private void LoadAssetBundle() {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        bundle?.Unload(true);

        var sw = Stopwatch.StartNew();
        
        // const string path = "/home/jakob/dev/unity/unity-scene-repacker/out/hollowknight.unity3d";
        // var allBytes = File.ReadAllBytes(path);
        // bundle = AssetBundle.LoadFromMemory(allBytes);
        // The bundle is defined in the .csproj as <EmbeddedResource />
        bundle = AssemblyUtils.GetEmbeddedAssetBundle("StaticPreloadExample.bundle.unity3d");

        StaticPreloadMod.Instance.Log($"LoadFromMemory took {sw.ElapsedMilliseconds}ms");

        // In a real mod, you might want to load the assetbundle once when you want to use it,
        // and keep the spawned scene in memory if they're not too big.
        // There's a bunch of optimizations you can figure out here.
        if (bundle == null) {
            StaticPreloadMod.Instance.LogError("Failed to load AssetBundle");
            return;
        }

        StartCoroutine(SpawnAllObjects(bundle, cts.Token));
    }


    // Loads all the scenes contained in the AssetBundle, and spawn copies of their objects
    private IEnumerator SpawnAllObjects(AssetBundle bundle, CancellationToken token, float delay = 0.5f, bool oneByOne = true) {
        foreach (var scenePath in bundle.GetAllScenePaths()) {
            if (token.IsCancellationRequested) break;

            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scenePath);
            StaticPreloadMod.Instance.Log($"{sceneName}:");

            foreach (var prefab in scene.GetRootGameObjects()) {
                if (oneByOne) loadedObjects.ForEach(Destroy);
                StaticPreloadMod.Instance.Log($"- {prefab.name}");
                
                var instantiation = Instantiate(prefab);
                instantiation.SetActive(true);

                var pos = HeroController.instance.transform.position + Vector3.right * 5;
                instantiation.transform.position = pos;
                loadedObjects.Add(instantiation);

                yield return new WaitForSeconds(delay);
            }

            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }

        loadedObjects.ForEach(Destroy);
    }


    private static IEnumerator LoadAllThenSpawn(AssetBundle bundle) {
        var sceneNames = bundle.GetAllScenePaths()
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();

        var sw = Stopwatch.StartNew();
        var ops = sceneNames
            .Select(sceneName =>
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive))
            .ToList();

        while (ops.Any(op => op.progress < 1)) {
            var progress = ops.Sum(op => op.progress) / ops.Count;
            StaticPreloadMod.Instance.Log($"{progress * 100:F0}%");
            yield return null;
        }

        StaticPreloadMod.Instance.Log($"Scene load took {sw.ElapsedMilliseconds}ms");
        bundle.Unload(false);

        try {
            List<GameObject> allObjects = [];
            foreach (var prefab in sceneNames.Select(UnityEngine.SceneManagement.SceneManager.GetSceneByName)
                         .SelectMany(scene => scene.GetRootGameObjects())) {
                var instantiation = Instantiate(prefab);
                instantiation.SetActive(true);
                instantiation.transform.position = HeroController.instance.transform.position
                                                   + Vector3.right * Random.Range(-10, 10);
                allObjects.Add(instantiation);
            }

            yield return new WaitForSeconds(10);

            allObjects.ForEach(Destroy);
        } finally {
            sceneNames.ForEach(sceneName => UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName));
        }
    }


    private void OnDestroy() {
        cts?.Cancel();
        bundle?.Unload(true);
    }
}