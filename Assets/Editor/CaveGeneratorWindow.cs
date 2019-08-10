using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CaveGeneratorWindow : EditorWindow
{
    public const string PrefabFolder = "Assets/CaveGenerator/Prefabs/";
    public const string RoomPrefix = "Room-";
    public const string CellPrefix = "Cell-";

    CaveSimulation caveSimulation;
    bool[,] grid;

    string prefabName = "Cave 1";
    Vector2 mapMatrix = new Vector2(2, 2);
    int size = 50;
    float percentageStartFloor = 0.5f;
    int floorLimit = 5;//3
    int wallLimit = 12;//6
    int steps = 2;
    int border = 1;

    GameObject map;
    GameObject wall;
    Dictionary<int, List<Vector2>> islands;
    Object prefabFloor;
    Object prefabWall;

    [SerializeField] List<CaveElement> elements = new List<CaveElement>();

    Vector3 initialPosition = new Vector3(0, 0, 0);
    Vector3 area = new Vector3(1, 1, 1);

    bool enableButtons = false;

    string genButton = "Generate";
    string polishButton = "Polish";
    string removeIslandsButton = "Remove islands";
    string addElementsButton = "Add elements";
    string removeElementsButton = "Remove elements";
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
        mapMatrix = EditorGUILayout.Vector2Field("Map matrix:", mapMatrix);
        size = EditorGUILayout.IntField("Size:", size);
        percentageStartFloor = EditorGUILayout.FloatField("Start porcent floor:", percentageStartFloor);
        floorLimit = EditorGUILayout.IntField("Floor limit:", floorLimit);
        wallLimit = EditorGUILayout.IntField("Wall limit:", wallLimit);
        steps = EditorGUILayout.IntField("Steps:", steps);
        border = EditorGUILayout.IntField("Border:", border);
        prefabFloor = EditorGUILayout.ObjectField("Floor prefab:", prefabFloor, typeof(Object), true);
        prefabFloor = AssetDatabase.LoadAssetAtPath(PrefabFolder + "Core/Floor.prefab", typeof(GameObject));
        prefabWall = EditorGUILayout.ObjectField("Wall prefab:", prefabWall, typeof(Object), true);
        prefabWall = AssetDatabase.LoadAssetAtPath(PrefabFolder + "Core/Wall.prefab", typeof(GameObject));

        GUILayout.Label("Element Settings", EditorStyles.boldLabel);
        int count = Mathf.Max(0, EditorGUILayout.IntField("Size:", elements.Count));
        while (count < elements.Count)
        {
            elements.RemoveAt(elements.Count - 1);
        }
        while (count > elements.Count)
        {
            elements.Add(null);
        }
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i] = EditorGUILayout.ObjectField("Element:", elements[i], typeof(Object), true) as CaveElement;
        }

        if (GUILayout.Button(genButton))
        {
            GenerateMap();
        }

        EditorGUI.BeginDisabledGroup(enableButtons == false);
        if (GUILayout.Button(polishButton))
        {
            Polish();
        }
        if (GUILayout.Button(removeIslandsButton))
        {
            RemoveIslands();
        }
        if (GUILayout.Button(addElementsButton))
        {
            AddElements(grid);
        }
        if (GUILayout.Button(removeElementsButton))
        {
            RemoveElements();
        }
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button(saveButton))
        {
            SavePrefab(map);
        }
        EditorGUI.EndDisabledGroup();
    }

    void GenerateMap()
    {
        caveSimulation = new CaveSimulation(GetWidth(), GetHeight(), percentageStartFloor, floorLimit, wallLimit, border);
        grid = caveSimulation.StartSimulation();
        islands = caveSimulation.DetectIslands(grid);  

        RecreateMap();

        enableButtons = true;
    }

    void Polish()
    {
        grid = caveSimulation.Polish(grid, steps);
        islands = caveSimulation.DetectIslands(grid);

        RecreateMap();
    }

    void CreateRoomsByIslands(Dictionary<int, List<Vector2>> islands)
    {
        foreach (KeyValuePair<int, List<Vector2>> i in islands)
        {
            GameObject room = new GameObject();
            room.name = RoomPrefix + i.Key;
            room.transform.parent = map.transform;

            foreach (Vector2 c in i.Value)
            {
                GameObject cell = GetCell((int)c.x, (int)c.y);
                cell.transform.parent = room.transform;
            }
        }
    }

    void RemoveIslands()
    {
        islands = caveSimulation.DetectIslands(grid);
        grid = CreatePathBetweenRooms(islands);
        
        RecreateMap();
    }

    bool[,] CreatePathBetweenRooms(Dictionary<int, List<Vector2>> islands)
    {
        foreach (KeyValuePair<int, List<Vector2>> i in islands)
        {
            int index = Random.Range(0, i.Value.Count - 1);
            Vector2 p = i.Value[index];

            bool horizontal = Random.Range(0, 2) == 0 ? true : false;
            int direction = Random.Range(0, 2) == 0 ? -1 : 1;            
            grid = (horizontal) ? caveSimulation.DigHorizontal(grid, (int)p.x, (int)p.y, direction) : caveSimulation.DigVertical(grid, (int)p.x, (int)p.y, direction);
        }
        return grid;
    }

    void RecreateMap()
    {
        if (GameObject.Find(prefabName))
        {
            Object.DestroyImmediate(map);
        }
        map = new GameObject();
        map.name = prefabName;

        wall = CreateWall(map);
        CreateCells(wall);
        CreateRoomsByIslands(islands);
    }

    GameObject CreateWall(GameObject map)
    {
        wall = new GameObject();
        wall.name = "Walls";
        wall.transform.parent = map.transform;
        return wall;
    }

    void CreateCells(GameObject wall)
    {
        for (int i = 0; i < GetWidth(); i++)
        {
            for (int j = 0; j < GetHeight(); j++)
            {
                GameObject parent = grid[i, j] ? map : wall;
                CreateCell(parent, grid[i, j], i, j);
            }
        }
    }

    void CreateCell(GameObject parent, bool floor, int i, int j)
    {
        GameObject prefab = ((floor) ? prefabFloor : prefabWall) as GameObject;
        string name = CellPrefix + (i + 1) + "." + (j + 1);
        GameObject cell = CreateGameObjectByPrefab(parent, prefab, name);
        cell.transform.position = new Vector3(
            (initialPosition.x + area.x) * i,
            (initialPosition.y * area.y),
            (initialPosition.z + area.z) * j
            );
    }

    GameObject GetCell(int i, int j)
    {
        return GameObject.Find(CellPrefix + (i + 1) + "." + (j + 1));
    }

    void AddElements(bool[,] grid)
    {
        List<Vector2> floors = new List<Vector2>();

        for (int i = 0; i < GetWidth(); i++)
        {
            for (int j = 0; j < GetHeight(); j++)
            {
                if (grid[i, j])
                {
                    floors.Add(new Vector2(i, j));
                }
            }
        }

        foreach (CaveElement ce in elements)
        {
            for (int i = 0; i < ce.limit; i++)
            {
                floors = ListUtil.Shuffle(floors);
                Vector2 f = floors[i];
                CreateElement(GetCell((int)f.x, (int)f.y), ce, (ce.prefab.name + "-" + i));
            }
        }
    }

    void RemoveElements()
    {
        foreach (CaveElement ce in elements)
        {
            for (int i = 0; i < ce.limit; i++)
            {
                DestroyImmediate(GameObject.Find(ce.prefab.name + "-" + i));
            }
        }
    }

    void CreateElement(GameObject parent, CaveElement ce, string elementName)
    {
        CreateGameObjectByPrefab(parent, ce.prefab, elementName);
    }

    GameObject CreateGameObjectByPrefab(GameObject parent, GameObject prefab, string objName)
    {
        GameObject gameObject = Instantiate(prefab, parent.transform.position, prefab.transform.rotation) as GameObject;
        gameObject.transform.parent = parent.transform;
        gameObject.name = objName;
        return gameObject;
    }

    int GetWidth()
    {
        return (int)mapMatrix.x * size / 2;
    }

    int GetHeight()
    {
        return (int)mapMatrix.y * size / 2;
    }

    void SavePrefab(GameObject cavePrefab)
    {
        string localPath = PrefabFolder + "Caves/" + cavePrefab.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(cavePrefab, localPath, InteractionMode.UserAction);
    }
}