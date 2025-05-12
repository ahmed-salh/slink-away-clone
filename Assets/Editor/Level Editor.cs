using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;


public class LevelEditor : EditorWindow
{
    private enum CellType { Ground, Empty, Obstacle, HeadWagon, TrailWagon }

    private GameObject groundPrefab, obstaclePrefab, headWagonPrefab, trailWagonPrefab, wallPrefab;

    private int width = 4, height = 4;

    private static EditorWindow window;

    private Vector2 scrolPosition;

    [MenuItem("Slink Away/Level Editor")]
    public static void ShowWindow()
    {
        window = GetWindow<LevelEditor>("Level Editor");

        window.minSize = new Vector2(300, 600);

        window.name = "Level Editor";
    }

    void OnGUI()
    {
        EditorGUILayout.Space(5);

        GUILayout.Label("Prefabs", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);

        groundPrefab = (GameObject)EditorGUILayout.ObjectField("Ground:", groundPrefab, typeof(GameObject), false);

        obstaclePrefab = (GameObject)EditorGUILayout.ObjectField("Obstacle:", obstaclePrefab, typeof(GameObject), false);

        wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall:", wallPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        GUILayout.Label("Level parameters", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.Label("Size");

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("X ", GUILayout.Width(10)); 

        width = EditorGUILayout.IntField(width, GUILayout.Width(50), GUILayout.ExpandWidth(true));

        GUILayout.Label("Y ", GUILayout.Width(10));

        height = EditorGUILayout.IntField(height, GUILayout.Width(50), GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        LevelDesigner();

    }


    void LevelDesigner()
    {
        GUILayout.Label("Level designer", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.EnumPopup("Paint Type", CellType.Ground);

        EditorGUILayout.Space();

        GUILayout.Label("Level designer");

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Button("Save as", GUILayout.Height(30));

        GUILayout.Button("Load", GUILayout.Height(30));

        GUILayout.Button("Clear", GUILayout.Height(30));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUIStyle gridBackgroundStyle = new GUIStyle(GUI.skin.box);

        gridBackgroundStyle.padding = new RectOffset(12, 12, 12, 12);

        gridBackgroundStyle.margin = new RectOffset(6, 6, 6, 6);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);

        buttonStyle.padding = new RectOffset(5, 5, 5, 5);

        buttonStyle.margin = new RectOffset(0, 0, 0, 0);

        scrolPosition = EditorGUILayout.BeginScrollView(scrolPosition);

        EditorGUILayout.BeginVertical(gridBackgroundStyle);

        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < width; i++)
        {
            EditorGUILayout.BeginVertical();

            for (int j = 0; j < height; j++)
            {
                GUILayout.Button("G", buttonStyle);

            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }
}
