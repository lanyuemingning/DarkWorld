using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ResourceReferenceFinder : EditorWindow
{
    private List<Object> referencingAssets = new List<Object>();
    private Vector2 scrollPosition;

    [MenuItem("Assets/ZYT ASSETS/Find References", true)]
    private static bool ValidateSearchReference()
    {
        // 只在选中了对象且不是文件夹时才显示菜单项
        return Selection.activeObject != null && !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/ZYT ASSETS/Find References")]
    private static void SearchReference()
    {
        // 创建并打开资源引用查找窗口
        if (Selection.activeObject != null && !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject)))
        {
            GetWindow<ResourceReferenceFinder>("Resource Reference Finder").ReferenceFinder(Selection.activeObject);
        }
    }

    private void OnGUI()
    {
        // 显示搜索结果
        EditorGUILayout.LabelField("Search Results:");
        // 滚动视图开始
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginVertical();

        // 显示搜索结果
        for (int i = 0; i < referencingAssets.Count; i++)
        {
            EditorGUILayout.ObjectField(referencingAssets[i], typeof(Object), true, GUILayout.Width(300));
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        // 滚动视图结束
    }

    // 查找引用
    private void ReferenceFinder(Object targetResource)
    {
        /*referencingAssets.Clear();

        // 获取选择资源的 GUID
        string assetPath = AssetDatabase.GetAssetPath(targetResource);
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

        // 遍历项目中所有 Prefab 文件
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int length = guids.Length;
        for (int i = 0; i < length; i++)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            EditorUtility.DisplayCancelableProgressBar("Checking", filePath, (float)i / length);

            // 读取 Prefab 文件内容，检查是否包含选择资源的 GUID
            string content = File.ReadAllText(filePath);
            if (content.Contains(assetGuid))
            {
                // 如果包含，将该 Prefab 添加到结果列表中
                Object referencingAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object));
                referencingAssets.Add(referencingAsset);
            }
        }

        // 清除进度条
        EditorUtility.ClearProgressBar();*/

        referencingAssets.Clear();

        // 获取选择资源的 GUID
        string assetPath = AssetDatabase.GetAssetPath(targetResource);
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

        // 支持多种类型的文件（Prefab, AnimationClip, AnimatorController）
        List<string> searchExtensions = new List<string> { ".prefab", ".anim", ".controller", ".unity" };
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

        int checkedCount = 0;
        foreach (string path in allAssetPaths)
        {
            if (!path.StartsWith("Assets")) continue;

            string ext = Path.GetExtension(path).ToLower();
            if (!searchExtensions.Contains(ext)) continue;

            EditorUtility.DisplayCancelableProgressBar("Checking", path, (float)checkedCount / allAssetPaths.Length);

            try
            {
                string content = File.ReadAllText(path);
                if (content.Contains(assetGuid))
                {
                    Object referencingAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    referencingAssets.Add(referencingAsset);
                }
            }
            catch
            {
                // 处理可能的读取异常（如二进制文件）
            }

            checkedCount++;
        }

        EditorUtility.ClearProgressBar();
    }
}