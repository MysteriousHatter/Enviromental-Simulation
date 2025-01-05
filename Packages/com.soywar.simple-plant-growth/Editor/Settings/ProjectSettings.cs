#if UNITY_2021_2_OR_NEWER
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace SoyWar.SimplePlantGrowth.Editor.Settings
{
    [FilePath("ProjectSettings/Packages/com.soywar.simple-plant-growth/Settings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class ProjectSettings : ScriptableSingleton<ProjectSettings>
    {
        [SerializeField] private string _version;

        public bool Updated()
        {
            PackageInfo packageInfo = GetPackageInfo();
            
            if (packageInfo != null && _version != packageInfo.version)
            {
                _version = packageInfo.version;
                Save();

                return true;
            }

            return false;
        }

        public static PackageInfo GetPackageInfo()
        {
            return PackageInfo.FindForAssembly(Assembly.GetCallingAssembly());
        }

        private void Save()
        {
            Save(EditorSettings.serializationMode == SerializationMode.ForceText);
        }
    }
}
#endif