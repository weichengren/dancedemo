using System.Collections.Generic;
using TJ.Common;

namespace TJ.Loader
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum AssetBundleType : byte
    {
        Texture,    //贴图
        Sound,      //音频
        UI,         //界面
        Prefab,     //预设
        Material,   //材质
        Animation,  //动画
        Shader,     //着色器
        FBX,        //模型
        Unity,      //场景文件
        Script,     //脚本
        ScriptDLL,  //DLL脚本
        Asset,      //Asset文件，如NavMesh
        Txt,        //文本文件
        TTF,        //字体文件
        Atlas,
        Xml,
        Max,
    }


    public class DependencyAsset
    {
        public string AssetName = "";
        public string AssetPath = "";
        public string AssetSuffix = "";
        public AssetBundleType AssetType = AssetBundleType.Max;
        public int Depth = 0;
        public bool IsLeaf = false; //true-叶子节点
        public HashSet<string> ParentNodeSet = new HashSet<string>();   //所有父节点
    }


    public class AssetBundleDir
    {
        public const string LANGUAGE_CN = "cn/";//简体中文
        public const string LANGUAGE_EN = "en/";//英文

        //UI
        public const string ASSET_PATH_UI = "Assets/FairyGUI/";
        public const string ASSET_PATH_UI_PREFAB_UNPACK = "Assets/Scenes/Prefabs_UI/";
        public const string ASSET_PATH_UI_PREFAB = "Assets/AddOnResource/UI/PrefabDynamic/";

        //界面
        public const string BUNDLE_PATH_UI = "UIs/";

        //Shader
        public const string ASSET_PATH_SHADER = "Assets/Shaders/";
        public const string BUNDLE_PATH_SHADER = "Shaders/";

        //effect
        public static string ASSET_PATH_EFFECT = "Assets/Effect/";
        public static string BUNDLE_PATH_EFFECT = "Effect/";

        //music
        public static string ASSET_PATH_MUSIC = "Assets/Sounds/";
        public static string BUNDLE_PATH_MUSIC = "Sounds/";

        //font
        public const string ASSET_PATH_FONT = "Assets/Fonts/";
        public const string BUNDLE_PATH_FONT = "Fonts/";

        //prefab
        public const string ASSET_PATH_PREFABS = "Assets/Prefabs";
        public const string BUNDLE_PATH_PREFABS = "Prefabs";

        //静态数据
        public const string BUNDLE_PATH_STATIC_DATA = "StaticData/";
        //配置文件
        public const string BUNDLE_PATH_CONFIG = "Config/";

        //缓存数据
        public static string GAMING_DATA_CACHA = GameFunc.GetWritableDir() + "cacha.byte";
    }
}