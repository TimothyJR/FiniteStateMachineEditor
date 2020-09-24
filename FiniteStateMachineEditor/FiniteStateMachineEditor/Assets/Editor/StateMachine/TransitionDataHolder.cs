using System.Collections.Generic;
using UnityEngine;
using System;

namespace FStateMachine
{
	/// <summary>
	/// Helper class to hold transition information for the state node
	/// </summary>
	[SerializeField]
	public class TransitionDataHolder : ScriptableObject
	{
		// Holds all transitions
		[SerializeField]
		private List<Transition> transitionsForState;
		public List<Transition> TransitionsForState { get { return transitionsForState; } set { transitionsForState = value; } }

		// Rotation needed to draw graphics between transitions
		private float rotation;
		public float Rotation { get { return rotation; } set { rotation = value; } }

		// The clickable area of the transition
		private Rect clickableArea;
		public Rect ClickableArea
		{ get { return clickableArea; } set { clickableArea = value; } }

		// Whether the transition is selected or not
		private bool selected;
		public bool Selected
		{ get { return selected; } set { selected = value; } }

		// Whether this transition is two way or not
		private bool twoWayTransition;
		public bool TwoWayTransition { set { twoWayTransition = value; } }

		// The transition that is going in the opposite direction of this one
		private TransitionDataHolder otherWayTransition;
		public TransitionDataHolder OtherWayTransition { get { return otherWayTransition; } set { otherWayTransition = value; } }

		// Offset for a two way transition
		private Vector2 offsetVector;

		// Action for removing all transitions
		private Action<TransitionDataHolder> OnRemove;
		public Action<TransitionDataHolder> RemoveAction { get { return OnRemove; } }

		// Action for removing an individual transition
		private Action<Transition, TransitionDataHolder> OnRemoveIndividual;
		public Action<Transition, TransitionDataHolder> RemoveIndividualAction { get { return OnRemoveIndividual; } }

		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name="removal"></param>
		/// <param name="removalIndividual"></param>
		public void Init(Action<TransitionDataHolder> removal, Action<Transition, TransitionDataHolder> removalIndividual)
		{
			OnRemove = removal;
			OnRemoveIndividual = removalIndividual;
		}

		/// <summary>
		/// Sets the clickable area and rotation that will be used for when drawing the triangle
		/// </summary>
		/// <param name="changedStatePosition"></param>
		/// <param name="currentStatePosition"></param>
		/// <param name="clickableSize"></param>
		public void UpdateTrianglePosition(Vector2 changedStatePosition, Vector2 currentStatePosition, Vector2 clickableSize, float triangleOffset)
		{
			rotation = Vector2.SignedAngle(new Vector2(0, -1.0f), changedStatePosition - currentStatePosition);

			if (!twoWayTransition)
			{
				clickableArea = new Rect(((currentStatePosition + changedStatePosition) / 2) - (clickableSize / 2), clickableSize);
				offsetVector = Vector2.zero;
			}
			else
			{
				Vector2 center = (currentStatePosition + changedStatePosition) / 2;
				offsetVector = Vector2.Perpendicular((center - currentStatePosition).normalized) * triangleOffset;
				clickableArea = new Rect((center + offsetVector) - (clickableSize / 2), clickableSize);
			}
		}

		/// <summary>
		/// Draws the transition to the screen
		/// </summary>
		/// <param name="startState"></param>
		/// <param name="endState"></param>
		public void DrawTransition(Vector2 startState, Vector2 endState)
		{
			if(twoWayTransition)
			{
				startState = startState + offsetVector;
				endState = endState + offsetVector;
			}

			Vector2 trianglePosition = clickableArea.center;
			if (selected)
			{
				Color highlightColor = new Color(0.3f, 0.3f, 1.0f, 0.3f);
				EditorDraw.DrawLineInEditorBounds(startState, endState, highlightColor, 8, StateMachineEditor.EditorWidth, StateMachineEditor.EditorHeight);
				EditorDraw.DrawTriangle(trianglePosition, rotation, highlightColor, 6);
			}

			EditorDraw.DrawLineInEditorBounds(startState, endState, Color.white, 5, StateMachineEditor.EditorWidth, StateMachineEditor.EditorHeight);
			EditorDraw.DrawTriangle(trianglePosition, rotation, Color.white, 5);
		}
	}
}