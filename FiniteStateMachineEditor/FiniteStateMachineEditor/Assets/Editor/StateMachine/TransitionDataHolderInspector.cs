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
		SerializedProperty transitionList;

		private void OnEnable()
		{
			transitionList = serializedObject.FindProperty("transitionsForState");
		}

		public override void OnInspectorGUI()
		{
			TransitionDataHolder transitionData = (TransitionDataHolder)target;

			GUILayout.Label("Transition to: " + transitionData.TransitionsForState[0].NextState.name);

			for (int i = 0; i < transitionData.TransitionsForState.Count; i++)
			{
				SerializedProperty listEntry = transitionList.GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(listEntry, true);

				serializedObject.ApplyModifiedProperties();

				if(GUILayout.Button("Remove this transition"))
				{
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