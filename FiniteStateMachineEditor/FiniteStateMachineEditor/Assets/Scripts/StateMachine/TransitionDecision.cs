using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
	/// <summary>
	/// These are the logic for transfering between states.
	/// Each transition can require multiple decisions.
	/// </summary>
	[System.Serializable]
	public abstract class TransitionDecision : ScriptableObject
	{
		public abstract bool Decide();
		public abstract void DecideEnter(GameObject owner);
	}
}