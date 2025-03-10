﻿using DeepBramble.MiscBehaviours;
using NewHorizons.Utility.Files;
using UnityEngine;

namespace DeepBramble.Helpers
{
    public static class PostCreditsHelper
    {
        public static AssetBundle leviathanBundle = null;

        public static void LoadEndingAdditions()
        {
            //Load the asset bundle
            GameObject leviathan = leviathanBundle.LoadAsset<GameObject>("Assets/Prefabs/props/end_leviathan.prefab");

            //Make the game object for the dragon
            Transform leviathanParent = GameObject.Find("PostCreditsScene/Canvas").transform;
            if (leviathan == null)
            {
                DeepBramble.debugPrint("Couldn't load leviathan object");
                return;
            }
            leviathan = GameObject.Instantiate(leviathan, leviathanParent);
            leviathan.name = "Leviathan";

            //Make sure it's visible and in the right location
            AssetBundleUtilities.ReplaceShaders(leviathan);
            leviathan.transform.localPosition = new Vector3(EndSceneAddition.x, EndSceneAddition.y, EndSceneAddition.z);

            //Need to make sure it's in the right spot of the hierachy to render properly
            leviathan.transform.SetSiblingIndex(4);

            //Add the component
            leviathan.AddComponent<EndSceneAddition>();
            DeepBramble.debugPrint("Finished loading the ending additions.");
        }
    }
}
