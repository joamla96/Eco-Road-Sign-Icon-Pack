// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using UnityEngine;

namespace Eco.Client.Utils.MiscUtils
{
    public static class AnimQuery
    {
        public static Vector3 GetNetSpeed(AnimationClip clip, string species)
        {
            if (clip == null)
                return Vector3.zero;

            // Create a temporary GameObject for sampling
            GameObject tempObject = new GameObject("TempRoot");
            Transform tempTransform = tempObject.transform;

            // Create a placeholder child object
            GameObject childObject = new GameObject($"{species}_Root_Bone");
            childObject.transform.parent = tempTransform;
            Transform childTransform = childObject.transform;

            // Sample at the first frame to record the starting position
            clip.SampleAnimation(tempObject, 0f);
            Vector3 startPosition = childTransform.position;

            // Sample at the last frame to record the ending position
            clip.SampleAnimation(tempObject, clip.length);
            Vector3 endPosition = childTransform.position;

            // If no motion is found, check all children to find the first moving object
            if (startPosition == endPosition)
            {
                foreach (Transform child in tempTransform)
                {
                    clip.SampleAnimation(tempObject, 0f);
                    startPosition = child.position;

                    clip.SampleAnimation(tempObject, clip.length);
                    endPosition = child.position;

                    if (startPosition != endPosition)
                    {
                        childTransform = child; // Found a moving child
                        break;
                    }
                }
            }

            // Cleanup
            GameObject.DestroyImmediate(tempObject);

            // Calculate net translation and speed
            Vector3 netTranslation = endPosition - startPosition;
            return netTranslation / clip.length; // Speed = Distance / Time
        }
    }
}
