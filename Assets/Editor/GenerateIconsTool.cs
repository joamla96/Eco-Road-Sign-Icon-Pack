using System.Collections.Generic;
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
            var scene = SceneManager.GetSceneByName("EcoRoadSignIcons");

            var rootObjects = scene.GetRootGameObjects();
            var itemsObjects = rootObjects.First(x => x.GetScenePath() == "Items");

            var itemChildren = itemsObjects.GetChildren();

            foreach( var item in itemChildren)
            {
                Object.Destroy(item);
            }

        }
    }
}
