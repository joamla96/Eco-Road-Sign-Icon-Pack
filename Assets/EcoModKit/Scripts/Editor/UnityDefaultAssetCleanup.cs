// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

#if UNITY_EDITOR // Needed directive to prevent InitializeOnLoad error in bundle builds
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary> Cleanups unity default garbage that prevents mod bundle builds for eco mod developers </summary>
[InitializeOnLoad]
public class UnityDefaultAssetCleanup
{
    static readonly string[] directories = {"Assets/TutorialInfo"};
    static readonly string[] files       = {"Assets/Readme.asset"};
    
    static UnityDefaultAssetCleanup() => CleanupUnityDefaultFiles();

    [MenuItem("Eco Tools/Mod Kit/Try Fix Bundle Creation Errors")] // Manual way
    static void CleanupUnityDefaultFiles()
    {
        var changed = false;

        foreach (var dir in directories) { if (TryRemoveDirectory(dir)) changed = true; }
        foreach (var file in files)      { if (TryRemoveFile(file))     changed = true; }
        
        if (changed)
        {
            AssetDatabase.Refresh();
            Debug.Log("[EcoModKit] Removed Unity default tutorial assets.");
        }
    }

    /// <summary> Safe removal of file </summary>
    static bool TryRemoveFile(string path)
    {
        if (!File.Exists(path)) return false;

        try
        {
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + ".meta");
            return true;
        }
        catch { return false; }
    }

    /// <summary> Safe removal of directory </summary>
    static bool TryRemoveDirectory(string path)
    {
        if (!Directory.Exists(path)) return false;

        try
        {
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + ".meta");
            return true;
        }
        catch { return false; }
    }
}

#endif
