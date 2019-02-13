using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace StateMachine
{
   public class StateNode
   {
      private State state;
      private GUIStyle stateStyle;

      private bool isSelected;
      private bool isDragged;

      private List<State> connectedStates;

      public Action<StateNode> OnRemoveNode;
      public Action<StateNode> OnCreateConnection;
      public Action<StateNode> Clicked;
      public Action<StateNode> Changed;
      public Action<TransitionDataHolder> OnTransitionClicked;

      // Transition related
      Vector2[] trianglePoints;
      Dictionary<State, TransitionDataHolder> stateTransitionInfo;

      public State NodeState
      {
         get { return state; }
      }

      public StateNode(Vector2 position, float width, float height, Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked)
      {
         state = ScriptableObject.CreateInstance<State>();
         state.StateName = "New State";

         state.Rectangle = new Rect(position.x, position.y, width, height);

         Init(OnClickRemoveState, OnClickCreateTransition, OnClicked, OnChanged, OnTransitionClicked);
      }

      public StateNode(State state, Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked)
      {
         this.state = state;

         Init(OnClickRemoveState, OnClickCreateTransition, OnClicked, OnChanged, OnTransitionClicked);
      }


      /// <summary>
      /// Used for shared code between the different constructors
      /// </summary>
      /// <param name="OnClickRemoveState"></param>
      /// <param name="OnClickCreateTransition"></param>
      /// <param name="OnClicked"></param>
      /// <param name="OnChanged"></param>
      public void Init(Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked)
      {
         stateStyle = StateMachineEditor.DefaultStyle;
         connectedStates = new List<State>();

         OnRemoveNode = OnClickRemoveState;
         OnCreateConnection = OnClickCreateTransition;
         Clicked = OnClicked;
         Changed = OnChanged;

         trianglePoints = new Vector2[3];
         trianglePoints[0] = new Vector3(0, 1.3f);
         trianglePoints[1] = new Vector2(1.15f, -0.7f);
         trianglePoints[2] = new Vector2(-1.15f, -0.7f);

         stateTransitionInfo = new Dictionary<State, TransitionDataHolder>();

         for (int i = 0; i < state.Transitions.Count; i++)
         {
            if(!connectedStates.Contains(state.Transitions[i].NextState))
            {
               connectedStates.Add(state.Transitions[i].NextState);
               TransitionDataHolder t = new TransitionDataHolder();
               t.CurrentTrianglePoints = new Vector2[3];
               t.TransitionsForState = new List<Transition>();
               t.TransitionsForState.Add(state.Transitions[i]);
               stateTransitionInfo.Add(state.Transitions[i].NextState, t);
               UpdateTriangle(state.Transitions[i].NextState);
            }
            else
            {
               stateTransitionInfo[state.Transitions[i].NextState].TransitionsForState.Add(state.Transitions[i]);
            }
         }
      }

      /// <summary>
      /// Updates the triangle on the transition graphics
      /// </summary>
      public void UpdateTriangle(State changedState)
      {
         if (connectedStates.Contains(changedState))
         {
            float rotation = Vector2.SignedAngle(-trianglePoints[0], state.Rectangle.center - changedState.Rectangle.center);
            CalculateTriangle((state.Rectangle.center + changedState.Rectangle.center) * 0.5f, 10.0f, rotation, changedState);
         }
         else if (state == changedState)
         {
            for(int i = 0; i < connectedStates.Count; i++)
            {
               float rotation = Vector2.SignedAngle(-trianglePoints[0], state.Rectangle.center - connectedStates[i].Rectangle.center);
               CalculateTriangle((state.Rectangle.center + connectedStates[i].Rectangle.center) * 0.5f, 10.0f, rotation, connectedStates[i]);
            }
         }
      }

      /// <summary>
      /// Draws a triangle pointing towards the outPoint node
      /// </summary>
      /// <param name="position"></param>
      /// <param name="size"></param>
      /// <param name="rotationAngle"></param>
      private void CalculateTriangle(Vector2 position, float size, float rotationAngle, State state)
      {
         float sin = Mathf.Sin(rotationAngle * Mathf.Deg2Rad);
         float cos = Mathf.Cos(rotationAngle * Mathf.Deg2Rad);

         stateTransitionInfo[state].CurrentTrianglePoints[0] = (new Vector2(trianglePoints[0].x * cos - trianglePoints[0].y * sin, trianglePoints[0].x * sin + trianglePoints[0].y * cos) * size) + position;
         stateTransitionInfo[state].CurrentTrianglePoints[1] = (new Vector2(trianglePoints[1].x * cos - trianglePoints[1].y * sin, trianglePoints[1].x * sin + trianglePoints[1].y * cos) * size) + position;
         stateTransitionInfo[state].CurrentTrianglePoints[2] = (new Vector2(trianglePoints[2].x * cos - trianglePoints[2].y * sin, trianglePoints[2].x * sin + trianglePoints[2].y * cos) * size) + position;
      }

      /// <summary>
      /// Draws the node
      /// </summary>
      public void Draw()
      {
         GUI.Box(state.Rectangle, state.StateName, stateStyle);
      }

      /// <summary>
      /// Draws a line from the inpoint to the outpoint
      /// </summary>
      public void DrawTransitions()
      {
         for (int i = 0; i < connectedStates.Count; i++)
         {
            Handles.DrawLine(state.Rectangle.center, connectedStates[i].Rectangle.center);

            Handles.DrawLine(stateTransitionInfo[connectedStates[i]].CurrentTrianglePoints[0], stateTransitionInfo[connectedStates[i]].CurrentTrianglePoints[1]);
            Handles.DrawLine(stateTransitionInfo[connectedStates[i]].CurrentTrianglePoints[1], stateTransitionInfo[connectedStates[i]].CurrentTrianglePoints[2]);
            Handles.DrawLine(stateTransitionInfo[connectedStates[i]].CurrentTrianglePoints[2], stateTransitionInfo[connectedStates[i]].CurrentTrianglePoints[0]);

            if (Handles.Button((state.Rectangle.center + connectedStates[i].Rectangle.center) * 0.5f, Quaternion.identity, 4, 8, Handles.CircleHandleCap))
            {
               if (OnTransitionClicked != null)
               {
                  OnTransitionClicked(stateTransitionInfo[connectedStates[i]]);
               }
            }
         }
      }

      /// <summary>
      /// Handles mouse input
      /// </summary>
      /// <param name="e"></param>
      /// <returns></returns>
      public bool ProcessEvent(Event e)
      {
         switch (e.type)
         {
            case EventType.MouseDown:
               if (e.button == 0)
               {
                  if (state.Rectangle.Contains(e.mousePosition))
                  {
                     isDragged = true;
                     GUI.changed = true;
                     isSelected = true;
                     stateStyle = StateMachineEditor.SelectedStyle;
                     if (Clicked != null)
                     {
                        Clicked(this);
                     }
                  }
                  else
                  {
                     GUI.changed = true;
                     isSelected = false;
                     stateStyle = StateMachineEditor.DefaultStyle;
                  }
               }
               if (e.button == 1)
               {
                  if (state.Rectangle.Contains(e.mousePosition))
                  {
                     GUI.changed = true;
                     isSelected = true;
                     stateStyle = StateMachineEditor.SelectedStyle;
                     ProcessContextMenu();
                     e.Use();
                  }
                  else
                  {
                     GUI.changed = true;
                     isSelected = false;
                     stateStyle = StateMachineEditor.DefaultStyle;
                  }

               }
               break;

            case EventType.MouseUp:
               isDragged = false;
               break;
            case EventType.MouseDrag:
               if (e.button == 0 && isDragged)
               {
                  DragState(e.delta);
                  e.Use();
                  OnNodeChange();
                  return true;
               }
               break;
         }

         return false;
      }

      /// <summary>
      /// Moves the node if it is being dragged
      /// </summary>
      /// <param name="delta"></param>
      public void DragState(Vector2 delta)
      {
         state.DragRect(delta);
      }

      /// <summary>
      /// Creates context menu when right clicking on nodes
      /// </summary>
      private void ProcessContextMenu()
      {
         GenericMenu genericMenu = new GenericMenu();
         genericMenu.AddItem(new GUIContent("Create connection"), false, OnClickCreateConnection);
         genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
         genericMenu.ShowAsContext();
      }

      /// <summary>
      /// Calls actions for node removal
      /// </summary>
      private void OnClickRemoveNode()
      {
         if (OnRemoveNode != null)
         {
            OnRemoveNode(this);
         }
      }

      /// <summary>
      /// Calls actions for creating connections
      /// </summary>
      private void OnClickCreateConnection()
      {
         if (OnCreateConnection != null)
         {
            OnCreateConnection(this);
         }
      }

      /// <summary>
      /// Calls actions when the node is changed
      /// </summary>
      private void OnNodeChange()
      {
         if (Changed != null)
         {
            Changed(this);
         }
      }

      /// <summary>
      /// Creates a new transition
      /// </summary>
      public void CreateTransition(State endState)
      {
         Transition transition = new Transition(endState);
         state.Transitions.Add(transition);

         if (!connectedStates.Contains(endState))
         {
            connectedStates.Add(endState);
            TransitionDataHolder t = new TransitionDataHolder();
            t.CurrentTrianglePoints = new Vector2[3];
            t.TransitionsForState = new List<Transition>();
            t.TransitionsForState.Add(transition);
            stateTransitionInfo.Add(endState, t);
         }
         else
         {
            stateTransitionInfo[endState].TransitionsForState.Add(transition);
         }

         UpdateTriangle(endState);
      }

      ///// <summary>
      ///// Removed a connection from this node
      ///// </summary>
      ///// <param name="transitionToRemove"></param>
      //public void RemoveTransition(EditorTransition transitionToRemove)
      //{
      //   for(int i = 0; i < transitionToRemove.Transitions.Length; i++)
      //   {
      //      state.Transitions.Remove(transitionToRemove.Transitions[i]);
      //      Transitions.Remove(transitionToRemove);
      //   }
      //}
   }

   [System.Serializable]
   public class State : ScriptableObject
   {
      [SerializeField] private Action[] actions;
      [SerializeField] private Action[] fixedActions;
      [SerializeField] private List<Transition> transitions;
      [SerializeField] private Rect rectangle;
      [SerializeField] private string stateName;

      public List<Transition> Transitions
      {
         get { return transitions; }
         set { transitions = value; }
      }

      public Rect Rectangle
      {
         get { return rectangle; }
         set { rectangle = value; }
      }

      public string StateName
      {
         get { return stateName; }
         set { stateName = value; }
      }

      public State()
      {
         transitions = new List<Transition>();
      }

      /// <summary>
      /// Called everytime the state machine tick is called
      /// </summary>
      /// <param name="stateMachine"></param>
      public void UpdateState(StateMachine stateMachine)
      {
         DoActions();
         CheckTransitions(stateMachine);
      }

      /// <summary>
      /// Called everytime the state machine fixed tick is called
      /// </summary>
      public void FixedUpdateState(StateMachine stateMachine)
      {
         DoFixedActions();
      }

      /// <summary>
      /// Called when the state is first transitioned to
      /// </summary>
      public void OnStateEnter()
      {
         for (int i = 0; i < actions.Length; i++)
         {
            actions[i].ActEnter();
         }
      }

      /// <summary>
      /// Called when leaving this state
      /// </summary>
      public void OnStateExit()
      {
         for (int i = 0; i < actions.Length; i++)
         {
            actions[i].ActExit();
         }
      }

      /// <summary>
      /// Actions to be done during update
      /// </summary>
      public void DoActions()
      {
         for (int i = 0; i < actions.Length; i++)
         {
            actions[i].Act();
         }
      }

      /// <summary>
      /// Actions to be done during fixed update
      /// </summary>
      public void DoFixedActions()
      {
         for (int i = 0; i < fixedActions.Length; i++)
         {
            fixedActions[i].Act();
         }
      }

      /// <summary>
      /// Check conditions for moving into other states
      /// </summary>
      /// <param name="stateMachine"></param>
      public void CheckTransitions(StateMachine stateMachine)
      {
         for (int i = 0; i < transitions.Count; i++)
         {
            bool decisionSucceeded = transitions[i].Transitioning;

            if (decisionSucceeded)
            {
               stateMachine.TransitionToState(transitions[i].NextState);
            }
         }
      }

      /// <summary>
      /// Used to update the node position
      /// Since rect is a struct it prevents creating a new rect every frame that a node is moved
      /// </summary>
      /// <param name="delta"></param>
      public void DragRect(Vector2 delta)
      {
         rectangle.position += delta;
      }
   }

   public class TransitionDataHolder
   {
      private Vector2[] currentTrianglePoints;
      private List<Transition> transitionsForState;

      public Vector2[] CurrentTrianglePoints
      {
         get { return currentTrianglePoints; }
         set { currentTrianglePoints = value; }
      }

      public List<Transition> TransitionsForState
      {
         get { return transitionsForState; }
         set { transitionsForState = value; }
      }
   }
}