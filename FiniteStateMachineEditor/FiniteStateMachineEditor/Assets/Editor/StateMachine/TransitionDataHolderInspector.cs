using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace StateMachine
{
	/// <summary>
	/// Used to change the inspector display of transitions
	/// </summary>
	[CustomEditor(typeof(TransitionDataHolder))]
	public class TransitionDataHolderInSpector : Editor
	{
		public override void OnInspectorGUI()
		{
			TransitionDataHolder transitionData = (TransitionDataHolder)target;

			GUILayout.Label("Transition to: " + transitionData.TransitionsForState[0].NextState.StateName);

			for(int i = 0; i < transitionData.TransitionsForState.Count; i++)
			{
				// Get the decisions array
				SerializedObject obj = new UnityEditor.SerializedObject(transitionData.TransitionsForState[i]);
				SerializedProperty decisions = obj.FindProperty("decisions");

				// Display array and edit it
				EditorGUILayout.PropertyField(decisions, true);
				serializedObject.ApplyModifiedProperties();

				// Create button to remove transitions
				if(GUILayout.Button("Remove this transition"))
				{
					// If there is more than one transition, remove only the single transition
					// If there is only one transition, remove the transition and destroy the state connection
					if(transitionData.TransitionsForState.Count > 1)
					{
						if(transitionData.RemoveIndividualAction != null)
						{
							transitionData.RemoveIndividualAction(transitionData.TransitionsForState[i], transitionData);
						}
					}
					else
					{
						if(transitionData.RemoveAction != null)
						{
							transitionData.RemoveAction(transitionData);
						}
					}
				}
			}
		}
	}
}