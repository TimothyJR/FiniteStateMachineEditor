using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
	/// <summary>
	/// These are the logic for transfering between states.
	/// Each transition can require multiple decisions.
	/// </summary>
	[CreateAssetMenu(menuName = "StateMachine/TransitionDecision"), System.Serializable]
	public abstract class TransitionDecision : ScriptableObject
	{
		public abstract bool Decide();
	}
}