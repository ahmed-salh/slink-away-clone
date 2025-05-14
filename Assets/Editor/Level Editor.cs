using UnityEditor;
using UnityEngine;


public class LevelEditor : EditorWindow
{
    private enum CellType { Ground, Empty, Obstacle, HeadWagon, TrailWagon }

    private CellType _cellType = CellType.Ground;

    private CellType[,] gridCellTypes;

    private Vector2 scrolPosition;

    private LevelStyle levelStyle;

    private GUIStyle gridBackgroundStyle;

    private static EditorWindow window;

    private GameObject gridOrigin;

    private int _width = 5, _height = 5;


    [MenuItem("Slink Away/Level Editor")]
    public static void CreateNewWindow()
    {
        window = GetWindow<LevelEditor>("Level Editor");

        window.minSize = new Vector2(300, 600);
    }

    private void OnEnable()
    {
        levelStyle = Resources.Load<LevelStyle>("Default Style");

        gridCellTypes = new CellType[_width, _height];

    }

    void OnGUI()
    {
        // Initialize if value changed
        if (gridCellTypes == null || gridCellTypes.GetLength(0) != _width || gridCellTypes.GetLength(1) != _height)
            gridCellTypes = new CellType[_width, _height];

        EditorGUILayout.Space(5);

        // Draw references for grid components
        GridObjects();

        EditorGUILayout.Space();

        // Grid size
        LevelParameters();

        EditorGUILayout.Space();

        // Draw the painting tool
        CustomPaintSection();
    }

    void LevelParameters()
    {
        GUILayout.Label("Level parameters", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.Label("Size");

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("X ", GUILayout.Width(10));

        _width = EditorGUILayout.IntField(_width, GUILayout.Width(50), GUILayout.ExpandWidth(true));

        GUILayout.Label("Y ", GUILayout.Width(10));

        _height = EditorGUILayout.IntField(_height, GUILayout.Width(50), GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
    }

    void GridObjects()
    {
        GUILayout.Label("Prefabs", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);

        levelStyle = (LevelStyle)EditorGUILayout.ObjectField("Style:", levelStyle, typeof(LevelStyle), false);
    }

    void CustomPaintSection()
    {
        GUILayout.Label("Paint Level", EditorStyles.boldLabel);

        EditorGUILayout.Space(1);

        _cellType = (CellType)EditorGUILayout.EnumPopup("Paint Type", _cellType);

        EditorGUILayout.Space(1);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate", GUILayout.Height(30)))
            createGridTiles(_cellType, Vector3.zero);
        

        GUILayout.Button("Load", GUILayout.Height(30));

        GUILayout.Button("Clear", GUILayout.Height(30));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(1);

        CreateGridGUI();
    }

    void CreateGridGUI()
    {

        if (gridBackgroundStyle == null)
        {
            gridBackgroundStyle = new GUIStyle(GUI.skin.box);

            gridBackgroundStyle.padding = new RectOffset(8, 8, 8, 8);

            gridBackgroundStyle.margin = new RectOffset(4, 4, 4, 4);
        }

        scrolPosition = EditorGUILayout.BeginScrollView(scrolPosition);

        // Create the background
        EditorGUILayout.BeginVertical(gridBackgroundStyle);

        // Instantiate every row from left to right
        EditorGUILayout.BeginVertical();

        for (int i = 0; i < _height; i++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int j = 0; j < _width; j++)
            {
                if (GUILayout.Button("G", GUILayout.Width(50), GUILayout.Height(50)))
                {
                    // Set the type
                    gridCellTypes[i, j] = _cellType;

                    // TODO: change button style depending on type

                    Debug.Log("Cell index is (" + i + ", " + j + ") Cell type : " + gridCellTypes[i, j]);

                }

            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    void createGridTiles(CellType type, Vector3 startPosition) 
    {
        if (gridOrigin != null)
            DestroyImmediate(gridOrigin);

        gridOrigin = new GameObject("NewLevel");

        float cellSize = 1.0f;

        float wallWidth = 0.1f;

        for (int i = -1; i <= _height; i++) // Note: start from -1 to _height for wall rows
        {
            for (int j = -1; j <= _width; j++) // Note: start from -1 to _width for wall columns
            {
                GameObject prefabToPlace;

                bool isTop = i == -1;
                bool isBottom = i == _height;
                bool isLeft = j == -1;
                bool isRight = j == _width;

                bool isCorner = (isTop || isBottom) && (isLeft || isRight);
                bool isEdge = (isTop || isBottom || isLeft || isRight) && !isCorner;

                // Choose based on cell type or wall creation
                if (isCorner)
                {
                    prefabToPlace = levelStyle.wallCornerPrefab;

                }
                else if (isEdge)
                {
                    prefabToPlace = levelStyle.wallSidePrefab;
                }
                else
                {
                    prefabToPlace = GetPrefabFromType(gridCellTypes[i, j]);
                }

                if (prefabToPlace == null) continue;

                // TODO: create new tiles
                Vector3 position = Vector3.zero;

                // Convert grid indices to positions
                if (prefabToPlace == levelStyle.wallCornerPrefab || prefabToPlace == levelStyle.wallSidePrefab)
                {
                    // Rotate side walls
                    if (prefabToPlace == levelStyle.wallSidePrefab)
                    {
                        if (isTop) position = startPosition + new Vector3(j * cellSize , 0, -i * 0.5f * cellSize + wallWidth); 
                        if (isLeft) position = startPosition + new Vector3(j * 0.5f * cellSize - wallWidth, 0, -i  * cellSize);                         
                        if (isBottom) position = startPosition + new Vector3(j * cellSize , 0, -i  * cellSize + 0.5f * cellSize - wallWidth);
                        if (isRight) position = startPosition + new Vector3(j * cellSize - 0.5f * cellSize + wallWidth, 0, -i  * cellSize ); 
                    }

                    // Rotate corner walls
                    if (prefabToPlace == levelStyle.wallCornerPrefab)
                    {
                        if (isTop && isLeft) position = startPosition + new Vector3(j * 0.5f * cellSize - wallWidth, 0, -i * 0.5f * cellSize + wallWidth);
                        if (isTop && isRight) position = startPosition + new Vector3(j * cellSize - 0.5f * cellSize + wallWidth, 0, -i * 0.5f * cellSize + wallWidth);
                        if (isBottom && isLeft) position = startPosition + new Vector3(j * 0.5f * cellSize - wallWidth, 0, -i * cellSize + 0.5f * cellSize - wallWidth);
                        if (isBottom && isRight) position = startPosition + new Vector3(j * cellSize - 0.5f* cellSize + wallWidth, 0, -i * cellSize + 0.5f * cellSize - wallWidth);
                    }
                }
                else 
                {
                    position = startPosition + new Vector3(j * cellSize, 0, -i * cellSize);
                }

                GameObject newTile = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);

                newTile.transform.position = position;

                newTile.transform.SetParent(gridOrigin.transform);

                // Rotate side walls
                if (prefabToPlace == levelStyle.wallSidePrefab)
                    if (isTop || isBottom) newTile.transform.rotation = Quaternion.Euler(0, 90, 0);


                // Rotate corner walls
                if (prefabToPlace == levelStyle.wallCornerPrefab)
                {
                    if (isTop && isRight) newTile.transform.rotation = Quaternion.Euler(0, 90, 0);
                    if (isBottom && isLeft) newTile.transform.rotation = Quaternion.Euler(0, -90, 0);
                    if (isBottom && isRight) newTile.transform.rotation = Quaternion.Euler(0, 180, 0);
                }

            }
        }
    }

    private GameObject GetPrefabFromType(CellType type)
    {
        switch (type)
        {
            case CellType.Ground:
                return levelStyle.groundPrefab;
            case CellType.Obstacle:
                return levelStyle.obstaclePrefab;
            case CellType.HeadWagon:
                return levelStyle.groundPrefab; // Replace with correct prefab when available
            case CellType.TrailWagon:
                return levelStyle.groundPrefab; // Replace with correct prefab when available
            case CellType.Empty:
            default:
                return null;
        }
    }
}
