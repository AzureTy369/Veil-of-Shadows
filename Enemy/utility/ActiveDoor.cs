using UnityEngine;
using UnityEngine.Tilemaps; 

public class ActiveDoor : MonoBehaviour
{
    TilemapRenderer tilemapRenderer;
    TilemapCollider2D tilemapCollider;
    void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    public void CloseDoor()
    {
        tilemapRenderer.enabled = true;
        tilemapCollider.enabled = true;
    }

    public void OpenDoor()
    {
        tilemapRenderer.enabled = false;
        tilemapCollider.enabled = false;
    }
}