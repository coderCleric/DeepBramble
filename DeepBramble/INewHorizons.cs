﻿using OWML.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DeepBramble
{
    /**
     * The interface for interacting with new horizons
     */
    public interface INewHorizons
    {
        void Create(Dictionary<string, object> config);

        void LoadConfigs(IModBehaviour mod);

        GameObject GetPlanet(string name);
        
        UnityEvent<string> GetStarSystemLoadedEvent(); 
        
        UnityEvent<string> GetChangeStarSystemEvent();

        string GetCurrentStarSystem();

        bool SetDefaultSystem(string name);
    }
}
