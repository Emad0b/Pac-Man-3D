using UnityEngine;

// Keep the existing prefab script GUID and spawner reference. Eaten ghosts now
// return home through the NavMesh instead of being destroyed and respawned.
public class GhostPrefabScript : GhostScript
{
    public GhostSpawner ghostSpawner;
}
