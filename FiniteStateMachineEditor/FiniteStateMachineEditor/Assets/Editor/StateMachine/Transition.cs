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

      /// <summary>
      /// Returns true if a transition condition is met
      /// </summary>
      public bool Transitioning
      {
         get
         {
            bool transition = true;
            for (int i = 0; i < decision.Length; i++)
            {
               if (!decision[i].Decide())
               {
                  transition = false;
                  break;
               }
            }
            return transition;
         }
      }
      public State NextState { get { return nextState; } }
   }
}