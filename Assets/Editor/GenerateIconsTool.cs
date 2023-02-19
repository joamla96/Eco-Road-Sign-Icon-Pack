using System.Collections.Generic;
using System.IO;
using System.Linq;

using Unity.Jobs.LowLevel.Unsafe;

using UnityEditor;
using UnityEditor.SearchService;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eco.Client
{
    public class GenerateIconsTool
    {
        [MenuItem("Tools/Generate Icons")]
        private static void GenerateIcons()
        {
            AssetDatabase.Refresh();

            var iconsToGenerate = Directory.GetFiles("Assets/Images").Where(x => x.EndsWith(".png")).ToList();

            foreach (var icon in iconsToGenerate)
            {
                var asset = AssetImporter.GetAtPath(icon) as TextureImporter;
                asset.textureShape = TextureImporterShape.Texture2D;
                asset.textureType = TextureImporterType.Sprite;
                asset.alphaIsTransparency = true;
                
                asset.SaveAndReimport();
            }

            var scene = SceneManager.GetSceneByName("EcoRoadSignIcons");

            var rootObjects = scene.GetRootGameObjects();
            var itemsObjects = rootObjects.First(x => x.GetScenePath() == "Items");

            var itemChildren = itemsObjects.GetChildren();

            //foreach (var item in itemChildren)
            //{
            //    Object.DestroyImmediate(item);
            //}

        }
    }
}
