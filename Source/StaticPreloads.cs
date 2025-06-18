using System.Reflection;
using Modding;

namespace StaticPreloadExample;

public class StaticPreloadMod : Mod, ITogglableMod {
    public static StaticPreloadMod? LoadedInstance { get; private set; }

    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public override void Initialize() {
        if (LoadedInstance != null) return;
        LoadedInstance = this;
    }

    public void Unload() {
        LoadedInstance = null;
    }
}