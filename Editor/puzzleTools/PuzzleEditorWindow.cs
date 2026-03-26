using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class PuzzleEditorWindow : EditorWindow
{
    [SerializeField] private PuzzleGrid grid;
    [SerializeField] private float currentZoom = 1.0f;
    private const int BASE_TILE_SIZE = 50;
    private float TileSize => BASE_TILE_SIZE * currentZoom;

    private int gridWidth = 20, gridHeight = 20;
    private Vector2 paletteScroll, gridScroll;
    private Sprite selectedSprite;
    private List<Sprite> spritePalette = new List<Sprite>();
    
    private HashSet<Vector2Int> selectedCells = new HashSet<Vector2Int>();
    private bool isBoxSelecting = false;
    private Vector2 boxStartPos;
    private Rect selectionRect;

    private int currentGroupId = 1;
    private int activeLayer = 0; 

    private string spriteFolder = "Assets/PuzzleSprites";
    private string savePath = "Assets/PuzzlePrefabs";
    private string prefabName = "Puzzle";

    private Vector2Int lastPaintedCell = new Vector2Int(-1, -1);

    [MenuItem("Tools/Puzzle Editor")]
    static void Open() => GetWindow<PuzzleEditorWindow>("Puzzle Editor");

    void OnEnable() {
        if (grid == null) grid = new PuzzleGrid(gridWidth, gridHeight);
        grid.CheckInitialization();
        LoadSprites();
    }

    void RecordUndo(string label) => Undo.RegisterCompleteObjectUndo(this, label);

    void OnGUI() {
        if (grid == null) grid = new PuzzleGrid(gridWidth, gridHeight);
        grid.CheckInitialization();

        DrawTopControls();
        DrawPalette();
        DrawGrid();
        
        if (Event.current.type == EventType.MouseMove || isBoxSelecting || lastPaintedCell.x != -1) Repaint();
    }

    void DrawTopControls() {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        Rect sRect = EditorGUILayout.BeginHorizontal();
        spriteFolder = EditorGUILayout.TextField("Sprite Folder", spriteFolder);
        EditorGUILayout.EndHorizontal();
        HandleFolderDrop(sRect, ref spriteFolder, true);

        Rect pRect = EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
        EditorGUILayout.EndHorizontal();
        HandleFolderDrop(pRect, ref savePath, false);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        currentGroupId = EditorGUILayout.IntSlider("Group ID", currentGroupId, 1, 30, GUILayout.Width(220));
        
        GUILayout.Space(10);
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = activeLayer == 0 ? Color.cyan : originalColor;
        if (GUILayout.Button("Layer: Bottom", EditorStyles.toolbarButton)) activeLayer = 0;
        GUI.backgroundColor = activeLayer == 1 ? Color.cyan : originalColor;
        if (GUILayout.Button("Layer: Top", EditorStyles.toolbarButton)) activeLayer = 1;
        GUI.backgroundColor = originalColor;

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Clear All", EditorStyles.toolbarButton)) {
            if (EditorUtility.DisplayDialog("Confirm", "Clear Grid?", "Yes")) { 
                RecordUndo("Clear"); 
                grid = new PuzzleGrid(gridWidth, gridHeight); 
            }
        }
        if (GUILayout.Button("Export Prefab", EditorStyles.toolbarButton)) PuzzlePrefabExporter.Export(grid, savePath, prefabName);
        EditorGUILayout.EndHorizontal();
    }

    void HandleFolderDrop(Rect dropArea, ref string path, bool isSpriteFolder) {
        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition)) return;
        if (evt.type == EventType.DragUpdated) {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        } else if (evt.type == EventType.DragPerform) {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences) {
                string p = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(p)) {
                    path = p;
                    if (isSpriteFolder) LoadSprites();
                    GUI.changed = true;
                    break;
                }
            }
            evt.Use();
        }
    }

    void LoadSprites() {
        spritePalette.Clear();
        if (!AssetDatabase.IsValidFolder(spriteFolder)) return;
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolder });
        foreach (var guid in guids) {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
            if (s != null) spritePalette.Add(s);
        }
    }

    void DrawPalette() {
        paletteScroll = EditorGUILayout.BeginScrollView(paletteScroll, GUILayout.Height(110));
        EditorGUILayout.BeginHorizontal();
        foreach (var sp in spritePalette) {
            Rect r = GUILayoutUtility.GetRect(BASE_TILE_SIZE + 10, BASE_TILE_SIZE + 10);
            if (selectedSprite == sp) Handles.DrawSolidRectangleWithOutline(r, new Color(1, 1, 0, 0.1f), Color.yellow);
            if (GUI.Button(r, "", GUIStyle.none)) selectedSprite = sp;
            DrawSpriteCorrectly(r, sp, 0, false);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    void DrawGrid() {
        gridScroll = EditorGUILayout.BeginScrollView(gridScroll);
        float sw = gridWidth * TileSize, sh = gridHeight * TileSize;
        Rect rect = GUILayoutUtility.GetRect(sw, sh);
        rect.x += Mathf.Max(0, (position.width - sw) * 0.5f);

        Event e = Event.current;
        if (e.control && e.type == EventType.ScrollWheel) { currentZoom = Mathf.Clamp(currentZoom - e.delta.y * 0.05f, 0.2f, 3f); e.Use(); }
        
        HandleGridInputs(rect, e);

        GUI.Box(rect, "");
        DrawGridLines(rect);
        DrawLayer(rect, 0); 
        DrawLayer(rect, 1);

        if (isBoxSelecting) {
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(selectionRect, new Color(0, 0.8f, 1f, 0.1f), Color.white);
            Handles.EndGUI();
        }
        EditorGUILayout.EndScrollView();
    }

    void DrawGridLines(Rect rect) {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        for (int x = 0; x <= gridWidth; x++) Handles.DrawLine(new Vector2(rect.x + x * TileSize, rect.y), new Vector2(rect.x + x * TileSize, rect.y + gridHeight * TileSize));
        for (int y = 0; y <= gridHeight; y++) Handles.DrawLine(new Vector2(rect.x, rect.y + y * TileSize), new Vector2(rect.x + gridWidth * TileSize, rect.y + y * TileSize));
    }

    void DrawLayer(Rect gridRect, int layerIndex) {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                Rect r = new Rect(gridRect.x + x * TileSize, gridRect.y + (gridHeight - 1 - y) * TileSize, TileSize, TileSize);
                var tile = grid.GetTile(x, y, layerIndex);
                
                if (tile != null) {
                    DrawSpriteCorrectly(r, tile.sprite, tile.rotation, tile.isMirrored);
                }

                // 始终显示 Layer 0 的分组数字，并置于最顶层绘制
                if (layerIndex == 1) {
                    var bottomTile = grid.GetTile(x, y, 0);
                    if (bottomTile != null) {
                        Rect labelRect = new Rect(r.x + 2, r.y + 2, 22, 16);
                        EditorGUI.DrawRect(labelRect, new Color(0, 0, 0, 0.7f)); 
                        GUI.color = Color.white;
                        GUI.Label(labelRect, bottomTile.groupId.ToString(), EditorStyles.miniBoldLabel);
                        GUI.color = Color.white;
                    }

                    if (selectedCells.Contains(new Vector2Int(x, y))) {
                        Handles.DrawSolidRectangleWithOutline(r, new Color(1, 0, 0, 0.2f), Color.red);
                    }
                }
            }
        }
    }

    void HandleGridInputs(Rect rect, Event e) {
        Vector2 m = e.mousePosition;
        int gx = Mathf.FloorToInt((m.x - rect.x) / TileSize);
        int gy = gridHeight - 1 - Mathf.FloorToInt((m.y - rect.y) / TileSize);
        Vector2Int cur = new Vector2Int(gx, gy);

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && !e.control) {
            if (rect.Contains(m) && grid.InBounds(gx, gy)) {
                if (cur != lastPaintedCell) {
                    if (selectedSprite != null) {
                        RecordUndo("Place Tile");
                        
                        int targetGroupId = currentGroupId;
                        if (activeLayer == 1) {
                            var bTile = grid.GetTile(gx, gy, 0);
                            if (bTile != null) targetGroupId = bTile.groupId;
                        }

                        grid.SetTile(gx, gy, selectedSprite, 0, targetGroupId, activeLayer);
                        lastPaintedCell = cur;
                        selectedCells.Clear();
                        e.Use();
                    }
                }
            }
        }

        if (e.type == EventType.MouseDown && rect.Contains(m) && e.control) {
            if (e.button == 0) {
                if (grid.InBounds(gx, gy)) { if (!selectedCells.Add(cur)) selectedCells.Remove(cur); }
                e.Use();
            } else if (e.button == 1) {
                isBoxSelecting = true;
                boxStartPos = m;
                selectionRect = new Rect(m.x, m.y, 0, 0);
                e.Use();
            }
        }

        if (e.type == EventType.MouseDrag && isBoxSelecting) {
            selectionRect = Rect.MinMaxRect(Mathf.Min(boxStartPos.x, m.x), Mathf.Min(boxStartPos.y, m.y), Mathf.Max(boxStartPos.x, m.x), Mathf.Max(boxStartPos.y, m.y));
            Repaint();
        }

        if (e.type == EventType.MouseUp) {
            if (isBoxSelecting) {
                for (int x = 0; x < gridWidth; x++) {
                    for (int y = 0; y < gridHeight; y++) {
                        Rect r = new Rect(rect.x + x * TileSize, rect.y + (gridHeight - 1 - y) * TileSize, TileSize, TileSize);
                        if (selectionRect.Overlaps(r)) selectedCells.Add(new Vector2Int(x, y));
                    }
                }
                isBoxSelecting = false;
            }
            lastPaintedCell = new Vector2Int(-1, -1);
        }

        if (e.type == EventType.KeyDown && selectedCells.Count > 0) {
            bool handled = false;
            if (e.keyCode == KeyCode.Q || e.keyCode == KeyCode.E || e.keyCode == KeyCode.R || e.keyCode == KeyCode.W || e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) {
                RecordUndo("Modify Tiles");
                foreach (var p in selectedCells) {
                    var t = grid.GetTile(p.x, p.y, activeLayer);
                    if (t == null) continue;
                    if (e.keyCode == KeyCode.Q) t.rotation -= 90;
                    if (e.keyCode == KeyCode.E || e.keyCode == KeyCode.R) t.rotation += 90;
                    if (e.keyCode == KeyCode.W) t.isMirrored = !t.isMirrored;
                    if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) grid.RemoveTile(p.x, p.y, activeLayer);
                }
                if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) selectedCells.Clear();
                handled = true;
            }
            if (e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9) {
                RecordUndo("Change GroupID");
                int newId = e.keyCode - KeyCode.Alpha0;
                foreach (var p in selectedCells) {
                    var t = grid.GetTile(p.x, p.y, activeLayer);
                    if (t != null) t.groupId = newId;
                    
                    var otherLayer = activeLayer == 0 ? 1 : 0;
                    var ot = grid.GetTile(p.x, p.y, otherLayer);
                    if (ot != null) ot.groupId = newId;
                }
                handled = true;
            }
            if (handled) { e.Use(); Repaint(); }
        }
    }

    void DrawSpriteCorrectly(Rect r, Sprite sp, float rot, bool mirrored) {
        if (sp == null) return;
        Rect spriteRect = sp.rect; 
        Texture2D tex = sp.texture;
        Rect uv = new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height);
        float aspect = spriteRect.width / spriteRect.height;
        Rect drawRect = r;
        if (aspect > 1) { float h = r.width / aspect; drawRect.height = h; drawRect.y += (r.height - h) * 0.5f; }
        else { float w = r.height * aspect; drawRect.width = w; drawRect.x += (r.width - w) * 0.5f; }
        Matrix4x4 b = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(mirrored ? -1 : 1, 1), drawRect.center);
        GUIUtility.RotateAroundPivot(rot, drawRect.center);
        GUI.DrawTextureWithTexCoords(drawRect, tex, uv, true);
        GUI.matrix = b;
    }
}