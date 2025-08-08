using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Events;
using System;
using System.Collections.Generic;
using System.Reflection;

public class ButtonSoundUnbinderWindow : EditorWindow
{
    private static GameObject selectedPrefab;
    private static GameObject prefabRoot;
    private static List<Button> buttons;
    private static List<MethodInfo> soundMethods;
    private static string[] methodNames;
    private static string[] buttonBindings; // 记录每个按钮当前绑定的方法名，没绑定为null或空

    public static void ShowWindow()
    {
        if (!(Selection.activeObject is GameObject prefab))
        {
            Debug.LogWarning("请选择一个Prefab");
            return;
        }

        selectedPrefab = prefab;
        string prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
        prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        buttons = new List<Button>(prefabRoot.GetComponentsInChildren<Button>(true));

        soundMethods = ButtonSoundBinder.GetCachedMethods();
        if (soundMethods == null || soundMethods.Count == 0)
        {
            Debug.LogWarning("未找到静态无参音效方法，请先执行绑定窗口以缓存方法");
            return;
        }

        methodNames = new string[soundMethods.Count];
        for (int i = 0; i < soundMethods.Count; i++)
        {
            methodNames[i] = soundMethods[i].Name;
        }

        buttonBindings = new string[buttons.Count];
        // 获取每个按钮当前绑定的音效方法名称（最多一个）
        for (int i = 0; i < buttons.Count; i++)
        {
            buttonBindings[i] = GetButtonBoundMethodName(buttons[i]);
        }

        GetWindow<ButtonSoundUnbinderWindow>("按钮音效解绑工具").Show();
    }

    private void OnGUI()
    {
        if (buttons == null || buttons.Count == 0)
        {
            EditorGUILayout.LabelField("未找到任何 Button");
            return;
        }

        EditorGUILayout.LabelField("当前所有按钮绑定的音效方法", EditorStyles.boldLabel);

        for (int i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            if (btn == null) continue;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(btn.name, GUILayout.Width(200));
            EditorGUILayout.LabelField(string.IsNullOrEmpty(buttonBindings[i]) ? "<无绑定>" : buttonBindings[i], GUILayout.Width(150));

            if (!string.IsNullOrEmpty(buttonBindings[i]))
            {
                if (GUILayout.Button("解绑", GUILayout.Width(80)))
                {
                    UnbindMethodFromButton(btn, buttonBindings[i]);
                    buttonBindings[i] = null; // 更新显示
                    SavePrefab();
                }
            }
            else
            {
                GUILayout.Space(80); // 保持按钮宽度占位
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("批量解绑所有绑定音效"))
        {
            int unbindCount = 0;
            for (int i = 0; i < buttons.Count; i++)
            {
                if (!string.IsNullOrEmpty(buttonBindings[i]))
                {
                    UnbindMethodFromButton(buttons[i], buttonBindings[i]);
                    buttonBindings[i] = null;
                    unbindCount++;
                }
            }
            if (unbindCount > 0)
            {
                SavePrefab();
                Debug.Log($"批量解绑完成，共解绑 {unbindCount} 个按钮");
            }
            else
            {
                Debug.Log("没有检测到绑定的按钮，跳过解绑");
            }
        }
    }

    private static string GetButtonBoundMethodName(Button button)
    {
        int count = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
        {
            var target = button.onClick.GetPersistentTarget(i);
            var methodName = button.onClick.GetPersistentMethodName(i);
            // 只检测target==null且名字在音效方法列表中的绑定
            if (target == null && soundMethods.Exists(m => m.Name == methodName))
            {
                return methodName;
            }
        }
        return null;
    }

    private void UnbindMethodFromButton(Button button, string methodName)
    {
        int count = button.onClick.GetPersistentEventCount();
        for (int i = count - 1; i >= 0; i--)
        {
            var m = button.onClick.GetPersistentMethodName(i);
            var target = button.onClick.GetPersistentTarget(i);
            if (target == null && m == methodName)
            {
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            }
        }
        Debug.Log($"已解绑方法 {methodName} 从按钮 {button.name}");
    }

    private void SavePrefab()
    {
        if (prefabRoot != null && selectedPrefab != null)
        {
            string path = AssetDatabase.GetAssetPath(selectedPrefab);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            Debug.Log("Prefab 保存完毕");
        }
    }

    private void OnDestroy()
    {
        if (prefabRoot != null)
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            prefabRoot = null;
            Debug.Log("Prefab 资源已释放");
        }
    }
}
