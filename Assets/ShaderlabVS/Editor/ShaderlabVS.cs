using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShaderlabVS
{
    public class Shaderlab
    {
        [MenuItem("Tools/ShaderlabVS Pro/Download Visual Studio")]
        public static void DownloadVS()
        {
            Application.OpenURL("https://visualstudio.microsoft.com/");
        }

        [MenuItem("Tools/ShaderlabVS Pro/Online Manual")]
        public static void OpenOnlineManual()
        {
            Application.OpenURL("https://www.amlovey.com/shaderlabvs/");
        }

        [MenuItem("Tools/ShaderlabVS Pro/Star And Review")]
        public static void StarAndReview()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/186176?aid=1011lGoJ");
        }
    }
}