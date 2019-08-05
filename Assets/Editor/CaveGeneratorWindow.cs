using UnityEditor;
using UnityEngine;

public class CaveGeneratorWindow : EditorWindow
{
    public const string PrefabFolder = "Assets/Prefabs/";

    string prefabName = "Cave 1";
    Vector2 rooms = new Vector2(2, 2);
    float porcentFloor = 0.5f;
    Vector3 initialPosition = new Vector3(0, 0, 0);
    Object prefabWall;
    Object prefabFloor;
    Vector3 area = new Vector3(1, 1, 1);
    int cells = 8;
    Vector2 grid;
    GameObject cavePrefab;
    string genButton = "Generate";
    string saveButton = "Save prefab";

    [MenuItem("Window/Cave Generator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CaveGeneratorWindow), false, "Cave Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("General Settings", EditorStyles.boldLabel);
        prefabName = EditorGUILayout.TextField("Name:", prefabName);
        rooms = EditorGUILayout.Vector2Field("Rooms:", rooms);
        porcentFloor = EditorGUILayout.FloatField("Porcent floor:", porcentFloor);

        GUILayout.Label("Cell Settings", EditorStyles.boldLabel);        
        prefabFloor = EditorGUILayout.ObjectField("Floor prefab", prefabFloor, typeof(Object), true);
        prefabFloor = AssetDatabase.LoadAssetAtPath(PrefabFolder + "Floor.prefab", typeof(GameObject));
        prefabWall = EditorGUILayout.ObjectField("Wall prefab", prefabWall, typeof(Object), true);
        prefabWall = AssetDatabase.LoadAssetAtPath(PrefabFolder + "Wall.prefab", typeof(GameObject));        
        
        cells = EditorGUILayout.IntField("Cells per room:", cells);

        if (GUILayout.Button(genButton))
        {

            if (GameObject.Find(prefabName))
            {
                Object.DestroyImmediate(cavePrefab);
            }

            cavePrefab = new GameObject();
            cavePrefab.name = prefabName;

            int r = 0;

            for (int i = 0; i < rooms.x; i++)
            {
                for (int j = 0; j < rooms.y; j++)
                {
                    GameObject room = new GameObject();
                    room.name = "Room-" + (r + 1);
                    room.transform.parent = cavePrefab.transform;

                    CreateCells(room);

                    room.transform.position = new Vector3(
                        (initialPosition.x + grid.x) * i,
                        (initialPosition.y),
                        (initialPosition.z + grid.y) * j
                    );

                    r++;
                }
            }
        }

        if (GUILayout.Button(saveButton))
        {
            CreatePrefab();
        }
    }

    void CreateCells(GameObject room)
    {
        grid = new Vector2(cells / 2, cells / 2);

        for (int i = 0; i < grid.x; i++)
        {
            for (int j = 0; j < grid.y; j++)
            {                
                GameObject cell = Instantiate((Random.value <= porcentFloor) ? prefabFloor : prefabWall) as GameObject;
                cell.transform.parent = room.transform;
                cell.name = "Cell-" + (i + 1) + "." + (j + 1);
                cell.transform.position = new Vector3(
                    (initialPosition.x + area.x) * i,
                    (initialPosition.y * area.y),
                    (initialPosition.z + area.z) * j
                    );
            }
        }
    }

    void CreatePrefab()
    {
        string localPath = PrefabFolder + cavePrefab.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(cavePrefab, localPath, InteractionMode.UserAction);
    }

}