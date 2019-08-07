using UnityEditor;
using UnityEngine;

public class CaveGeneratorWindow : EditorWindow
{
    public const string PrefabFolder = "Assets/Prefabs/";

    string prefabName = "Cave 1";
    Vector2 rooms = new Vector2(2, 2);
    float porcentFloor = 0.5f;
    int deathLimit = 2;
    int birthLimit = 2;
    Vector3 initialPosition = new Vector3(0, 0, 0);
    Object prefabFloor;
    Object prefabWall;
    Vector3 area = new Vector3(1, 1, 1);
    int cells = 8;
    int width;
    int height;
    bool[,] grid;
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
        deathLimit = EditorGUILayout.IntField("Death limit:", deathLimit);
        birthLimit = EditorGUILayout.IntField("Birth limit:", birthLimit);

        GUILayout.Label("Cell Settings", EditorStyles.boldLabel);
        prefabFloor = EditorGUILayout.ObjectField("Floor prefab", prefabFloor, typeof(Object), true);
        prefabFloor = AssetDatabase.LoadAssetAtPath(PrefabFolder + "Floor.prefab", typeof(GameObject));
        prefabWall = EditorGUILayout.ObjectField("Wall prefab", prefabWall, typeof(Object), true);
        prefabWall = AssetDatabase.LoadAssetAtPath(PrefabFolder + "Wall.prefab", typeof(GameObject));

        cells = EditorGUILayout.IntField("Cells per room:", cells);

        if (GUILayout.Button(genButton))
        {
            width = cells / 2;
            height = cells / 2;

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
                        (initialPosition.x + width) * i,
                        (initialPosition.y),
                        (initialPosition.z + height) * j
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
        grid = new bool[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = (Random.value <= porcentFloor) ? true : false;
                //CreateVisualCell(room.transform, grid[i, j], i, j);
            }
        }

        // 1 - Any floor cell with less than two neighbors floor becomes a wall.
        // 2 - Any floor cell with more than three floor neighbors becomes a wall.
        // 3 - Any wall cell with exactly three floor neighbors becomes a floor cell.
        // 4 - Any floor cell with two or three floor neighbors remains in the same state for the next generation.

        bool[,] newGrid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int amountFloorCells = CountNearFloorCells(x, y);

                if (grid[x, y])
                {
                    if (amountFloorCells < 2 || amountFloorCells > 3)
                    {
                        newGrid[x, y] = false;
                    }
                    else if (amountFloorCells == 2 || amountFloorCells == 3)
                    {
                        newGrid[x, y] = true;
                    }
                }
                else
                {
                    if (amountFloorCells == 3)
                    {
                        newGrid[x, y] = true;
                    }
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                CreateVisualCell(room.transform, newGrid[i, j], i, j);
            }
        }
    }

    void CreateVisualCell(Transform parent, bool floor, int i, int j)
    {
        GameObject cell = Instantiate(floor ? prefabFloor : prefabWall) as GameObject;
        cell.transform.parent = parent;
        cell.name = "Cell-" + (i + 1) + "." + (j + 1);
        cell.transform.position = new Vector3(
            (initialPosition.x + area.x) * i,
            (initialPosition.y * area.y),
            (initialPosition.z + area.z) * j
            );
    }

    int CountNearFloorCells(int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int nx = x + i;
                int ny = y + j;

                if (nx != i || ny != j)
                {
                    if (nx > -1 && ny > -1 && nx < width && ny < height)
                    {
                        if (grid[nx, ny])
                        {
                            count++;
                        }
                    }
                }
            }
        }
        return count;
    }

    void CreatePrefab()
    {
        string localPath = PrefabFolder + cavePrefab.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(cavePrefab, localPath, InteractionMode.UserAction);
    }

}