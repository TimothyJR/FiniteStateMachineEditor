using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FStateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/StateMachine"), System.Serializable]
	public class StateMachine : ScriptableObject
	{
		// The State the FSM is currently in
		// If nothing has run, this is the starting state
		[SerializeField]
		private State currentState;
		public State CurrentState { get { return currentState; } set { currentState = value; } }

		// This state does not have any actions
		// It is used to transition to states that can be transitioned to at any time
		[SerializeField, HideInInspector]
		private State anyState;
		public State AnyState { get { return anyState; } set { anyState = value; } }

		// The gameobject that holdss this state machine
		private GameObject machineHolder;

#if UNITY_EDITOR
		[SerializeField, HideInInspector]
		private Vector2 graphOffset = new Vector2(0,0);
		public Vector2 GraphOffset { get { return graphOffset; } set { graphOffset = value; } }
#endif


		/// <summary>
		/// Initialize the state machine
		/// </summary>
		public void Init(GameObject owner)
		{
			machineHolder = owner;
			currentState.OnStateEnter(machineHolder);
		}
 
		/// <summary>
		/// Update the state machine
		/// </summary>
		public void Tick()
		{
			currentState.UpdateState(this);
		}
 
		/// <summary>
		/// Fixed update for the state machine
		/// </summary>
		public void FixedTick()
		{
			currentState.FixedUpdateState(this);
		}

		/// <summary>
		/// Changes state machine to a different state
		/// </summary>
		/// <param name="nextState">State to transition to</param>
		public void TransitionToState(State nextState)
		{
			if(nextState != null)
			{
				currentState.OnStateExit();
				currentState = nextState;
				currentState.OnStateEnter(machineHolder);
			}
		}
	}
}