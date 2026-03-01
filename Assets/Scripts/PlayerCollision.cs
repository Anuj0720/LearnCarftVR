using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public Transform head;
    public Transform floorReference;

    [Tooltip("Set this to the tag you assigned to your puzzle/demo cubes (e.g. 'Cube')")]
    public string cubeTag = "Cube";

    CapsuleCollider myCollider;

    void Start()
    {
        myCollider = GetComponent<CapsuleCollider>();

        // Find every collider on objects tagged as a cube and ignore collisions with them
        IgnoreCubeCollisions();
    }

    void Update()
    {
        float height = head.position.y - floorReference.position.y;
        myCollider.height = height;
        transform.position = head.position - Vector3.up * height / 2;
    }

    void IgnoreCubeCollisions()
    {
        // Find all GameObjects with the cube tag
        GameObject[] cubes = GameObject.FindGameObjectsWithTag(cubeTag);

        foreach (GameObject cube in cubes)
        {
            // Get all colliders on the cube and its children
            Collider[] cubeColliders = cube.GetComponentsInChildren<Collider>(true);

            foreach (Collider cubeCollider in cubeColliders)
            {
                Physics.IgnoreCollision(myCollider, cubeCollider, true);
            }
        }

        Debug.Log($"PlayerCollision: Ignoring collisions with {cubes.Length} cube(s) tagged '{cubeTag}'.");
    }

    /// <summary>
    /// Call this at runtime if new cubes are spawned after Start
    /// so their collisions are also ignored.
    /// </summary>
    public void RefreshCubeCollisions()
    {
        IgnoreCubeCollisions();
    }
}