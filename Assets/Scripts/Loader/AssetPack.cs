using System.Collections;
using System.Collections.Generic;
using TJ.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace TJ.Loader
{
    public class AssetPack
    {
        public SingleCoroutine m_loadCoroutine = null;//加载下载用Coroutine;
        public Callback<string> m_CallBack = null;

        private AssetState m_AssetState = AssetState.Waitting;

        public string m_strAssetFullName = null;  // 资源名 - 带后缀名;
        public string m_strAssetName = null;      // 资源名 - key;

        public bool m_IsResident = false;
        public int m_AssetRefer = 0;
        public UnityWebRequest m_AssetWWW { get; set; } = null;
        public Object m_AssetObject = null;

        //public byte[] m_dataBytes = null;
        public AssetBundle m_AssetBundle = null;
        public Dictionary<string, Object> m_Assets = new Dictionary<string, Object>();  // 解压后的资源;
        public AudioClip m_AudioClip = null;
        //public string m_AssetText = null;

        public AssetPack(string strAssetName, string strAssetExtension, bool bResident)
        {
            m_strAssetName = strAssetName;
            m_strAssetFullName = strAssetName + "." + strAssetExtension;

            m_IsResident = bResident;
            m_AssetRefer = (bResident ? 0 : 1);
        }

        public bool AssetReady
        {
            get
            {
                if (m_AssetWWW != null && m_AssetWWW.isDone && m_AssetWWW.error == null)
                {
                    m_AssetBundle = DownloadHandlerAssetBundle.GetContent(m_AssetWWW);

                    m_AssetState = AssetState.Finish;
                    m_AssetWWW.Dispose();
                    m_AssetWWW = null;

                    if (m_AssetState == AssetState.HasRelease)
                    {
                        Debug.Log("Asset set to be released before ready, check logic if not on purpose");
#if UNITY_4_0
						m_AssetObject = null;
						UnloadAssetBundle(true);
#else
                        UnlaodAssetObject();
                        UnloadAssetBundle(true);
#endif

                        m_AssetRefer = (m_IsResident ? 0 : 1);
                    }
                }

                return m_AssetState == AssetState.Finish;
            }
        }

        public void ReleaseAsset(bool unloadFlag)
        {
            if (m_AssetState == AssetState.Finish)
            {
                m_AssetObject = null;
                UnloadAssetBundle(unloadFlag);
                m_Assets.Clear();
                m_AssetState = AssetState.HasRelease;
            }
        }

        public AssetState State
        {
            get
            {
                return m_AssetState;
            }
            set
            {
                m_AssetState = value;
            }
        }

        public IEnumerator LoadAsset(string assetDir)
        {
            m_AssetState = AssetState.LocalLoading;

            string assetPath = assetDir + m_strAssetFullName;
            if (assetPath.IndexOf("file://") == -1)
            {
                assetPath = "file://" + assetPath;
            }
            using (m_AssetWWW = UnityWebRequestAssetBundle.GetAssetBundle(assetPath))
            {
                yield return m_AssetWWW.SendWebRequest();

                while (m_AssetWWW == null || !m_AssetWWW.isDone)
                {
                    yield return null;
                }

                if (m_AssetWWW.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(m_AssetWWW.error);
                }
                else
                {
                    m_AssetBundle = DownloadHandlerAssetBundle.GetContent(m_AssetWWW);
                }

                if (m_AssetState == AssetState.HasRelease)
                {
                    Debug.Log("Asset set to be released before ready, check logic if not on purpose." + assetPath);

                    UnlaodAssetObject();
                    UnloadAssetBundle(true);
                }
                else
                {
                    m_AssetState = AssetState.Finish;
                }

                m_AssetWWW.Dispose();
                m_AssetWWW = null;
            }
        }



        public void UnloadAssetBundle(bool unloadFlag)
        {
            if (m_AssetBundle != null)
            {
                m_AssetBundle.Unload(unloadFlag);
                m_AssetBundle = null;
            }
        }

        public void UnlaodAssetObject()
        {
            if (m_AssetObject != null)
            {
                if (!(m_AssetObject is GameObject || m_AssetObject is Transform))
                {
                    Resources.UnloadAsset(m_AssetObject);
                }

                m_AssetObject = null;
            }
        }

        public Dictionary<string, Object> GetAllAsset()
        {
            if (m_Assets.Count > 0)
            {
                Debug.LogWarning("AssetPack GetAllAsset some assets have already loaded, check logic if not on purpose." + m_strAssetFullName);
            }

            if (m_AssetBundle == null)
            {
                Debug.LogWarning("AssetPack GetAllAsset failed, assetbundle is null, check logic if not on purpose." + m_strAssetFullName);
            }
            else
            {
                Object[] arr = m_AssetBundle.LoadAllAssets();
                Object asset = null;
                for (int i = 0; i < arr.Length; ++i)
                {
                    asset = arr[i];

                    if (asset == null) continue;
                    if (m_Assets.ContainsKey(asset.name))
                    {
                        Debug.LogWarning("AssetPack GetAllAsset same asset has been loaded." + asset.name);
                        continue;
                    }

                    m_Assets.Add(asset.name, asset);
                }
            }

            return m_Assets;
        }

        public Object GetAsset(string assetName)
        {
            if (m_Assets.Count == 0)
            {
                GetAllAsset();
            }

            if (m_Assets.ContainsKey(assetName))
            {
                return m_Assets[assetName];
            }
            return null;
        }
    }
}