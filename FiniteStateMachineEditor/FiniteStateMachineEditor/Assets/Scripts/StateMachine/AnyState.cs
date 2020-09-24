using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FStateMachine
{
	public class AnyState : State
	{
		/// <summary>
		/// Checks if we have any transitions to make
		/// </summary>
		/// <param name="stateMachine"></param>
		public override void UpdateState(StateMachine stateMachine)
		{
			CheckTransitions(stateMachine);
		}
	}
}