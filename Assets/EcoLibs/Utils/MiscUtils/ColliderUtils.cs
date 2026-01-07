// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using Eco.Shared.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Random = UnityEngine.Random;
using System.Linq;

public static class ColliderUtils
{
    public static Vector3 ColliderSizeDecreaseForSmoothBlockChecks = Vector3.one * 0.05f; // Unity doesn't like OverlapBoxNonAlloc on same coords, so need to use small shift inside

    static Collider[] colliderTest = new Collider[20];

    static Collider BlocksCollider(Vector3 center, Vector3 halfExtents, Quaternion rot, int layerMask, Func<Collider, bool> filter)
    {
        var actualHalfExtents = halfExtents - ColliderSizeDecreaseForSmoothBlockChecks; // small decrease for checks to not be so strict
        
        var num = Physics.OverlapBoxNonAlloc(center, actualHalfExtents, colliderTest, rot, layerMask);
        for (int i = 0; i < num; i++)
            if (!colliderTest[i].isTrigger)
                if (filter == null || filter.Invoke(colliderTest[i]))
                    return colliderTest[i];
        return null;
    }

    public static bool TouchesCollider(this WorldRange rangeInc, int layerMask, Func<Collider, bool> filter, out Collider overlapCollider)
    {
        overlapCollider = BlocksCollider(rangeInc.CenterInc - new Vector3(.5f, .5f, .5f), rangeInc.SizeInc * .5f, Quaternion.identity, layerMask, filter);
        return overlapCollider != null;
    }

    public static Vector3 RandomPointInside(this BoxCollider box)
    {
        var boxSize = box.size;
        return box.transform.TransformPoint(box.center + new Vector3((Random.value - 0.5f) * boxSize.x, (Random.value - 0.5f) * boxSize.y, (Random.value - 0.5f) * boxSize.z));
    }

    /// <summary>
    /// Calculates compound collider bounds which contains bounds of all active colliders within <paramref name="gameObject"/>.
    /// If no colliders then it will return default bounds (zero size at zero point).
    /// </summary>
    public static Bounds GetCompoundColliderBounds(this GameObject gameObject)
    {
        var list = TempLists.Rent<Collider>();
        try
        {
            gameObject.GetComponentsInChildren(list);
            return list.GetCompoundColliderBounds();
        }
        finally
        {
            TempLists.Return(list);
        }
    }
    
    /// <summary>
    /// Calculates compound collider bounds which contains bounds of all colliders in <see cref="list"/>.
    /// If no colliders then it will return default bounds (zero size at zero point).
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public static Bounds GetCompoundColliderBounds(this List<Collider> list)
    {
        using (var enumerator = list.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                return default;

            var bounds = enumerator.Current.bounds;
            while (enumerator.MoveNext())
                bounds.Encapsulate(enumerator.Current.bounds);
            return bounds;
        }
    }

    /// <summary>
    /// Get start and end of a capsule collider without having to pass the instance. Useful when needing to check for multiple poses
    /// but don't want to actually move the collider.
    /// If <paramref name="fullHeight"/> is true, we will use the full height of the capsule; otherwise, we will subtract the radius from the spheres at the limits of the capsule 
    /// </summary>
    /// <remarks>Only works for capsules with default value of direction.</remarks>
    public static (Vector3 start, Vector3 end) GetCapsuleStartEnd(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 center, float radius, float height, bool fullHeight = false)
    {
        // if height doesn't greater than diameter, the capsule is actually just a sphere.
        if (height <= radius * 2)
        {
            var startEnd = center.TransformPoint(position, rotation, scale);
            return (startEnd, startEnd);
        }
		
        ///If the capsule is long enough to be a normal one:
        ///If <param name="fullHeight"/> is false, subtract height by radius twice to get the height of the cylinder inside the capsule.
        ///Otherwise, just get the height of the capsule.
        var halfHeight = fullHeight ? (height / 2f) : ((height - radius * 2) / 2);
        // start and end are the top and bottom centers of the circle on top and bottom of cylinder
        var start = center.WithY(center.y - halfHeight).TransformPoint(position, rotation, scale);
        var end   = center.WithY(center.y + halfHeight).TransformPoint(position, rotation, scale);
        return (start, end);
    }

    /// <summary>
    /// Get positions of capsule's start and end (center of the 2 spheres on capsule's 2 ends) in world space.
    /// Useful when using with <see cref="Physics.CheckCapsule(Vector3,Vector3,float)"/> or <see cref="Physics.CapsuleCast(Vector3,Vector3,float,Vector3)"/>.
    /// </summary>
    public static (Vector3 start, Vector3 end) GetCapsuleStartEnd(this CapsuleCollider capsule)
    {
        var transform      = capsule.transform;
        var direction      = capsule.GetDirectionVector();           //Find vector representing direction of the capsule in local space
        var startEndOffset = (capsule.height / 2f) + capsule.radius; //Calculate offset of start and end coordinates from center of the capsule

        //Calculate start and end (centers of the spheres) coordinates in local space
        var start = capsule.center - direction * startEndOffset;
        var end   = capsule.center + direction * startEndOffset;

        //Transform to world space
        start = transform.TransformPoint(start);
        end   = transform.TransformPoint(end);

        return (start, end);
    }

    /// <summary>Returns a vector representing direction of the capsule in local space.</summary>
    public static Vector3 GetDirectionVector(this CapsuleCollider capsule) => capsule.direction switch
    {
        0 => Vector3.right,
        1 => Vector3.up,
        2 => Vector3.forward,
        _ => throw new Exception($"Capsule collider has invalid value of direction: {capsule.direction}. Only valid values are 0, 1 and 2."),
    };

    /// <summary>Returns what collider a given collider touches, with an option to ignore colliders under a given gameobject.
    /// Ingores chunks.</summary>
    public static IEnumerable<(Transform TouchingObj, Vector3 Direction, float Distance)> GetTouchingColliders(this Collider collider, GameObject rootObject, Func<GameObject, bool> ignore, int layerMask)
    {
        Collider[] allColliders = Physics.OverlapBox(collider.bounds.center, 
                                                     Vector3.Scale(collider.bounds.size, collider.transform.lossyScale) / 2f, 
                                                     collider.transform.rotation, layerMask);

        foreach (Collider otherCollider in allColliders)
        {
            if (otherCollider.gameObject.HasAncestor(rootObject))  continue;
            if (ignore?.Invoke(otherCollider.gameObject) == true)  continue;

            if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation,
                                            otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                                            out var direction, out var distance))
                yield return (otherCollider.transform, direction, distance);
        }
    }

    public static void IgnoreAllSelfCollisions(this GameObject obj)
    {
        // Get all colliders on this GameObject
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < colliders.Length; i++) 
            for (int j = i + 1; j < colliders.Length; j++) 
                Physics.IgnoreCollision(colliders[i], colliders[j]);
    }

    public enum MeshGenerationOptions { ConvexMeshColliders, NonConvexMeshColliders, BoxCollider }

    /// <summary>Creates mesh colliders for each renderer that has a mesh and marks them as convex for collision detection.</summary>
    public static void CreateColliderForObject(GameObject newObject, MeshGenerationOptions meshGenerationOptions)
    {
        if (meshGenerationOptions == MeshGenerationOptions.ConvexMeshColliders || meshGenerationOptions == MeshGenerationOptions.NonConvexMeshColliders)
        {
            var renderers = newObject.GetComponentsInChildren<Renderer>().Where(x => x is MeshRenderer || x is SkinnedMeshRenderer); // only count those types
            foreach (var renderer in renderers)
            {
                Mesh mesh = null;
                if (renderer is MeshRenderer meshRenderer && meshRenderer.GetComponent<MeshFilter>() is { } meshFilter)
                    mesh = meshFilter.sharedMesh;
                else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    mesh = skinnedMeshRenderer.sharedMesh;
                
                if (mesh != null)
                {
                    var meshCollider = renderer.gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                    meshCollider.convex = meshGenerationOptions == MeshGenerationOptions.ConvexMeshColliders; // Mark as convex for collision detection
                }
            }
        }
        else if (meshGenerationOptions == MeshGenerationOptions.BoxCollider)
        {
            var boxCollider = newObject.AddComponent<BoxCollider>();
            var bounds = newObject.GetEncompassingRendererLocalBounds(false);
            if (bounds.HasValue)
            {
                boxCollider.size = bounds.Value.size;
                boxCollider.center = bounds.Value.center;
            }
        }
    }
}
