using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace TJ.Loader
{
    /// <summary>
    /// 文本文件加载器
    /// </summary>
    public class TextLoader
    {
        public static TextLoader Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new TextLoader();
                }

                return s_Instance;
            }
        }

        private static TextLoader s_Instance = null;

        private string m_strBundlePath = null;  // 资源路径;

        private Dictionary<string, TxtAssetPack> m_dicTxtAssetPack = new Dictionary<string, TxtAssetPack>();

        public void InitTextLoader(string strBundlePath)
        {
            m_strBundlePath = strBundlePath;
        }

        public void LoadText(string txtName, string strAssetExtension, string childDir)
        {
            if (!m_dicTxtAssetPack.ContainsKey(txtName))
            {
                m_dicTxtAssetPack.Add(txtName, new TxtAssetPack());
                m_dicTxtAssetPack[txtName].InitAssetPack(m_strBundlePath + childDir + txtName + "." + strAssetExtension);
            }
            m_dicTxtAssetPack[txtName].LoadAssetPack();
        }

        public IEnumerator LoadTextSymc(string txtName, string strAssetExtension)
        {
            if (!m_dicTxtAssetPack.ContainsKey(txtName))
            {
                m_dicTxtAssetPack.Add(txtName, new TxtAssetPack());
                m_dicTxtAssetPack[txtName].InitAssetPack(m_strBundlePath + txtName + "." + strAssetExtension);
            }
            IEnumerator itor = m_dicTxtAssetPack[txtName].LoadAssetPackSync();
            while (itor.MoveNext())
            {
                yield return null;
            }
        }

        public byte[] GetBuffer(string txtName)
        {
            if (m_dicTxtAssetPack.ContainsKey(txtName))
            {
                return m_dicTxtAssetPack[txtName].DataBytes;
            }
            return null;
        }

        /// <summary>
        /// 释放文本文件
        /// </summary>
        public void UnloadText(string txtName)
        {
            if (m_dicTxtAssetPack.ContainsKey(txtName))
            {
                m_dicTxtAssetPack[txtName].UnLoadAssetPack();
                if (m_dicTxtAssetPack[txtName].RefCount == 0)
                {
                    m_dicTxtAssetPack.Remove(txtName);
                }
            }
            else
            {
                Debug.LogError("TextLoader UnloadText, m_dicTxtAssetPack not ContainsKey ： " + txtName);
            }
        }
    }

    public class TxtAssetPack
    {
        private byte[] m_dataBytes = null;
        private int m_nRefCount = 0;

        private string m_strFilePath = null;

        public void InitAssetPack(string strFilePath)
        {
            m_strFilePath = strFilePath;
        }

        public void LoadAssetPack()
        {
            if (m_nRefCount == 0)
            {
                if (m_strFilePath.IndexOf("file://") == -1)
                {
                    m_strFilePath = "file://" + m_strFilePath;
                }
                UnityWebRequest webRequest = UnityWebRequest.Get(m_strFilePath);
                webRequest.SendWebRequest();
                while (!webRequest.isDone)
                {

                }

                if (string.IsNullOrEmpty(webRequest.error))
                {
                    DownloadHandler handler = webRequest.downloadHandler;
                    using (StreamReader fs = new StreamReader(new MemoryStream(handler.data)))
                    {
                        m_dataBytes = new byte[fs.BaseStream.Length];
                        fs.BaseStream.Read(m_dataBytes, 0, (int)fs.BaseStream.Length);
                        fs.Close();
                    }
                }
                else
                {
                    Debug.LogError("TxtAssetPack LoadAssetPack, File not exists : " + m_strFilePath);
                }
                webRequest.Dispose();
            }
            m_nRefCount++;
        }

        public IEnumerator LoadAssetPackSync()
        {
            if (m_nRefCount == 0)
            {
                if (m_strFilePath.IndexOf("file://") == -1)
                {
                    m_strFilePath = "file://" + m_strFilePath;
                }
                UnityWebRequest webRequest = UnityWebRequest.Get(m_strFilePath);
                webRequest.SendWebRequest();
                while (!webRequest.isDone)
                {
                    yield return null;
                }

                if (string.IsNullOrEmpty(webRequest.error))
                {
                    DownloadHandler handler = webRequest.downloadHandler;
                    using (StreamReader fs = new StreamReader(new MemoryStream(handler.data)))
                    {
                        m_dataBytes = new byte[fs.BaseStream.Length];
                        fs.BaseStream.Read(m_dataBytes, 0, (int)fs.BaseStream.Length);
                        fs.Close();
                    }
                }
                else
                {
                    Debug.LogError("TxtAssetPack LoadAssetPack, File not exists : " + m_strFilePath);
                }
                webRequest.Dispose();
            }
            m_nRefCount++;
        }

        public void UnLoadAssetPack()
        {
            m_nRefCount--;
            if (m_nRefCount <= 0)
            {
                m_dataBytes = null;
            }
        }

        public byte[] DataBytes
        {
            get
            {
                return m_dataBytes;
            }
        }

        public int RefCount
        {
            get
            {
                return m_nRefCount;
            }
        }
    }
}