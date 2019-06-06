using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace StateMachine
{
	[System.Serializable]
	public class Transition
	{
		[SerializeField] private TransitionDecision[] decision;
		[SerializeField, HideInInspector] private State nextState;


		public Transition(State state)
		{
			nextState = state;
		}
		public State NextState { get { return nextState; } }

		/// <summary>
		/// Returns true if a transition condition is met
		/// </summary>
		public bool Transitioning(GameObject owner)
		{
			bool transition = true;
			for (int i = 0; i < decision.Length; i++)
			{
				if (!decision[i].Decide(owner))
				{
					transition = false;
					break;
				}
			}
			return transition;
		}

	}
}