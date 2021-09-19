﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Factories
{
    /// <summary>
    /// Initializes all factories in resources. This doesn't even need to be created in a scene!
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class FactoryInitializer
    {
        /// <summary>
        /// Initializes the factories before the splash screen loads
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Initialize()
        {   //Load all factories from resources.
            FactoryBase[] factories = Resources.LoadAll<FactoryBase>("Factories");
            //Loop over the loaded factories and initialize them
            foreach (FactoryBase f in factories)
                f.Initialize();
        }
    }
}