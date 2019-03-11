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
      private float rotation;
      [SerializeField] private List<Transition> transitionsForState;
      private Rect clickableArea;
      bool selected;

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
   }
}