using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizonsTemplate
{
    public interface INewHorizons
    {
        void Create(Dictionary<string, object> config);

        void LoadConfigs(IModBehaviour mod);

        GameObject GetPlanet(string name);
    }
}
