using System.Collections;
using UnityEngine;

namespace TJ.Loader
{
    /// <summary>
    /// Shader加载器
    /// </summary>
    public class ShaderLoader
    {
        public static ShaderLoader Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ShaderLoader();
                }

                return s_Instance;
            }
        }

        private const string m_ShaderName = "Shaders";

        private static ShaderLoader s_Instance = null;

        private AssetLoader m_ShaderAssetLoader = new AssetLoader();

        public void InitShaderLoader(string strBundlePath)
        {
            m_ShaderAssetLoader.InitLoader("ShaderLoader", strBundlePath);
        }

        /// <summary>
        /// 加载所有shader
        /// </summary>
        public IEnumerator LoadAllShaderSync()
        {
            IEnumerator itor = m_ShaderAssetLoader.LoadAssetSync(m_ShaderName, AssetBundleType.Shader.ToString().ToLower(), true);
            while (itor.MoveNext())
            {
                yield return null;
            }

            if (m_ShaderAssetLoader.GetAllAsset(m_ShaderName) != null)
            {
                Shader.WarmupAllShaders();
            }
            else
            {
                Debug.LogError("ShaderLoader load shader error, no assetpack" + m_ShaderName);
            }
        }

        /// <summary>
        /// 获取指定的Shader
        /// </summary>
        /// <param name="shaderName">shader名</param>
        public Shader GetShader(string shaderName)
        {
            AssetPack gtPack = m_ShaderAssetLoader.GetAssetPack(m_ShaderName);
            if (gtPack != null)
            {
                Object obj = gtPack.GetAsset(shaderName);
                if (obj != null)
                {
                    return (Shader)obj;
                }
                else
                {
                    Debug.LogError("ShaderLoader GetShader, obj can not be null. key : " + shaderName);
                }
            }
            else
            {
                Debug.LogError("ShaderLoader GetShader, AssetBundle can not be null. key : " + m_ShaderName);
            }

            return null;
        }
    }
}