﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FStateMachine
{
	/// <summary>
	/// These are actions done by states
	/// Each state can have multiple actions
	/// </summary>
	[System.Serializable]
	public abstract class Action : ScriptableObject
	{
		public abstract void Act();
		public abstract void FixedAct();
		public abstract void ActEnter(GameObject owner);
		public abstract void ActExit();
	}
}