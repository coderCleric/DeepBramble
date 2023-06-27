using DeepBramble.BaseInheritors;
using DeepBramble.Ditylum;
using DeepBramble.MiscBehaviours;
using DeepBramble.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    public static class ForgottenLocator
    {
        //Things that Patches needs to function
        public static GameObject brambleSingularity = null;
        public static GameObject eyeHologram = null;
        public static InnerFogWarpVolume largeLabEntrance = null;
        public static InnerFogWarpVolume smallLabEntrance = null;
        public static OuterFogWarpVolume languageOuterWarp = null;
        public static KevinController registeredKevin = null;
        public static List<BlockableQuantumSocket> blockableSockets = new List<BlockableQuantumSocket>();
        public static HazardVolume hotNodeHazard = null;
        public static OuterFogWarpVolume dilationOuterWarp = null;
        public static DilatedDitylumManager dilatedDitylum = null;

        //Things that the main class needs to know
        public static GameObject startDimensionObject = null;
        public static AssetBundle titleBundle = null;
        public static PlayerAudioController playerAudioController = null;

        //Things that need to know each other to load properly
        public static BlockableQuantumObject quantumRock = null;
        public static BlockableQuantumSocket specialSocket = null;

        public static LightFadeGroup lavaLightFadeGroup = null;
        public static LightFadeTrigger heartLightFadeTrigger = null;

        public static Material greenTreeMat = null;
        public static Transform heartDimensionSector = null;
    }
}
