using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class LevelEditor : EditorWindow
{
    private CellData[,] cellData;

    private enum CellType { Ground, Obstacle, WagonExit}
    private enum StairDirection { None,Top, Left, Down, Right}
    private enum WagonDirection { Up, Left, Down, Right}
    private enum WagonType { None,Head, Mid, Tail}


    private WagonType _hasWagon = WagonType.None;

    private StairDirection _hasPassage = StairDirection.None;

    private WagonDirection _wagonDirection = WagonDirection.Up;

    private CellType _cellType = CellType.Ground;

    private CellType[,] gridCellTypes;

    private Vector2Int selectedCell = new Vector2Int(-1, -1);

    private Vector2 scrolPosition;

    private LevelStyle levelStyle;

    private GUIStyle gridBackgroundStyle;

    private static EditorWindow window;

    private Dictionary<Vector2Int, Queue<Passenger>> passengerQueues = new Dictionary<Vector2Int, Queue<Passenger>>();

    private GameObject gridOrigin;

    private int _width = 5, _height = 5;

    private bool isPaintMode = true;

    private string newName = "";

    private int newCount = 1;

    private Material newColor;

    [MenuItem("Slink Away/Level Editor")]
    public static void CreateNewWindow()
    {
        window = GetWindow<LevelEditor>("Level Editor");

        window.minSize = new Vector2(300, 600);
    }

    private void OnEnable()
    {
        levelStyle = Resources.Load<LevelStyle>("Default Style");

        ResizeGrid(_height, _width);

        passengerQueues.Clear();

        for (int r = 0; r < _height; r++)
            for (int c = 0; c < _width; c++)
                passengerQueues[new Vector2Int(r, c)] = new Queue<Passenger>();
    }

    void OnGUI()
    {
        // Insert space from above
        EditorGUILayout.Space(5);

        // Draw references for grid components
        GridObjects();

        EditorGUILayout.Space();

        // Draw the painting tool
        CustomPaintSection();
    }

    void GridObjects()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);

        levelStyle = (LevelStyle)EditorGUILayout.ObjectField("Style:", levelStyle, typeof(LevelStyle), false);
        
        GUILayout.Label("Size:");

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("X ", GUILayout.Width(10));

        _width = EditorGUILayout.IntField(_width, GUILayout.Width(50), GUILayout.ExpandWidth(true));

        GUILayout.Label("Y ", GUILayout.Width(10));

        _height = EditorGUILayout.IntField(_height, GUILayout.Width(50), GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
    }

    void CustomPaintSection()
    {
        GUILayout.Label("Paint Level", EditorStyles.boldLabel);

        EditorGUILayout.Space(1);

        _cellType = (CellType)EditorGUILayout.EnumPopup("Paint Type", _cellType);

        EditorGUILayout.Space(1);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Update", GUILayout.Height(30)))
        {
            ResizeGrid(_height, _width);

            createGridTiles(_cellType, Vector3.zero);
        }

        isPaintMode = GUILayout.Toggle(
           isPaintMode,
           new GUIContent(isPaintMode ? "Paint" : "Select"),
           "Button",
           GUILayout.Height(30)
       );


        EditorGUILayout.EndHorizontal();

        if (!isPaintMode) 
        {
            // Create the background
            EditorGUILayout.BeginVertical(gridBackgroundStyle);

            if (selectedCell == new Vector2Int(-1, -1))
                EditorGUILayout.LabelField("Select a cell to change its type", EditorStyles.miniLabel);
            else 
            {
                EditorGUILayout.LabelField("Cell: " + selectedCell);
                EditorGUILayout.LabelField("Base Type: " + gridCellTypes[selectedCell.x, selectedCell.y].ToString());

                if (gridCellTypes[selectedCell.x, selectedCell.y] != CellType.Obstacle && gridCellTypes[selectedCell.x, selectedCell.y] != CellType.WagonExit)
                    _hasWagon = (WagonType)EditorGUILayout.EnumPopup("Has wagon: ", _hasWagon);

                if(_hasWagon != WagonType.None)
                    _wagonDirection = (WagonDirection)EditorGUILayout.EnumPopup("Has wagon: ", _wagonDirection);


                switch (_hasWagon)
                {
                    case WagonType.None:
                        break;
                    case WagonType.Head:
                        break;
                    case WagonType.Mid:
                        break;
                    case WagonType.Tail:
                        break;
                    default:
                        break;
                }

                bool isTop = selectedCell.x == 0;
                
                bool isBottom = selectedCell.x == _height - 1 ;
                
                bool isLeft = selectedCell.y == 0;
                
                bool isRight = selectedCell.y == _width - 1;

                if (isTop || isBottom || isLeft || isRight) 
                {

                    _hasPassage = (StairDirection)EditorGUILayout.EnumPopup("Has passage: ", _hasPassage);

                    if (_hasPassage != StairDirection.None)
                    {

                        EditorGUILayout.BeginVertical(gridBackgroundStyle);

                        var queue = passengerQueues[selectedCell];

                        // group by Name+Color
                        var groups = queue
                            .GroupBy(p => new { p.Name, p.Color })
                            .Select(g => new { Key = g.Key, Count = g.Count() })
                            .ToList();

                        EditorGUILayout.LabelField($"Passengers: {queue.Count}", EditorStyles.boldLabel);

                        foreach (var group in groups)
                        {
                            EditorGUILayout.BeginVertical();

                            // show class name and count
                            EditorGUILayout.LabelField(
                                $"{group.Key.Name} (x{group.Count})"

                            );

                            // show its color
                            EditorGUILayout.ColorField(group.Key.Color.color);

                            // remove entire class when clicked
                            if (GUILayout.Button("Remove Class"))
                            {
                                // rebuild the queue without any passenger of this class
                                var filtered = queue
                                    .Where(p => !(p.Name == group.Key.Name
                                               && p.Color == group.Key.Color));
                                passengerQueues[selectedCell] = new Queue<Passenger>(filtered);
                                break; // exit the loop since the queue has changed
                            }

                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndVertical();

                        // controls to enqueue
                        EditorGUILayout.BeginHorizontal();
                        
                        EditorGUILayout.LabelField("Count: ", GUILayout.Width(40));
                        
                        newCount = EditorGUILayout.IntField(newCount);
                        
                        EditorGUILayout.LabelField("Name: ", GUILayout.Width(40));
                        
                        newName = EditorGUILayout.TextField(newName);
                        
                        EditorGUILayout.LabelField("Material: ", GUILayout.Width(40));

                        newColor = (Material)EditorGUILayout.ObjectField(newColor, typeof(Material), false);

                        if (GUILayout.Button("Enqueue", GUILayout.Height(20)))
                        {
                            int existingPassengers = queue.Count;

                            GameObject newPassage = (GameObject)PrefabUtility.InstantiatePrefab(levelStyle.passage);

                            if (isLeft) 
                            {
                                newPassage.transform.position = new Vector3(selectedCell.y - 0.8f, 0.2f, -selectedCell.x);
                                newPassage.transform.rotation = Quaternion.Euler(0, 90, 0);

                            }

                            if (isRight)
                            {
                                newPassage.transform.position = new Vector3(selectedCell.y + 0.8f, 0.2f, -selectedCell.x);
                                newPassage.transform.rotation = Quaternion.Euler(0, -90, 0);

                            }           
                            if (isTop)
                            {
                                newPassage.transform.position = new Vector3(selectedCell.y , 0.2f, -selectedCell.x + 0.8f);
                                newPassage.transform.rotation = Quaternion.Euler(0, 180, 0);

                            }     
                            if (isBottom)
                            {
                                newPassage.transform.position = new Vector3(selectedCell.y , 0.2f, -selectedCell.x - 0.8f);
                                newPassage.transform.rotation = Quaternion.Euler(0, 0, 0);

                            }
                            for (int i = 0; i < newCount; i++) 
                            {
                                queue.Enqueue(new Passenger(newName, newColor));

                                GameObject newPassenger = (GameObject)PrefabUtility.InstantiatePrefab(levelStyle.passenger);

                                Vector3 position = new Vector3(0, 0, 0);    

                                if (isLeft)
                                    position = new Vector3(selectedCell.y - 0.5f * existingPassengers - 0.5f* i - 1.5f , 0, -selectedCell.x);
                                if (isRight)
                                    position = new Vector3(selectedCell.y + 0.5f*existingPassengers + 0.5f* i + 1.5f, 0, -selectedCell.x);
                                if (isTop)
                                    position = new Vector3(selectedCell.y  , 0, -selectedCell.x + 0.5f * existingPassengers + 0.5f * i + 1.5f);
                                if (isBottom)
                                    position = new Vector3(selectedCell.y , 0, -selectedCell.x - 0.5f * existingPassengers - 0.5f * i - 1.5f);


                                newPassenger.transform.position = position;

                                newPassenger.GetComponentsInChildren<SkinnedMeshRenderer>()[0].material = newColor;

                                newPassenger.transform.SetParent(gridOrigin.transform);
                            }


                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUILayout.EndVertical();

        }

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

        EditorGUILayout.LabelField("Level grid", EditorStyles.miniLabel);

        scrolPosition = EditorGUILayout.BeginScrollView(scrolPosition);

        // Create the background
        EditorGUILayout.BeginVertical(gridBackgroundStyle);

        // Instantiate every row from left to right
        EditorGUILayout.BeginVertical();

        for (int i = 0; i < gridCellTypes.GetLength(0); i++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int j = 0; j < gridCellTypes.GetLength(1); j++)
            {
                string buttonName = gridCellTypes[i, j].ToString();


                if (isPaintMode)
                {
                    if (GUILayout.Button(buttonName, GUILayout.Width(70), GUILayout.Height(70)))
                    {
                        // Set the type
                        gridCellTypes[i, j] = _cellType;
                    }
                }
                else
                {
                    bool selected = selectedCell == new Vector2(i, j);

                    if (GUILayout.Toggle(selected, new GUIContent(isPaintMode ? buttonName : buttonName), "Button", GUILayout.Width(70), GUILayout.Height(70)))
                    {
                        selectedCell = new Vector2Int(i, j);
                    }
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

        for (int i = -1; i <= _height; i++) 
        {
            for (int j = -1; j <= _width; j++) 
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
                    prefabToPlace = levelStyle.wallCorner;

                }
                else if (isEdge)
                {
                    prefabToPlace = levelStyle.wallSide;
                }
                else
                {
                    prefabToPlace = GetPrefabFromType(gridCellTypes[i, j]);
                }

                // Spawn position
                Vector3 position = Vector3.zero;

                // Convert grid indices to positions
                if (prefabToPlace == levelStyle.wallCorner || prefabToPlace == levelStyle.wallSide)
                {
                    // Rotate side walls
                    if (prefabToPlace == levelStyle.wallSide)
                    {
                        if (isTop) position = startPosition + new Vector3(j * cellSize , 0, -i * 0.5f * cellSize + wallWidth); 
                        if (isLeft) position = startPosition + new Vector3(j * 0.5f * cellSize - wallWidth, 0, -i  * cellSize);                         
                        if (isBottom) position = startPosition + new Vector3(j * cellSize , 0, -i  * cellSize + 0.5f * cellSize - wallWidth);
                        if (isRight) position = startPosition + new Vector3(j * cellSize - 0.5f * cellSize + wallWidth, 0, -i  * cellSize ); 
                    }

                    // Rotate corner walls
                    if (prefabToPlace == levelStyle.wallCorner)
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
                if (prefabToPlace == levelStyle.wallSide)
                    if (isTop || isBottom) newTile.transform.rotation = Quaternion.Euler(0, 90, 0);


                // Rotate corner walls
                if (prefabToPlace == levelStyle.wallCorner)
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
                return levelStyle.ground;
            case CellType.Obstacle:
                return levelStyle.obstacle;
            case CellType.WagonExit:
                return levelStyle.wagonExit;
            default:
                return null;
        }
    }

    private void ResizeGrid(int newHeight, int newWidth)
    {
        // If its not initialized create new grid
        if (gridCellTypes == null)
        {
            gridCellTypes = new CellType[newHeight, newWidth];
            return;
        }

        int oldRows = gridCellTypes.GetLength(0);
        int oldCols = gridCellTypes.GetLength(1);

        // If not changed don't update size
        if (oldRows == newHeight && oldCols == newWidth)
            return;

        var newGrid = new CellType[newHeight, newWidth];

        // Only copy rows that exist in old grid but don't exceed new grid size
        int copyRows = Math.Min(oldRows, newHeight);
        int copyCols = Math.Min(oldCols, newWidth);

        for (int r = 0; r < copyRows; r++)
            for (int c = 0; c < copyCols; c++)
                newGrid[r, c] = gridCellTypes[r, c];

        gridCellTypes = newGrid;

        passengerQueues.Clear();
        for (int r = 0; r < _height; r++)
            for (int c = 0; c < _width; c++)
                passengerQueues[new Vector2Int(r, c)] = new Queue<Passenger>();
    }


}

