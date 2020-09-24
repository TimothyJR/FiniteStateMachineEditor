using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FStateMachine
{
	/// <summary>
	/// These are the logic for transfering between states
	/// Each transition can require multiple decisions
	/// </summary>
	[System.Serializable]
	public abstract class TransitionDecision : ScriptableObject
	{
		/// <summary>
		/// Returns true if the condition for transitioning is true
		/// </summary>
		/// <returns></returns>
		public abstract bool Decide();

		/// <summary>
		/// Used to initialize values for the decision
		/// </summary>
		/// <param name="owner"></param>
		public abstract void DecideEnter(GameObject owner);

		/// <summary>
		/// Used to clean up any values for the transition
		/// </summary>
		public abstract void DecideExit();
	}
}