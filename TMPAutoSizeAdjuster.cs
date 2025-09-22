using UnityEngine;
using UnityEditor;
using TMPro;

public class TMPAutoSizeAdjuster : EditorWindow
{
    [MenuItem("Tools/TMP AutoSize Adjuster")]
    public static void ShowWindow()
    {
        GetWindow<TMPAutoSizeAdjuster>("TMP AutoSize Adjuster");
    }

    // 添加持久化的开关状态
    private static bool enableMask = true;
    private static bool enableAutoFont = false;
    private static bool enableRaycast = true;

    private void OnEnable()
    {
        // 从EditorPrefs加载开关状态，使用不同的键名
        enableAutoFont = EditorPrefs.GetBool("TMPAutoSizeAdjuster_AutoFont", false);
        enableRaycast = EditorPrefs.GetBool("TMPAutoSizeAdjuster_Raycast", true);
        enableMask = EditorPrefs.GetBool("TMPAutoSizeAdjuster_Mask", true);
    }

    private void OnGUI()
    {
        GUILayout.Label("TextMeshPro 组件调整工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // AutoFont 设置区域
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("AutoSize 设置", EditorStyles.boldLabel);
        bool newEnableAutoFont = EditorGUILayout.Toggle("启用 AutoSize 功能", enableAutoFont);
        if (newEnableAutoFont != enableAutoFont)
        {
            enableAutoFont = newEnableAutoFont;
            EditorPrefs.SetBool("TMPAutoSizeAdjuster_AutoFont", enableAutoFont);
        }

        EditorGUI.BeginDisabledGroup(!enableAutoFont);
        if (GUILayout.Button("应用 AutoSize 设置", GUILayout.Height(25)))
        {
            ProcessAutoFont();
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.Label("启用AutoSize，设置Min=原大小-10，Max=原大小", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        // Raycast 设置区域
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("RaycastTarget 设置", EditorStyles.boldLabel);
        bool newEnableRaycast = EditorGUILayout.Toggle("启用 Raycast 功能", enableRaycast);
        if (newEnableRaycast != enableRaycast)
        {
            enableRaycast = newEnableRaycast;
            EditorPrefs.SetBool("TMPAutoSizeAdjuster_Raycast", enableRaycast);
        }

        if (GUILayout.Button(enableRaycast ? "启用所有 Raycast" : "禁用所有 Raycast", GUILayout.Height(25)))
        {
            ProcessRaycast();
        }
        GUILayout.Label(enableRaycast ? "将所有TMP文本的raycastTarget设为true" : "将所有TMP文本的raycastTarget设为false", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        // Mask 设置区域
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Maskable 设置", EditorStyles.boldLabel);
        bool newEnableMask = EditorGUILayout.Toggle("启用 Mask 功能", enableMask);
        if (newEnableMask != enableMask)
        {
            enableMask = newEnableMask;
            EditorPrefs.SetBool("TMPAutoSizeAdjuster_Mask", enableMask);
        }

        if (GUILayout.Button(enableMask ? "启用所有 Maskable" : "禁用所有 Maskable", GUILayout.Height(25)))
        {
            ProcessMask();
        }
        GUILayout.Label(enableMask ? "将所有TMP文本的maskable设为true" : "将所有TMP文本的maskable设为false", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        // 批量处理按钮
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("批量处理", EditorStyles.boldLabel);
        if (GUILayout.Button("一键处理所有设置", GUILayout.Height(30)))
        {
            ProcessAll();
        }
        GUILayout.Label("同时应用以上所有已启用的设置", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("请确保已选中一个预制体文件", MessageType.Info);
    }

    private GameObject GetSelectedPrefab()
    {
        GameObject activeObject = Selection.activeGameObject;
        if (activeObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选中一个预制体", "确定");
            return null;
        }

        PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(activeObject);
        PrefabInstanceStatus prefabStatus = PrefabUtility.GetPrefabInstanceStatus(activeObject);

        bool isPrefab = prefabType != PrefabAssetType.NotAPrefab ||
                       prefabStatus != PrefabInstanceStatus.NotAPrefab;

        if (!isPrefab)
        {
            EditorUtility.DisplayDialog("错误", "选中的对象不是预制体", "确定");
            return null;
        }

        return activeObject;
    }

    private TMP_Text[] GetTMPComponents(GameObject prefab)
    {
        TMP_Text[] tmpComponents = prefab.GetComponentsInChildren<TMP_Text>(true);
        if (tmpComponents.Length == 0)
        {
            EditorUtility.DisplayDialog("信息", "未找到TMP文本组件", "确定");
            return null;
        }
        return tmpComponents;
    }

    private void SavePrefab(GameObject prefab, int processedCount)
    {
        if (processedCount > 0)
        {
            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab));
            AssetDatabase.Refresh();
        }
    }

    private void ProcessAutoFont()
    {
        GameObject prefab = GetSelectedPrefab();
        if (prefab == null) return;

        TMP_Text[] tmpComponents = GetTMPComponents(prefab);
        if (tmpComponents == null) return;

        int processedCount = 0;
        int skippedCount = 0;

        foreach (TMP_Text tmpComponent in tmpComponents)
        {
            if (tmpComponent.enableAutoSizing)
            {
                skippedCount++;
                continue;
            }

            float originalSize = tmpComponent.fontSize;
            tmpComponent.enableAutoSizing = true;
            tmpComponent.fontSizeMin = Mathf.Max(1, originalSize - 10);
            tmpComponent.fontSizeMax = originalSize;

            processedCount++;
            EditorUtility.SetDirty(tmpComponent);
        }

        SavePrefab(prefab, processedCount);
        EditorUtility.DisplayDialog("AutoSize 完成",
            $"已处理: {processedCount} 个组件\n跳过: {skippedCount} 个已启用AutoSize的组件", "确定");
    }

    private void ProcessRaycast()
    {
        GameObject prefab = GetSelectedPrefab();
        if (prefab == null) return;

        TMP_Text[] tmpComponents = GetTMPComponents(prefab);
        if (tmpComponents == null) return;

        int processedCount = 0;

        foreach (TMP_Text tmpComponent in tmpComponents)
        {
            tmpComponent.raycastTarget = enableRaycast;
            processedCount++;
            EditorUtility.SetDirty(tmpComponent);
        }

        SavePrefab(prefab, processedCount);
        EditorUtility.DisplayDialog("Raycast 完成",
            $"已{(enableRaycast ? "启用" : "禁用")} {processedCount} 个组件的RaycastTarget", "确定");
    }

    private void ProcessMask()
    {
        GameObject prefab = GetSelectedPrefab();
        if (prefab == null) return;

        TMP_Text[] tmpComponents = GetTMPComponents(prefab);
        if (tmpComponents == null) return;

        int processedCount = 0;

        foreach (TMP_Text tmpComponent in tmpComponents)
        {
            tmpComponent.maskable = enableMask;
            processedCount++;
            EditorUtility.SetDirty(tmpComponent);
        }

        SavePrefab(prefab, processedCount);
        EditorUtility.DisplayDialog("Maskable 完成",
            $"已{(enableMask ? "启用" : "禁用")} {processedCount} 个组件的Maskable", "确定");
    }

    private void ProcessAll()
    {
        GameObject prefab = GetSelectedPrefab();
        if (prefab == null) return;

        TMP_Text[] tmpComponents = GetTMPComponents(prefab);
        if (tmpComponents == null) return;

        int autoFontCount = 0;
        int raycastCount = 0;
        int maskCount = 0;
        int skippedAutoFont = 0;

        foreach (TMP_Text tmpComponent in tmpComponents)
        {
            // 处理 AutoFont
            if (enableAutoFont && !tmpComponent.enableAutoSizing)
            {
                float originalSize = tmpComponent.fontSize;
                tmpComponent.enableAutoSizing = true;
                tmpComponent.fontSizeMin = Mathf.Max(1, originalSize - 10);
                tmpComponent.fontSizeMax = originalSize;
                autoFontCount++;
            }
            else if (enableAutoFont)
            {
                skippedAutoFont++;
            }

            // 处理 Raycast
            tmpComponent.raycastTarget = enableRaycast;
            raycastCount++;

            // 处理 Mask
            tmpComponent.maskable = enableMask;
            maskCount++;

            EditorUtility.SetDirty(tmpComponent);
        }

        SavePrefab(prefab, autoFontCount + raycastCount + maskCount);

        string message = $"处理完成!\n\n";
        if (enableAutoFont)
        {
            message += $"AutoSize: {autoFontCount} 个已处理, {skippedAutoFont} 个已跳过\n";
        }
        message += $"RaycastTarget: {raycastCount} 个已{(enableRaycast ? "启用" : "禁用")}\n";
        message += $"Maskable: {maskCount} 个已{(enableMask ? "启用" : "禁用")}";

        EditorUtility.DisplayDialog("批量处理完成", message, "确定");
    }
}