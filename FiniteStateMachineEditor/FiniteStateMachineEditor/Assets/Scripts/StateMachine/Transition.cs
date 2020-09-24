using UnityEngine;

namespace FStateMachine
{
	[System.Serializable]
	public class Transition : ScriptableObject
	{
		/// <summary>
		/// List of all the different decisions that need to happen to transition out of the state
		/// </summary>
		#pragma warning disable 0649
		[SerializeField] private TransitionDecision[] decisions;
		#pragma warning restore 0649

		/// <summary>
		/// The state we will transition to
		/// </summary>
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

		/// <summary>
		/// Used to initialize any variables for the decisions
		/// </summary>
		/// <param name="owner"></param>
		public void DecisionEnter(GameObject owner)
		{
			for (int i = 0; i < decisions.Length; i++)
			{
				decisions[i].DecideEnter(owner);
			}
		}

		/// <summary>
		/// Used to clean up any variables for the decisions
		/// </summary>
		public void DecisionExit()
		{
			for(int i = 0; i < decisions.Length; i++)
			{
				decisions[i].DecideExit();
			}
		}

	}
}