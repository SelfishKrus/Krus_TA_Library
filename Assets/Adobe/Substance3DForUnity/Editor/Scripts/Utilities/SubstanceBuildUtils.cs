using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    public class SubstanceBuildUtils
    {
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.StandaloneLinux64)
                OnPostprocessBuildLinux(pathToBuiltProject);
            else if (target == BuildTarget.StandaloneOSX)
                OnPostprocessBuildMac(pathToBuiltProject);

            SubstanceEditorEngine.instance.RefreshActiveInstances();
        }

        private static void OnPostprocessBuildLinux(string pathToBuiltProject)
        {
            var dataPath = pathToBuiltProject.Replace(".x86_64", "_Data");
            var pluginsPath = Path.Combine(dataPath, "Plugins");

            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);

            var enginePath = Path.GetDirectoryName(PlatformUtils.GetEnginePath());
            string[] files = Directory.GetFiles(enginePath);

            foreach (string s in files)
            {
                var extension = Path.GetExtension(s);

                if (string.Equals(extension, ".so") || string.Equals(extension, ".1"))
                {
                    var fileName = Path.GetFileName(s);
                    var testination = Path.Combine(pluginsPath, fileName);
                    File.Copy(s, testination, true);
                }
            }
        }

        private static void OnPostprocessBuildMac(string pathToBuiltProject)
        {
            var pluginsPath = Path.Combine(Path.Combine(pathToBuiltProject, "Contents"), "PlugIns");

            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);

            var enginePath = Path.GetDirectoryName(PlatformUtils.GetEnginePath());
            string[] files = Directory.GetFiles(enginePath);

            foreach (string s in files)
            {
                var extension = Path.GetExtension(s);

                if (string.Equals(extension, ".dylib") || string.Equals(extension, ".1"))
                {
                    var fileName = Path.GetFileName(s);
                    var testination = Path.Combine(pluginsPath, fileName);
                    File.Copy(s, testination, true);
                }
            }
        }
    }
}