using System.Collections.Generic;
using UnityEngine;

namespace FStateMachine
{
	[System.Serializable]
	public class State : ScriptableObject
	{
#if UNITY_EDITOR
		// The name of the state
		[SerializeField]
		private string stateName;
		public string StateName { get { return stateName; } set { stateName = value; } }

		// Position of the node in the graph
		[SerializeField, HideInInspector]
		private Rect rectangle;
		public Rect Rectangle { get { return rectangle; } set { rectangle = value; } }
#endif
		// Actions that are performed when in the state
		[SerializeField]
		private Action[] actions = null;
		public Action[] Actions {  get { return actions; } set { actions = value; } }

		// Groups of decisions that will change the state out of this state
		[SerializeField, HideInInspector]
		private List<Transition> transitions = new List<Transition>();
		public List<Transition> Transitions { get { return transitions; } set { transitions = value; } }

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
		public void OnStateEnter(GameObject owner)
		{
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].ActEnter(owner);
			}
			for(int i = 0; i < transitions.Count; i++)
			{
				transitions[i].DecisionEnter(owner);
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
			for(int i = 0; i < transitions.Count; i++)
			{
				transitions[i].DecisionExit();
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
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].FixedAct();
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
				bool decisionSucceeded = transitions[i].Transitioning();

				if (decisionSucceeded)
				{
					stateMachine.TransitionToState(transitions[i].NextState);
				}
			}
		}
#if UNITY_EDITOR
		/// <summary>
		/// Used to update the node position
		/// Since rect is a struct it prevents creating a new rect every frame that a node is moved
		/// </summary>
		/// <param name="delta"></param>
		public void DragRect(Vector2 delta)
		{
			rectangle.position += delta;
		}
#endif
	}
}