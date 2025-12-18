using System.Collections;
using TJ.Common;
using UnityEngine;

namespace TJ.Loader
{
    public class PrefabsLoader
    {
        public static PrefabsLoader Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new PrefabsLoader();
                }
                return s_Instance;
            }
        }

        private static PrefabsLoader s_Instance = null;
        private AssetLoader m_PrefabsLoader = new AssetLoader();

        public void InitPrefabsLoader()
        {
            m_PrefabsLoader.InitLoader("PrefabsLoader", GameFunc.GetBundleResPath() + AssetBundleDir.BUNDLE_PATH_PREFABS);
        }

        /// <summary>
        /// 加载prefab
        /// </summary>
        /// <param name="prefabName">prefab名</param>
        public IEnumerator LoadPrefabSync(string prefabName)
        {
            IEnumerator itor = m_PrefabsLoader.LoadAssetSync(prefabName, AssetBundleType.Prefab.ToString().ToLower(), false);
            while (itor.MoveNext())
            {
                yield return null;
            }
        }

        /// <summary>
        /// 异步加载prefab
        /// </summary>
        public void LoadPrefabAsync(string prefabName, Callback<string> callBack)
        {
            m_PrefabsLoader.LoadAssetAsync(prefabName, AssetBundleType.Prefab.ToString().ToLower(), false, callBack);
        }

        /// <summary>
        /// 获取prefab文件
        /// </summary>
        /// <param name="prefabName">prefab名</param>
        public GameObject GetPrefab(string prefabName)
        {
            Object obj = m_PrefabsLoader.GetMainAsset(prefabName);
            if (obj != null)
            {
                return (GameObject)obj;
            }

            return null;
        }

        /// <summary>
        /// 释放prefab
        /// </summary>
        /// <param name="prefabName">prefab音乐名</param>
        public void UnloadPrefab(string prefabName)
        {
            m_PrefabsLoader.ReleaseAsset(prefabName, null, true);
        }

        public void UnloadPrefab(string prefabName, Callback<string> callBack)
        {
            m_PrefabsLoader.ReleaseAsset(prefabName, callBack, true);
        }
    }
}