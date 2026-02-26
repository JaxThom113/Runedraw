using UnityEngine;
using Cinemachine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera OverworldViewCamera;
    
    // Hardcode this to match your Camera Distance (e.g., 30) 
    // to keep the mouse on a stable plane.
    [SerializeField] float depthFromCamera = 10f; 

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;  
        
        // 1. MUST BE POSITIVE. This is distance FROM the lens to the world.
        mousePosition.z = depthFromCamera; 

        // 2. Convert pixels to World Units
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        
        // 3. Lock Z to 0 so it stays on the same plane as your player/grid
        worldPosition.z = 0f;

        transform.position = worldPosition;
    }
}