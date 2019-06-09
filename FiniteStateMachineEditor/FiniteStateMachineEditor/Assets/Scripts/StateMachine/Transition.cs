using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace StateMachine
{
	[System.Serializable]
	public class Transition : ScriptableObject
	{
		[SerializeField] private TransitionDecision[] decisions;
		[SerializeField, HideInInspector] private State nextState;

		public State NextState { get { return nextState; } }

		public void Init(State state)
		{
			nextState = state;
		}

		/// <summary>
		/// Returns true if a transition condition is met
		/// </summary>
		public bool Transitioning()
		{
			bool transition = true;
			for (int i = 0; i < decisions.Length; i++)
			{
				if (!decisions[i].Decide())
				{
					transition = false;
					break;
				}
			}
			return transition;
		}

		public void DecisionEnter(GameObject owner)
		{
			for (int i = 0; i < decisions.Length; i++)
			{
				decisions[i].DecideEnter(owner);
			}
		}

		public void DecisionExit()
		{
			for(int i = 0; i < decisions.Length; i++)
			{
				decisions[i].DecideExit();
			}
		}

	}
}