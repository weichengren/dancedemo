using System.Collections.Generic;
using TJ.Common;
using TJ.Loader;
using UnityEditor;

namespace TJ.MyEditor
{

    public class BuildPrefabs
    {
        [MenuItem("Extension/BuildAsset/Prefabs All")]
        static void ExecuteSelected()
        {
            BundleHelper.BuildHelper(BuildAssets, "prefabs 打包完成");
        }

        private static void BuildAssets()
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            string assetPath = AssetBundleDir.ASSET_PATH_PREFABS;
            string parentPath = AssetBundleDir.BUNDLE_PATH_PREFABS;
            FileHelper.CreateFolder(parentPath, false, false);

            List<string> fileArr = FileHelper.GetFiles(assetPath, "*.*", true);
            for (int i = 0; i < fileArr.Count; ++i)
            {
                string filePath = fileArr[i];
                if (BundleHelper.GetAssetType(filePath) == AssetBundleType.Prefab)
                {
                    string strFileName = FileHelper.GetFileName(filePath);
                    builds.Add(new AssetBundleBuild
                    {
                        assetNames = new string[] { filePath },
                        assetBundleName = strFileName + "." + AssetBundleType.Prefab.ToString().ToLower()
                    });
                }
            }

            BuildPipeline.BuildAssetBundles(parentPath, builds.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}