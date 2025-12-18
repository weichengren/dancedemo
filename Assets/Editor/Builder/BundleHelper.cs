using System.Collections.Generic;
using System.IO;
using TJ.Common;
using TJ.Loader;
using UnityEditor;
using UnityEngine;

namespace TJ.MyEditor
{
    /// <summary>
    ///  编译资源帮助类
    /// </summary>
    public class BundleHelper
    {
        public static void BuildHelper(System.Action func, string desc)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            func?.Invoke();

            sw.Stop();

            Debug.LogError($"{desc} 完成，用时{(double)sw.ElapsedMilliseconds / 1000}秒！！！！");
        }

        /// <summary>
        /// 打包资源
        /// </summary>
        /// <param name="assetPath">源路径</param>
        /// <param name="bundleParentPath">目标父路径</param>
        /// <param name="suffix">目标文件后缀</param>
        /// <param name="isLeaf">是否是叶子节点</param>
        /// <param name="needPopDepencance"></param>
        public static void BuildBundle(string assetPath, string bundlePath, bool isLeaf, bool popDependence)
        {
            if (!IsLegalAsset(FileHelper.GetFileName(bundlePath)))
            {
                Debug.LogError("BuildBundle error, asset name is not all lower," + assetPath);
            }

            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));

            BuildBundle(new Object[] { obj }, bundlePath, isLeaf, popDependence);

            obj = null;
        }

        public static void BuildBundle(Object[] assets, string bundlePath, bool isLeaf, bool popDependence)
        {
            if (assets == null || assets.Length < 1)
            {
                Debug.LogError("Build bundle error, not asset can be build." + bundlePath);
                return;
            }
            if (!IsLegalAsset(FileHelper.GetFileName(bundlePath)))
            {
                Debug.LogError("BuildBundles error, asset name is not all lower," + bundlePath);
            }

            // 解析输出目录与 bundle 文件名（如果 bundlePath 包含扩展名则视为文件路径）
            string outputFolder = bundlePath;
            string bundleFileName = null;
            try
            {
                if (!string.IsNullOrEmpty(Path.GetExtension(bundlePath)))
                {
                    outputFolder = Path.GetDirectoryName(bundlePath);
                    bundleFileName = Path.GetFileName(bundlePath);
                }
            }
            catch
            {
                outputFolder = bundlePath;
            }

            if (string.IsNullOrEmpty(outputFolder)) outputFolder = "Assets/StreamingAssets";
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

            // 收集 asset paths
            List<string> assetPaths = new List<string>();
            foreach (var a in assets)
            {
                if (a == null) continue;
                string p = AssetDatabase.GetAssetPath(a);
                if (string.IsNullOrEmpty(p)) continue;
                if (!assetPaths.Contains(p)) assetPaths.Add(p);
            }

            if (assetPaths.Count == 0)
            {
                Debug.LogError("No valid asset paths to build bundle: " + bundlePath);
                return;
            }

            if (string.IsNullOrEmpty(bundleFileName)) bundleFileName = Path.GetFileName(assetPaths[0]);

            var abb = new AssetBundleBuild();
            abb.assetBundleName = bundleFileName.ToLower();
            abb.assetNames = assetPaths.ToArray();

            Debug.Log($"Building assetbundle '{abb.assetBundleName}' to folder '{outputFolder}' with {abb.assetNames.Length} assets...");
            var builds = new AssetBundleBuild[] { abb };
            var manifest = BuildPipeline.BuildAssetBundles(outputFolder, builds, options, EditorUserBuildSettings.activeBuildTarget);
            if (manifest == null)
            {
                Debug.LogError("BuildAssetBundles failed for: " + bundlePath);
            }
            else
            {
                Debug.Log($"BuildAssetBundles finished. Output: {Path.Combine(outputFolder, abb.assetBundleName)}");
            }
        }



        /// <summary>
        /// 获取依赖资源的入度数
        /// </summary>
        public static Dictionary<string, List<DependencyAsset>> GetAssetDependencieRefs(string[] assetPaths)
        {
            Dictionary<string, List<DependencyAsset>> resMap = new Dictionary<string, List<DependencyAsset>>();

            //获取所有资源依赖关系
            Dictionary<string, string[]> allDepMap = new Dictionary<string, string[]>();
            foreach (string assetPath in assetPaths)
            {
                string[] depArr = AssetDatabase.GetDependencies(new string[] { assetPath });
                if (!allDepMap.ContainsKey(assetPath))
                {
                    allDepMap.Add(assetPath, depArr);
                }

                foreach (string assetDepPath in depArr)
                {
                    if (!allDepMap.ContainsKey(assetDepPath))
                    {
                        allDepMap.Add(assetDepPath, AssetDatabase.GetDependencies(new string[] { assetDepPath }));
                    }
                }
            }

            //生成依赖关系图
            foreach (string path in assetPaths)
            {
                Dictionary<string, DependencyAsset> depMap = new Dictionary<string, DependencyAsset>();
                GetAssetDependencies_Penetration(path, path, 0, allDepMap, ref depMap);

                //排序
                List<DependencyAsset> list = new List<DependencyAsset>(depMap.Values);
                for (int i = 0; i < list.Count; ++i)
                {
                    for (int j = i; j < list.Count; ++j)
                    {
                        DependencyAsset daTemp = null;
                        if (list[i].Depth < list[j].Depth)
                        {
                            daTemp = list[i];
                            list[i] = list[j];
                            list[j] = daTemp;
                        }
                    }
                }

                if (!resMap.ContainsKey(path))
                {
                    resMap.Add(path, list);
                }
            }

            return resMap;
        }

        static void GetAssetDependencies_Penetration(string assetPath, string parentNodePath, int depth, Dictionary<string, string[]> allDepMap,
            ref Dictionary<string, DependencyAsset> depMap)
        {
            string[] depArr = null; //当前资源所有依赖关系
            List<string> childrenList = new List<string>(); //所有间接依赖关系
            List<string> directChildrenList = new List<string>();   //直接依赖关系

            //获取当前资源依赖关系
            if (allDepMap.ContainsKey(assetPath))
            {
                depArr = allDepMap[assetPath];

                if (depArr.Length > 1)
                {
                    AddAssetDependeny(assetPath, parentNodePath, depth, false, ref depMap);
                }
            }
            else
            {
                Debug.LogError("Has not contain asset dependencies," + assetPath);
            }

            ++depth;

            //获取当前资源间接依赖关系
            foreach (string depPath in depArr)
            {
                if (!depPath.Equals(assetPath))
                {
                    if (allDepMap.ContainsKey(depPath))
                    {
                        string[] depArrChild = allDepMap[depPath];
                        foreach (string depChild in depArrChild)
                        {
                            if (!depChild.Equals(depPath) && !childrenList.Contains(depChild))
                            {
                                childrenList.Add(depChild);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Has not contain asset dependencies," + assetPath);
                    }
                }
            }

            //获取当前资源直接依赖关系
            foreach (string directDepChild in depArr)
            {
                if (!childrenList.Contains(directDepChild) && !directDepChild.Equals(assetPath))
                {
                    directChildrenList.Add(directDepChild);
                }
            }

            //处理
            foreach (string directDepChild in directChildrenList)
            {
                bool isLeaf = false;
                if (allDepMap.ContainsKey(directDepChild))
                {
                    isLeaf = (allDepMap[directDepChild].Length > 1) ? false : true;
                }
                else
                {
                    Debug.LogError("Has not contain asset dependencies," + directDepChild);
                }

                AddAssetDependeny(directDepChild, assetPath, depth, isLeaf, ref depMap);

                GetAssetDependencies_Penetration(directDepChild, assetPath, depth, allDepMap, ref depMap);
            }
        }

        static void AddAssetDependeny(string assetPath, string parentNodePath, int depth, bool isLeaf, ref Dictionary<string, DependencyAsset> depMap)
        {
            string assetPathTemp = assetPath.Replace('\\', '/');
            string sectionName = assetPathTemp.Substring(assetPathTemp.LastIndexOf('/') + 1);

            DependencyAsset dasset = null;
            if (depMap.ContainsKey(sectionName))
            {
                dasset = depMap[sectionName];
            }
            else
            {
                dasset = new DependencyAsset();
                dasset.AssetName = sectionName.Substring(0, sectionName.LastIndexOf('.'));
                dasset.AssetSuffix = sectionName.Substring(sectionName.LastIndexOf('.') + 1);
                dasset.AssetPath = assetPath;
                dasset.AssetType = BundleHelper.GetAssetType(assetPath);
                dasset.IsLeaf = isLeaf;
                dasset.Depth = depth;

                depMap.Add(sectionName, dasset);
            }

            dasset.ParentNodeSet.Add(parentNodePath);
        }

        /// <summary>
        ///  判断是否是符合标准的资源
        /// </summary>
        /// <param name="assetName">资源名</param>
        public static bool IsLegalAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return false;
            }

            return assetName.Equals(assetName.ToLower());
        }

        /// <summary>
        /// 获取文件类型
        /// </summary>
        public static AssetBundleType GetAssetType(string assetPath)
        {
            AssetBundleType arType = AssetBundleType.Max;

            if (IsImageFile(assetPath))
            {
                arType = AssetBundleType.Texture;
            }
            else if (IsAtlasFile(assetPath))
            {
                arType = AssetBundleType.Atlas;
            }
            else if (IsSoundFile(assetPath))
            {
                arType = AssetBundleType.Sound;
            }
            else if (IsPrefabFile(assetPath))
            {
                arType = AssetBundleType.Prefab;
            }
            else if (IsFBXFile(assetPath))
            {
                arType = AssetBundleType.FBX;
            }
            else if (IsScriptFile(assetPath))
            {
                arType = AssetBundleType.Script;
            }
            else if (IsScriptDLLFile(assetPath))
            {
                arType = AssetBundleType.ScriptDLL;
            }
            else if (IsMatFile(assetPath))
            {
                arType = AssetBundleType.Material;
            }
            else if (IsAnimationFile(assetPath))
            {
                arType = AssetBundleType.Animation;
            }
            else if (IsShaderFile(assetPath))
            {
                arType = AssetBundleType.Shader;
            }
            else if (IsSceneFile(assetPath))
            {
                arType = AssetBundleType.Unity;
            }
            else if (IsMetaFile(assetPath))
            {
                //Ignore this type file
            }
            else if (IsAssetFile(assetPath))
            {
                arType = AssetBundleType.Asset;
            }
            else if (IsFontFile(assetPath))
            {
                arType = AssetBundleType.TTF;
            }
            else
            {
                Debug.LogError("UnKnown asset Type, assetPath=" + assetPath);
            }

            return arType;
        }

        static bool IsMetaFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".meta"))
            {
                return true;
            }

            return false;
        }

        static bool IsSceneFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".unity"))
            {
                return true;
            }

            return false;
        }

        static bool IsShaderFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".shader"))
            {
                return true;
            }

            return false;
        }

        static bool IsPrefabFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".prefab"))
            {
                return true;
            }

            return false;
        }

        static bool IsAtlasFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".spriteatlas"))
            {
                return true;
            }

            return false;
        }

        static bool IsImageFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".png") || filePath.EndsWith(".bmp") || filePath.EndsWith(".tga")
                || filePath.EndsWith(".psd") || filePath.EndsWith(".dds") || filePath.EndsWith(".jpg"))
            {
                return true;
            }

            return false;
        }

        static bool IsSoundFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".ogg") || filePath.EndsWith(".wav") || filePath.EndsWith(".mp3"))
            {
                return true;
            }

            return false;
        }

        static bool IsScriptFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".js") || filePath.EndsWith(".cs") || filePath.EndsWith(".boo"))
            {
                return true;
            }

            return false;
        }

        static bool IsScriptDLLFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".dll"))
            {
                return true;
            }

            return false;
        }

        static bool IsMatFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".mat"))
            {
                return true;
            }

            return false;
        }

        static bool IsFBXFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".fbx"))
            {
                return true;
            }

            return false;
        }

        static bool IsAnimationFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".anim"))
            {
                return true;
            }

            return false;
        }

        static bool IsAssetFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".asset"))
            {
                return true;
            }

            return false;
        }

        static bool IsFontFile(string filePath)
        {
            filePath = filePath.ToLower();
            if (filePath.EndsWith(".ttf"))
            {
                return true;
            }

            return false;
        }
    }
}