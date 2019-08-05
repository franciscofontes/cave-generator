using UnityEditor;
using UnityEngine;

public class CaveGeneratorWindow : EditorWindow
{
    string prefabName = "Cave 1";
    Vector2 rooms = new Vector2(2, 2);
    Vector3 initialPosition = new Vector3(0, 0, 0);
    Object prefabCell;
    Vector3 area = new Vector3(1, 1, 1);
    Vector2 grid = new Vector2(4, 4);
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

        GUILayout.Label("Cell Settings", EditorStyles.boldLabel);
        prefabCell = EditorGUILayout.ObjectField("Cell Prefab", prefabCell, typeof(Object), true);
        grid = EditorGUILayout.Vector2Field("Cells per room:", grid);

        if (GUILayout.Button(genButton))
        {
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
        for (int i = 0; i < grid.x; i++)
        {
            for (int j = 0; j < grid.y; j++)
            {
                GameObject tile = Instantiate(prefabCell) as GameObject;
                tile.transform.parent = room.transform;
                tile.name = "Cell-" + (i + 1) + "." + (j + 1);
                tile.transform.position = new Vector3(
                    (initialPosition.x + area.x) * i,
                    (initialPosition.y * area.y),
                    (initialPosition.z + area.z) * j
                    );
            }
        }
    }

    void CreatePrefab()
    {
        string localPath = "Assets/Prefabs/" + cavePrefab.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(cavePrefab, localPath, InteractionMode.UserAction);
    }

}