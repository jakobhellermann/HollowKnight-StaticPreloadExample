using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace StaticPreloadExample;

[PublicAPI]
public static class AssemblyUtils {
    private static Stream? GetEmbeddedResourceInner(Assembly assembly, string fileName) {
        var stream = assembly.GetManifestResourceStream(fileName);
        if (stream is null) {
            var embeddedResources = assembly.GetManifestResourceNames();
            StaticPreloadMod.Instance?.LogError(
                embeddedResources.Length == 0
                    ? $"Could not load embedded resource '{fileName}', the assembly {assembly.GetName().Name} contains no resources"
                    : $"Could not load embedded resource '{fileName}', did you mean one of {string.Join(", ", embeddedResources)}?");
            return null;
        }

        return stream;
    }

    private static byte[]? GetEmbeddedResourceBytesInner(Assembly assembly, string fileName) {
        var stream = GetEmbeddedResourceInner(assembly, fileName);
        if (stream is null) return null;

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    public static byte[]? GetEmbeddedResource(string name) =>
        GetEmbeddedResourceBytesInner(Assembly.GetCallingAssembly(), name);


    public static AssetBundle? GetEmbeddedAssetBundle(string name) {
        var assembly = Assembly.GetCallingAssembly();
        var stream = GetEmbeddedResourceInner(assembly, name);
        if (stream is null) return null;

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return AssetBundle.LoadFromMemory(memoryStream.ToArray());
    }
}