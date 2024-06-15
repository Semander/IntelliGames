using UnityEngine;

public class Point : MonoBehaviour
{
    [HideInInspector] public int Id;
    [HideInInspector] public Vector3 Position;

    public void Init(Vector3 pos, int id)
    {
        Id = id;
        Position = pos;
        transform.position = Position;
    }
}