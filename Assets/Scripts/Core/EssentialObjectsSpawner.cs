using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0)
        {

            // If there is a grid, then spawn at it's center

            var spawnPos = new Vector3(0, 1, 0);                //TROCHU WYZEJ DO Y 1 dodalem

            var grid = FindObjectOfType<Grid>();
            if (grid != null)
            {
                spawnPos += grid.transform.position;            //TROCHU ZMIENILEM
            }

            Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}
