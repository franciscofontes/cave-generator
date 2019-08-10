using UnityEngine;

[CreateAssetMenu(menuName="Cave Generator/Cave Element")]
public class CaveElement : ScriptableObject
{
    public new string name;
    public GameObject prefab;
    public int limit;
}
