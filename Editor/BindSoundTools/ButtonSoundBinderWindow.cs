using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor.Events;
using System;

public class ButtonSoundBinderWindow : EditorWindow
{
    private static List<MethodInfo> methods;
    private static GameObject selectedPrefab;
    private static GameObject prefabRoot;
    private static List<Button> buttons;
    private static int[] selectedMethodIndexes;

    public static void ShowWindow()
    {
        if (!(Selection.activeObject is GameObject prefab))
        {
            Debug.LogWarning("请选择一个Prefab");
            return;
        }

        selectedPrefab = prefab;
        methods = ButtonSoundBinder.GetCachedMethods();
        if (methods == null || methods.Count == 0)
        {
            Debug.LogWarning("未找到静态无参方法");
            return;
        }

        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        buttons = new List<Button>(prefabRoot.GetComponentsInChildren<Button>(true));
        selectedMethodIndexes = new int[buttons.Count]; // 默认都为0

        GetWindow<ButtonSoundBinderWindow>("按钮音效绑定工具").Show();
    }

    private void OnGUI()
    {
        if (buttons == null || buttons.Count == 0)
        {
            EditorGUILayout.LabelField("未找到任何 Button");
            return;
        }

        EditorGUILayout.LabelField("为每个按钮选择一个方法绑定音效：", EditorStyles.boldLabel);

        for (int i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            if (btn == null) continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{btn.name}", GUILayout.Width(200));
            List<string> methodNames = methods.ConvertAll(m => m.Name);
            methodNames.Add("<Null>");
            selectedMethodIndexes[i] = EditorGUILayout.Popup(selectedMethodIndexes[i], methodNames.ToArray());

            if (GUILayout.Button("绑定", GUILayout.Width(80)))
            {
                BindMethodToButton(btn, selectedMethodIndexes[i]);

                SavePrefabOnly();
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("批量绑定所有按钮"))
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                BindMethodToButton(buttons[i], selectedMethodIndexes[i]);
            }

            SavePrefabOnly();
        }
    }

    private void BindMethodToButton(Button button, int methodIndex)
    {
        if (button == null) return;

        // 移除同类方法
        int count = button.onClick.GetPersistentEventCount();
        for (int i = count - 1; i >= 0; i--)
        {
            var m = button.onClick.GetPersistentMethodName(i);
            var target = button.onClick.GetPersistentTarget(i);
            if (target == null && methods.Exists(c => c.Name == m))
            {
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            }
        }

        if (methodIndex >= methods.Count)
        {
            Debug.Log($"按钮 {button.name} 未绑定任何方法");
            return;
        }

        var method = methods[methodIndex];
        if (method == null) return;

        // 添加
        UnityEventTools.AddPersistentListener(button.onClick, Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), method) as UnityEngine.Events.UnityAction);
        Debug.Log($"已绑定方法 {method.Name} 到按钮 {button.name}");
    }

    private void OnDestroy()
    {
        SaveAndCleanup();
    }

    private void SavePrefabOnly()
    {
        if (prefabRoot != null && selectedPrefab != null)
        {
            string path = AssetDatabase.GetAssetPath(selectedPrefab);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            Debug.Log("Prefab 保存完毕");
        }
    }

    private void SaveAndCleanup()
    {
        if (prefabRoot != null && selectedPrefab != null)
        {
            //string path = AssetDatabase.GetAssetPath(selectedPrefab);
            //PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            Debug.Log("Prefab 释放资源");
        }
    }
}

