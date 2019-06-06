using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/StateMachine"), System.Serializable]
	public class StateMachine : ScriptableObject
	{
	
		[SerializeField] private State currentState;
		[SerializeField, HideInInspector] private State anyState; 
		[SerializeField, HideInInspector] private Vector2 graphOffset = new Vector2(0,0);
		private GameObject machineHolder;

		public State CurrentState
		{
			get { return currentState; }
			set { currentState = value; }
		}

		public State AnyState
		{
			get { return anyState; }
			set { anyState = value; }
		}

		public Vector2 GraphOffset
		{
			get { return graphOffset; }
			set { graphOffset = value; }
		}

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
			currentState.UpdateState(this, machineHolder);
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