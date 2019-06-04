using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace StateMachine
{
	[System.Serializable]
	public class State : ScriptableObject
	{
		[SerializeField] private string stateName;
		[SerializeField] private Action[] actions;
		[SerializeField] private Action[] fixedActions;
		[SerializeField, HideInInspector] private List<Transition> transitions;
		[SerializeField, HideInInspector] private Rect rectangle;


		public List<Transition> Transitions
		{
			get { return transitions; }
			set { transitions = value; }
		}

		public Rect Rectangle
		{
			get { return rectangle; }
			set { rectangle = value; }
		}

		public string StateName
		{
			get { return stateName; }
			set { stateName = value; }
		}

		public State()
		{
			transitions = new List<Transition>();
		}

		/// <summary>
		/// Called everytime the state machine tick is called
		/// </summary>
		/// <param name="stateMachine"></param>
		public virtual void UpdateState(StateMachine stateMachine)
		{
			DoActions();
			CheckTransitions(stateMachine);
		}

		/// <summary>
		/// Called everytime the state machine fixed tick is called
		/// </summary>
		public void FixedUpdateState(StateMachine stateMachine)
		{
			DoFixedActions();
		}

		/// <summary>
		/// Called when the state is first transitioned to
		/// </summary>
		public void OnStateEnter()
		{
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].ActEnter();
			}
		}

		/// <summary>
		/// Called when leaving this state
		/// </summary>
		public void OnStateExit()
		{
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].ActExit();
			}
		}

		/// <summary>
		/// Actions to be done during update
		/// </summary>
		private void DoActions()
		{
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].Act();
			}
		}

		/// <summary>
		/// Actions to be done during fixed update
		/// </summary>
		private void DoFixedActions()
		{
			for (int i = 0; i < fixedActions.Length; i++)
			{
				fixedActions[i].Act();
			}
		}

		/// <summary>
		/// Check conditions for moving into other states
		/// </summary>
		/// <param name="stateMachine"></param>
		protected void CheckTransitions(StateMachine stateMachine)
		{
			for (int i = 0; i < transitions.Count; i++)
			{
				bool decisionSucceeded = transitions[i].Transitioning;

				if (decisionSucceeded)
				{
					stateMachine.TransitionToState(transitions[i].NextState);
				}
			}
		}

		/// <summary>
		/// Used to update the node position
		/// Since rect is a struct it prevents creating a new rect every frame that a node is moved
		/// </summary>
		/// <param name="delta"></param>
		public void DragRect(Vector2 delta)
		{
			rectangle.position += delta;
		}
	}


}