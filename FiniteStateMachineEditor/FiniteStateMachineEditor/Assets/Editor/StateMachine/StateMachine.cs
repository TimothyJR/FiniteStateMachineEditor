using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

namespace StateMachine
{
   [CreateAssetMenu(menuName = "StateMachine/StateMachine"), System.Serializable]
   public class StateMachine : ScriptableObject
   {
   
      [SerializeField] private State currentState;
      [SerializeField, HideInInspector] private State anyState; 
      [SerializeField, HideInInspector] private Vector2 graphOffset = new Vector2(0,0);

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
      public void Init()
      {
         currentState.OnStateEnter();
      }
 
      /// <summary>
      /// Update the state machine
      /// </summary>
      public void Tick()
      {
         currentState.UpdateState(this);
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
            currentState.OnStateEnter();
         }
      }

      /// <summary>
      /// Opens the scriptable object with the editor
      /// </summary>
      /// <param name="instanceID"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      [OnOpenAsset]
      public static bool PullUpNodeEditor(int instanceID, int line)
      {
         StateMachine stateMachine = EditorUtility.InstanceIDToObject(instanceID) as StateMachine;
         if (stateMachine != null)
         {
            StateMachineEditor.OpenWindow(stateMachine);
            return true;
         }

         return false;
      }
   }
}