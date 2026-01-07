// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace Eco.Client.Utils.MiscUtils
{
    public static class AnimQueryEditor
    {
        public static string[] GetAllAnimatorStates(Animator animator)
        {
            #if UNITY_EDITOR
            var runtimeController = animator.runtimeAnimatorController;

            // Handle AnimatorOverrideController
            if (runtimeController is AnimatorOverrideController overrideController)
                runtimeController = overrideController.runtimeAnimatorController;

            var controller = runtimeController as AnimatorController;
            if (controller == null) return new string[0];

            List<string> stateNames = new List<string>();
            foreach (var layer in controller.layers)
                ExtractStatesFromStateMachine(layer.stateMachine, stateNames);

            return stateNames.ToArray();
            #else
            return new string[0];
            #endif
        }

        #if UNITY_EDITOR
        static void ExtractStatesFromStateMachine(AnimatorStateMachine stateMachine, List<string> stateNames)
        {
            foreach (var state in stateMachine.states) stateNames.Add(state.state.name);
            foreach (var subStateMachine in stateMachine.stateMachines)
                ExtractStatesFromStateMachine(subStateMachine.stateMachine, stateNames);
        }
        #endif
    }
}
