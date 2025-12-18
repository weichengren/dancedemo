using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

///
/// 检查资源命名工具
///
public class CheckResNameTools
{
    private static readonly string[] strCommonNameArr = new string[] { "coat", "face", "foot", "pants" };

    private static bool IsCommonName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return strCommonNameArr.Any(s => name.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0);
    }

    // 返回在第二个下划线位置之前的子串（包含第一个下划线），
    // 例如: "a_b_c" -> "a_b"；"a_b" -> "a_b"；"a" -> "a"
    private static string PrefixBeforeSecondUnderscore(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var parts = name.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return parts[0] + "_" + parts[1];
        return name;
    }


    [MenuItem("Tools/Check Res Name")]
    public static void CheckResNameToolsMethod()
    {
        //检查文件夹名是否和里面的prefab名字一致，不一致则打印出来
        //检查prefab中引用的材质球和贴图名字是否和文件名一致，不一致则打印出来
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Art/Res" });


        var problemPrefabs = new HashSet<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = Path.GetFileNameWithoutExtension(path);
            string parentFolder = Path.GetFileName(Path.GetDirectoryName(path));

            if (!string.Equals(prefabName, parentFolder, System.StringComparison.Ordinal))
            {
                Debug.LogWarning($"Prefab name and parent folder mismatch: Prefab='{prefabName}' Path='{path}' ParentFolder='{parentFolder}'");
                problemPrefabs.Add(path);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            // 收集 prefab 中所有 Renderer 的材质
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (var rend in renderers)
            {
                var mats = rend.sharedMaterials;
                if (mats == null) continue;
                foreach (var mat in mats)
                {
                    if (mat == null) continue;

                    string matAssetPath = AssetDatabase.GetAssetPath(mat);
                    string matNameTrimmed = mat.name.Trim();

                    // 排除公用材质
                    if (IsCommonName(matNameTrimmed))
                        continue;


                    // 使用第二个下划线之前的前缀作为匹配前缀（如 a_b_c -> a_b）
                    string matPrefix = PrefixBeforeSecondUnderscore(matNameTrimmed);

                    if (!string.IsNullOrEmpty(matAssetPath) && Path.GetExtension(matAssetPath).Equals(".mat", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string matFileName = Path.GetFileNameWithoutExtension(matAssetPath);
                        string filePrefix = PrefixBeforeSecondUnderscore(matFileName);

                        if (!string.Equals(matPrefix, parentFolder, System.StringComparison.Ordinal))
                        {
                            Debug.LogWarning($"Material name mismatch in prefab '{prefabName}': Material.Name='{mat.name}' (prefix='{matPrefix}') File='{matFileName}' (prefix='{filePrefix}') AssetPath='{matAssetPath}' PrefabPath='{path}'");
                            problemPrefabs.Add(path);
                        }
                    }
                    else
                    {

                        Debug.Log($"Material embedded or not a separate .mat in prefab '{prefabName}': Name='{matNameTrimmed}' (prefix='{matPrefix}') PrefabPath='{path}'");
                        problemPrefabs.Add(path);
                    }
                }
            }
        }

        // 打印有问题的 prefab 汇总
        if (problemPrefabs.Count > 0)
        {
            Debug.LogWarning($"Found {problemPrefabs.Count} problem prefabs:\n{string.Join("\n", problemPrefabs)}");

            // 在当前打开的场景中实例化这些 prefab，方便在编辑器中查看（不会切换或创建新场景）
            var activeScene = EditorSceneManager.GetActiveScene();
            var parent = GameObject.Find("ProblemPrefabs");
            if (parent == null) parent = new GameObject("ProblemPrefabs");

            foreach (var p in problemPrefabs)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go == null) continue;
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(go, activeScene);
                if (inst != null)
                {
                    inst.transform.SetParent(parent.transform);
                    inst.transform.localPosition = Vector3.zero;
                }
            }

            // 标记当前场景为已修改
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log($"Instantiated problem prefabs into current scene: {activeScene.name}. Parent object: 'ProblemPrefabs'");
        }


    }

}
