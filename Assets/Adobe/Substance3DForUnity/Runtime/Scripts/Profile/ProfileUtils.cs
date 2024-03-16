#define SUBSTANCE_PROFILE_ENABLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if SUBSTANCE_PROFILE_ENABLE

using UnityEngine.Profiling;

#endif

namespace Adobe.Substance
{
    internal static class ProfileUtils
    {
        internal static void BeginSample(string name)
        {
#if SUBSTANCE_PROFILE_ENABLE
            Profiler.BeginSample(name);
#endif
        }

        internal static void EndSample()
        {
#if SUBSTANCE_PROFILE_ENABLE
            Profiler.EndSample();
#endif
        }
    }
}