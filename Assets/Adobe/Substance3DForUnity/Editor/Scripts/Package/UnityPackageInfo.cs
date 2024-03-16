using Adobe.Substance;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    internal static class UnityPackageInfo
    {
        public const string PackageName = "com.adobe.substanceforunity";

        private static UnityEditor.PackageManager.PackageInfo _cachedPackageInfo = null;

        private static string IconPath_256 => $"{PathUtils.SubstanceRootPath}/Editor/Assets/Logo_256.png";
        private static string IconPath_128 => $"{PathUtils.SubstanceRootPath}/Editor/Assets/Logo_128.png";
        private static string IconPath_64 = $"{PathUtils.SubstanceRootPath}/Editor/Assets/Logo_64.png";
        private static string IconPath_32 = $"{PathUtils.SubstanceRootPath}/Editor/Assets/Logo_32.png";

        private static Texture2D _cachedIcon256 = null;
        private static Texture2D _cachedIcon128 = null;
        private static Texture2D _cachedIcon64 = null;
        private static Texture2D _cachedIcon32 = null;

        public static UnityEditor.PackageManager.PackageInfo GetPackageInfo()
        {
#pragma warning disable 162
            if (!PathUtils.UsingPackageManager)
                return null;
#pragma warning restore 162

            if (_cachedPackageInfo != null)
                return _cachedPackageInfo;

            var packageInfoRequest = UnityEditor.PackageManager.Client.List(true);

            while (packageInfoRequest.Status == UnityEditor.PackageManager.StatusCode.InProgress)
                continue;

            var packages = packageInfoRequest.Result?.FirstOrDefault(a => a.name == PackageName);

            if (packages != null)
                _cachedPackageInfo = packages;

            return _cachedPackageInfo;
        }

        public static Texture2D GetSubstanceIcon(int width, int height)
        {
            if (width != height)
                return GetSubstanceIconResized(width, height);

            switch (width)
            {
                case 256:
                    return GetSubstanceIcon256();

                case 128:
                    return GetSubstanceIcon128();

                case 64:
                    return GetSubstanceIcon64();

                case 32:
                    return GetSubstanceIcon32();

                default:
                    return GetSubstanceIconResized(width, height);
            }
        }

        public static Texture2D GetSubstanceIconResized(int width, int height)
        {
            var icon = GetSubstanceIcon256();
            Texture2D tex = new Texture2D(width, height);
            EditorUtility.CopySerialized(icon, tex);
            return tex;
        }

        public static Texture2D GetSubstanceIcon256()
        {
            if (_cachedIcon256 != null)
                return _cachedIcon256;

            _cachedIcon256 = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath_256);
            return _cachedIcon256;
        }

        public static Texture2D GetSubstanceIcon128()
        {
            if (_cachedIcon128 != null)
                return _cachedIcon128;

            _cachedIcon128 = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath_128);
            return _cachedIcon128;
        }

        public static Texture2D GetSubstanceIcon64()
        {
            if (_cachedIcon64 != null)
                return _cachedIcon64;

            _cachedIcon64 = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath_64);
            return _cachedIcon64;
        }

        public static Texture2D GetSubstanceIcon32()
        {
            if (_cachedIcon32 != null)
                return _cachedIcon32;

            _cachedIcon32 = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath_32);
            return _cachedIcon32;
        }
    }
}