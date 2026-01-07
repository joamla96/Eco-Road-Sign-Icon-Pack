// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEngine;

/// <summary>Put this on a collider to designate info about it for purposes of World Object placement.</summary>
public class ColliderPlacementOptions : MonoBehaviour
{
    [Help("Put this on an object with a collider in a placed world object to define properties:" + "\n- AccessSpace: Defines a volume of space that must be open to use the object." +
        "\n- UseAsPlacementVolume: Put this on an object with a box collider, and then it will use that collider for placing the object, allowing it to ignore penetrations that other colliders would have with the terrain (like the tip of claim stakes).")]
    public PlacementColliderType ColliderType;

    [Help("If collider used only for placement and not needed - toggle this and it will be destroyed after placement")]
    public bool RemoveColliderAfterPlacement = false;

    void Start()
    {
        if (this.RemoveColliderAfterPlacement && this.GetComponentInParent<WorldObjectPreview>(true) == null) // destroy only on object placement, not for preview
        {
            var col = this.GetComponent<BoxCollider>();
            if (col != null) Destroy(col);
        }
    }
}

public enum PlacementColliderType
{
    AccessSpace, 
    UseAsPlacementVolume,
}
