using System.Collections;
using System.Collections.Generic;
using TJ.Common;
using UnityEngine;

namespace TJ.Loader
{
    /// <summary>
    /// 下载任务的状态;
    /// </summary>
    public enum AssetState : byte
    {
        Waitting,       //等待下载中;
        DownLoading,    //下载中;
        LocalLoading,   //本地加载中;
        Finish,         //完成下载或加载;
        HasRelease,     //已被释放;
    }

    public class AssetLoader
    {
        /// <summary>
        /// 加载信息节点;
        /// </summary>
        class AssetLoadInfo
        {
            public int m_nRefer = 1;//加载任务计数;
            public List<Callback<string>> m_listCallBack = new List<Callback<string>>();//各个界面回调;
            public SingleCoroutine m_loadCoroutine = null;//加载下载用Coroutine;
        }

        string m_strAssetDir = "";

        Dictionary<string, AssetPack> m_AllAssets = new Dictionary<string, AssetPack>();

        private GameObject m_objLoaderCoroutine = null;//Coroutine节点;
        private Queue<SingleCoroutine> m_qCoroutineInFree = new Queue<SingleCoroutine>();//Coroutine管理,保存空闲的Coroutine;

        /// <summary>
        /// 获取一个空闲Coroutine;
        /// </summary>
        private SingleCoroutine GetAssetLoadCoroutine(string strKey)
        {
            if (!m_AllAssets.ContainsKey(strKey))
            {
                Debug.LogWarning("DownLoadControl GetDownLoadCoroutine, Don't Has Key: " + strKey + " ??");
                return null;
            }
            if (m_AllAssets[strKey].m_loadCoroutine != null)
            {
                Debug.LogWarning("DownLoadControl GetDownLoadCoroutine, Already has Coroutine : " + strKey);
                return null;
            }

            SingleCoroutine gtFreeCoroutine = null;
            if (m_qCoroutineInFree.Count != 0)
            {
                gtFreeCoroutine = m_qCoroutineInFree.Dequeue();//踢出队列;
            }
            if (gtFreeCoroutine == null)
            {
                gtFreeCoroutine = m_objLoaderCoroutine.AddComponent<SingleCoroutine>();
            }
            m_AllAssets[strKey].m_loadCoroutine = gtFreeCoroutine;

            return gtFreeCoroutine;
        }

        /// <summary>
        /// 释放一个在用的Coroutine;
        /// </summary>
        private void ReleaseDownLoadCoroutine(string strKey)
        {
            if (m_AllAssets.ContainsKey(strKey))
            {
                SingleCoroutine downLoadCoroutine = m_AllAssets[strKey].m_loadCoroutine;
                m_AllAssets[strKey].m_loadCoroutine = null;

                if (downLoadCoroutine != null)
                {
                    downLoadCoroutine.StopAllCoroutines();//必须立即结束;
                    m_qCoroutineInFree.Enqueue(downLoadCoroutine);//放入空闲队列里;
                }
            }
        }

        public void InitLoader(string strLoaderName, string strBundlePath)
        {
            m_strAssetDir = strBundlePath;

            //创建Coroutine节点;
            GameObject gtGroupObj = GameObject.Find("LoaderCoroutineGroup");
            if (gtGroupObj == null)
            {
                gtGroupObj = new GameObject("LoaderCoroutineGroup");
                GameObject.DontDestroyOnLoad(gtGroupObj);
            }
            m_objLoaderCoroutine = new GameObject(strLoaderName);//Coroutine节点;
            m_objLoaderCoroutine.transform.parent = gtGroupObj.transform;
            GameObject.DontDestroyOnLoad(m_objLoaderCoroutine.transform.root);
        }

        /// <summary>
        /// 对外接口异步下载;
        /// </summary>
        public void LoadAssetAsync(string assetName, string strAssetExtension, bool bResident, Callback<string> callBack)
        {
            LoadAsset(assetName, strAssetExtension, null, bResident, callBack);
        }

        public void LoadAssetAsync(string assetName, string strAssetExtension, string childDir, bool bResident, Callback<string> callBack)
        {
            LoadAsset(assetName, strAssetExtension, childDir, bResident, callBack);
        }

        /// <summary>
        /// 对外接口同步下载;
        /// </summary>
        public IEnumerator LoadAssetSync(string assetName, string strAssetExtension, bool bResident)
        {
            IEnumerator itor = LoadAssetSync(assetName, strAssetExtension, null, bResident);
            while (itor.MoveNext())
            {
                yield return null;
            }
        }

        public IEnumerator LoadAssetSync(string assetName, string strAssetExtension, string childDir, bool bResident)
        {
            LoadAsset(assetName, strAssetExtension, childDir, bResident, null);

            while (!ChechLoadReady(assetName))
            {
                yield return null;
            }
        }

        private bool ChechLoadReady(string assetName)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                return assetPack.AssetReady;
            }
            return true;
        }

        /// <summary>
        /// 内部下载接口;
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="childDir">附加路径</param>
        /// <param name="bResident">是否常驻</param>
        /// <param name="callBack">回调函数</param>
        private void LoadAsset(string assetName, string strAssetExtension, string childDir, bool bResident, Callback<string> callBack)
        {
            AssetPack assetPack = null;
            if (m_AllAssets.ContainsKey(assetName))
            {
                assetPack = m_AllAssets[assetName];
                if (!assetPack.m_IsResident)
                {
                    if (bResident)
                    {
                        assetPack.m_IsResident = true;
                        assetPack.m_AssetRefer = 0;
                    }
                    else
                    {
                        assetPack.m_AssetRefer++;
                    }
                }
                assetPack.m_CallBack += callBack;

                if (assetPack.AssetReady)
                {
                    if (callBack != null)
                    {
                        callBack(assetName);
                    }
                }
            }
            else
            {
                assetPack = new AssetPack(assetName, strAssetExtension, bResident);
                m_AllAssets.Add(assetName, assetPack);
                assetPack.m_CallBack += callBack;

                SingleCoroutine gtDLC = GetAssetLoadCoroutine(assetName);
                if (gtDLC != null)
                {
                    gtDLC.StartCoroutine(LoadingAsset(assetName, childDir));
                }
                else
                {
                    Debug.LogError("AssetLoader LoadAssetAsync,Coroutine Obj can not be null.");
                }
            }
        }

        private IEnumerator LoadingAsset(string assetName, string childDir)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                IEnumerator itor = assetPack.LoadAsset(m_strAssetDir + childDir);
                while (itor.MoveNext())
                {
                    yield return null;
                }

                if (assetPack.m_CallBack != null)
                {
                    assetPack.m_CallBack(assetName);
                }

                if (!assetPack.m_IsResident && assetPack.m_AssetRefer <= 0)//非常驻资源,没有外部使用,并且下载完成的情况下;
                {
                    ReleaseAsset(assetName, null, false);
                }
                else
                {
                    ReleaseDownLoadCoroutine(assetName);
                }
            }
            else
            {
                Debug.LogError("AssetLoader LoadingAsset, m_AllAssets dont has key : " + assetName);
            }
        }

        /// <summary>
        /// 卸载资源;
        /// </summary>
        public bool ReleaseAsset(string assetName, Callback<string> callBack, bool isKeepLoading)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                if (assetPack != null)
                {
                    if (assetPack.m_CallBack != null)
                    {
                        assetPack.m_CallBack -= callBack;
                    }
                    if (!assetPack.m_IsResident)
                    {
                        assetPack.m_AssetRefer--;

                        if (assetPack.m_AssetRefer <= 0)//非常驻资源,没有外部使用,并且下载完成的情况下;
                        {
                            assetPack.m_CallBack = null;

                            ReleaseDownLoadCoroutine(assetName);//立即停止Coroutine;
                            assetPack.ReleaseAsset(true);
                            m_AllAssets.Remove(assetName);

                            return true;
                        }
                    }
                }
                else
                {
                    Debug.LogError("AssetLoader ReleaseAsset,AssetPack can not be null. " + assetName);
                }
            }

            return false;
        }

        /// <summary>
        /// 卸载所有资源;
        /// </summary>
        public void ReleaseAllAsset()
        {
            List<AssetPack> delAsset = new List<AssetPack>();
            foreach (KeyValuePair<string, AssetPack> kvp in m_AllAssets)
            {
                if (!kvp.Value.m_IsResident) // 常驻资源不卸载;
                {
                    delAsset.Add(kvp.Value);
                }
            }

            for (int i = 0; i < delAsset.Count; i++)
            {
                AssetPack gtPack = delAsset[i];
                if (gtPack != null)
                {
                    string strAssetName = gtPack.m_strAssetName;
                    ReleaseDownLoadCoroutine(strAssetName);//立即停止Coroutine;

                    m_AllAssets.Remove(strAssetName);

                    gtPack.m_AssetRefer--;
                    gtPack.ReleaseAsset(true);

                    if (gtPack.m_AssetRefer > 0)
                    {
                        Debug.LogError("AssetLoader ReleaseAllAsset, Release Asset Still Used: " + gtPack.m_strAssetFullName);
                    }

                    gtPack = null;
                }
                else
                {
                    Debug.LogError("AssetLoader ReleaseAllAsset, gtPack can not be null.");
                }
            }

            delAsset.Clear();
        }

        /// <summary>
        /// 获取下载内容的资源;
        /// </summary>
        public Object GetMainAsset(string assetName)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                if (assetPack.AssetReady)
                {
                    if (assetPack.m_AssetObject == null && assetPack.m_AssetBundle != null)
                    {
                        Object[] all = assetPack.m_AssetBundle.LoadAllAssets();
                        assetPack.m_AssetObject = all[0];
                    }

                    return assetPack.m_AssetObject;
                }
            }

            return null;
        }

        //public byte[] GetDataBytes(string assetName)
        //{
        //	if (m_AllAssets.ContainsKey(assetName))
        //	{
        //		AssetPack assetPack = m_AllAssets[assetName];
        //		if (assetPack.AssetReady)
        //		{
        //			return assetPack.m_dataBytes;
        //		}
        //	}
        //	return null;
        //}

        //public string GetAssetText(string assetName)
        //{
        //	if (m_AllAssets.ContainsKey(assetName))
        //	{
        //		AssetPack assetPack = m_AllAssets[assetName];
        //		if (assetPack.AssetReady)
        //		{
        //			return assetPack.m_AssetText;
        //		}
        //	}
        //	return null;
        //}

        /// <summary>
        /// 获取下载内容的资源;
        /// </summary>
        public AssetBundle GetMainAssetBundle(string assetName)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                if (assetPack.AssetReady)
                {
                    return assetPack.m_AssetBundle;
                }
            }

            return null;
        }

        public Dictionary<string, Object> GetAllAsset(string assetName)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                if (assetPack.AssetReady)
                {
                    return assetPack.GetAllAsset();
                }
            }

            return null;
        }

        public AssetPack GetAssetPack(string assetName)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                if (assetPack.AssetReady)
                {
                    return assetPack;
                }
            }
            return null;
        }

        public void UnloadAssetBundle(string assetName)
        {
            if (m_AllAssets.ContainsKey(assetName))
            {
                AssetPack assetPack = m_AllAssets[assetName];
                if (assetPack.AssetReady)
                {
                    assetPack.UnloadAssetBundle(false);
                }
                m_AllAssets.Remove(assetName);
            }
        }
    }
}