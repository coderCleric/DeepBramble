using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity;
using UnityEngine;
using System.Threading.Tasks;

namespace DeepBramble
{
    /**
     * Simple little class to help redecorate various things throughout the mod
     */
    class DecorHelper
    {
        /**
         * Code that is blatantly stolen from Quantum Space Buddies, originally written by JohnCorby & Nebula
         * 
         * @param target The gameobject to fix
         */
        public void FixShaders(GameObject target)
        {
			//Check to ensure target != null
			if (target == null)
			{
				DeepBramble.debugPrint("Target doesn't exist");
				return;
			}

            //Loop through each renderer & material in the object
            foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
					//Safety check to ensure material exists
					if (material == null)
					{
						DeepBramble.debugPrint("Material doesn't exist");
						continue;
					}

					//Find the actual shader, ensure that it exists
					Shader originalShader = material.shader;
					var replacementShader = Shader.Find(material.shader.name);
					if (replacementShader == null)
					{
						DeepBramble.debugPrint("Replacement shader not found");
						continue;
					}

					if(originalShader == replacementShader)
                    {
						DeepBramble.debugPrint("Shaders match");
                    }

					// preserve override tag and render queue (for Standard shader)
					// keywords and properties are already preserved
					if (material.renderQueue != material.shader.renderQueue)
					{
						var renderType = material.GetTag("RenderType", false);
						var renderQueue = material.renderQueue;
						material.shader = replacementShader;
						material.SetOverrideTag("RenderType", renderType);
						material.renderQueue = renderQueue;
						DeepBramble.debugPrint("Shader replaced");
					}
					else
					{
						material.shader = replacementShader;
						DeepBramble.debugPrint("Shader replaced");
					}
				}
            }
        }

		/**
		 * Applies the various decor fixes that are required throughout the mod
		 */
		public void FixDecor()
        {
		}
    }
}
