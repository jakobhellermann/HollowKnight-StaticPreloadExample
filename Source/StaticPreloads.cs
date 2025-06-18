using System.Reflection;
using Modding;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StaticPreloadExample;

public class StaticPreloadMod : Mod, ITogglableMod {
    public static StaticPreloadMod Instance = null!;
    private MonoBehaviour spawner = null!;

    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public override void Initialize() {
        Instance = this;

        spawner = new GameObject().AddComponent<BundleSpawner>();
        Object.DontDestroyOnLoad(spawner.gameObject);
    }

    public void Unload() {
        Instance = null!;
        Object.Destroy(spawner.gameObject);
    }
}