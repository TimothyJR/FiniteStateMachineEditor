using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


namespace StateMachine
{
   /// <summary>
   /// Helper class to hold transition information for the state node
   /// </summary>
   [SerializeField]
   public class TransitionDataHolder : ScriptableObject
   {
      [SerializeField] private List<Transition> transitionsForState;
      private float rotation;
      private Rect clickableArea;
      private bool selected;
      private Action<TransitionDataHolder> OnRemove;
      private Action<Transition, TransitionDataHolder> OnRemoveIndividual;

      public float Rotation
      {
         get { return rotation; }
         set { rotation = value; }
      }

      public List<Transition> TransitionsForState
      {
         get { return transitionsForState; }
         set { transitionsForState = value; }
      }

      public Rect ClickableArea
      {
         get { return clickableArea; }
         set { clickableArea = value; }
      }

      public bool Selected
      {
         get { return selected; }
         set { selected = value; }
      }

      public Action<TransitionDataHolder> RemoveAction
      { get { return OnRemove; } }

      public Action<Transition, TransitionDataHolder> RemoveIndividualAction
      { get { return OnRemoveIndividual; } }

      public void Init(Action<TransitionDataHolder> removal, Action<Transition, TransitionDataHolder> removalIndividual)
      {
         OnRemove = removal;
         OnRemoveIndividual = removalIndividual;
      }
   }
}