using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindUnusedAnimationsWindow : EditorWindow
{
    private List<string> unusedClips = new List<string>();
    private List<string> unusedControllers = new List<string>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Find Unused Animations")]
    public static void ShowWindow()
    {
        GetWindow<FindUnusedAnimationsWindow>("Find Unused Animations");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("开始扫描"))
        {
            ScanUnusedAnimations();
        }

        GUILayout.Space(10);

        if (unusedClips.Count == 0 && unusedControllers.Count == 0)
        {
            GUILayout.Label("没有未使用的动画片段或Animator控制器！");
            return;
        }

        // 新加的删除按钮
        if (GUILayout.Button("一键删除未使用的动画和Animator"))
        {
            if (EditorUtility.DisplayDialog("确认删除", "确定要删除所有未使用的 AnimationClip 和 AnimatorController 吗？此操作不可撤回！", "删除", "取消"))
            {
                DeleteUnusedAssets();
            }
        }

        GUILayout.Space(10);

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        GUIStyle redStyle = new GUIStyle(GUI.skin.button);
        redStyle.normal.textColor = Color.red;

        GUILayout.Label("未使用的 AnimationClips:", EditorStyles.boldLabel);
        foreach (var path in unusedClips)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(System.IO.Path.GetFileName(path), redStyle))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        GUILayout.Label("未使用的 AnimatorControllers:", EditorStyles.boldLabel);
        foreach (var path in unusedControllers)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(System.IO.Path.GetFileName(path), redStyle))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private void ScanUnusedAnimations()
    {
        unusedClips.Clear();
        unusedControllers.Clear();

        var clipGUIDs = AssetDatabase.FindAssets("t:AnimationClip");
        var controllerGUIDs = AssetDatabase.FindAssets("t:AnimatorController");

        HashSet<string> allDependencies = new HashSet<string>();
        var allAssetGUIDs = AssetDatabase.FindAssets("t:Prefab t:Scene t:ScriptableObject");

        foreach (var guid in allAssetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string[] dependencies = AssetDatabase.GetDependencies(path, true);
            foreach (var dep in dependencies)
            {
                allDependencies.Add(dep);
            }
        }

        foreach (var guid in clipGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!allDependencies.Contains(path))
            {
                unusedClips.Add(path);
            }
        }

        foreach (var guid in controllerGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!allDependencies.Contains(path))
            {
                unusedControllers.Add(path);
            }
        }

        Debug.Log($"扫描完成！找到 {unusedClips.Count} 个未使用AnimationClip，{unusedControllers.Count} 个未使用AnimatorController");
    }

    private void DeleteUnusedAssets()
    {
        foreach (var path in unusedClips)
        {
            AssetDatabase.DeleteAsset(path);
        }
        foreach (var path in unusedControllers)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.Refresh();
        Debug.Log("删除完成，资源已刷新！");

        // 清空列表
        unusedClips.Clear();
        unusedControllers.Clear();
    }
}
