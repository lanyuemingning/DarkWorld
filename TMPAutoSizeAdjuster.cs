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

    // ��ӳ־û��Ŀ���״̬
    private static bool enableMask = true;
    private static bool enableAutoFont = false;
    private static bool enableRaycast = true;

    private void OnEnable()
    {
        // ��EditorPrefs���ؿ���״̬��ʹ�ò�ͬ�ļ���
        enableAutoFont = EditorPrefs.GetBool("TMPAutoSizeAdjuster_AutoFont", false);
        enableRaycast = EditorPrefs.GetBool("TMPAutoSizeAdjuster_Raycast", true);
        enableMask = EditorPrefs.GetBool("TMPAutoSizeAdjuster_Mask", true);
    }

    private void OnGUI()
    {
        GUILayout.Label("TextMeshPro �����������", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // AutoFont ��������
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("AutoSize ����", EditorStyles.boldLabel);
        bool newEnableAutoFont = EditorGUILayout.Toggle("���� AutoSize ����", enableAutoFont);
        if (newEnableAutoFont != enableAutoFont)
        {
            enableAutoFont = newEnableAutoFont;
            EditorPrefs.SetBool("TMPAutoSizeAdjuster_AutoFont", enableAutoFont);
        }

        EditorGUI.BeginDisabledGroup(!enableAutoFont);
        if (GUILayout.Button("Ӧ�� AutoSize ����", GUILayout.Height(25)))
        {
            ProcessAutoFont();
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.Label("����AutoSize������Min=ԭ��С-10��Max=ԭ��С", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        // Raycast ��������
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("RaycastTarget ����", EditorStyles.boldLabel);
        bool newEnableRaycast = EditorGUILayout.Toggle("���� Raycast ����", enableRaycast);
        if (newEnableRaycast != enableRaycast)
        {
            enableRaycast = newEnableRaycast;
            EditorPrefs.SetBool("TMPAutoSizeAdjuster_Raycast", enableRaycast);
        }

        if (GUILayout.Button(enableRaycast ? "�������� Raycast" : "�������� Raycast", GUILayout.Height(25)))
        {
            ProcessRaycast();
        }
        GUILayout.Label(enableRaycast ? "������TMP�ı���raycastTarget��Ϊtrue" : "������TMP�ı���raycastTarget��Ϊfalse", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        // Mask ��������
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Maskable ����", EditorStyles.boldLabel);
        bool newEnableMask = EditorGUILayout.Toggle("���� Mask ����", enableMask);
        if (newEnableMask != enableMask)
        {
            enableMask = newEnableMask;
            EditorPrefs.SetBool("TMPAutoSizeAdjuster_Mask", enableMask);
        }

        if (GUILayout.Button(enableMask ? "�������� Maskable" : "�������� Maskable", GUILayout.Height(25)))
        {
            ProcessMask();
        }
        GUILayout.Label(enableMask ? "������TMP�ı���maskable��Ϊtrue" : "������TMP�ı���maskable��Ϊfalse", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);

        // ��������ť
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("��������", EditorStyles.boldLabel);
        if (GUILayout.Button("һ��������������", GUILayout.Height(30)))
        {
            ProcessAll();
        }
        GUILayout.Label("ͬʱӦ���������������õ�����", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("��ȷ����ѡ��һ��Ԥ�����ļ�", MessageType.Info);
    }

    private GameObject GetSelectedPrefab()
    {
        GameObject activeObject = Selection.activeGameObject;
        if (activeObject == null)
        {
            EditorUtility.DisplayDialog("����", "����ѡ��һ��Ԥ����", "ȷ��");
            return null;
        }

        PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(activeObject);
        PrefabInstanceStatus prefabStatus = PrefabUtility.GetPrefabInstanceStatus(activeObject);

        bool isPrefab = prefabType != PrefabAssetType.NotAPrefab ||
                       prefabStatus != PrefabInstanceStatus.NotAPrefab;

        if (!isPrefab)
        {
            EditorUtility.DisplayDialog("����", "ѡ�еĶ�����Ԥ����", "ȷ��");
            return null;
        }

        return activeObject;
    }

    private TMP_Text[] GetTMPComponents(GameObject prefab)
    {
        TMP_Text[] tmpComponents = prefab.GetComponentsInChildren<TMP_Text>(true);
        if (tmpComponents.Length == 0)
        {
            EditorUtility.DisplayDialog("��Ϣ", "δ�ҵ�TMP�ı����", "ȷ��");
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
        EditorUtility.DisplayDialog("AutoSize ���",
            $"�Ѵ���: {processedCount} �����\n����: {skippedCount} ��������AutoSize�����", "ȷ��");
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
        EditorUtility.DisplayDialog("Raycast ���",
            $"��{(enableRaycast ? "����" : "����")} {processedCount} �������RaycastTarget", "ȷ��");
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
        EditorUtility.DisplayDialog("Maskable ���",
            $"��{(enableMask ? "����" : "����")} {processedCount} �������Maskable", "ȷ��");
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
            // ���� AutoFont
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

            // ���� Raycast
            tmpComponent.raycastTarget = enableRaycast;
            raycastCount++;

            // ���� Mask
            tmpComponent.maskable = enableMask;
            maskCount++;

            EditorUtility.SetDirty(tmpComponent);
        }

        SavePrefab(prefab, autoFontCount + raycastCount + maskCount);

        string message = $"�������!\n\n";
        if (enableAutoFont)
        {
            message += $"AutoSize: {autoFontCount} ���Ѵ���, {skippedAutoFont} ��������\n";
        }
        message += $"RaycastTarget: {raycastCount} ����{(enableRaycast ? "����" : "����")}\n";
        message += $"Maskable: {maskCount} ����{(enableMask ? "����" : "����")}";

        EditorUtility.DisplayDialog("�����������", message, "ȷ��");
    }
}