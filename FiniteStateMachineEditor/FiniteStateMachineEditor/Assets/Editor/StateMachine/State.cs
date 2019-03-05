using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace StateMachine
{
   public class StateNode
   {
      static int idCount = 0;
      private State state;
      private GUIStyle stateStyle;

      private bool isSelected;
      private bool isDragged;

      private List<State> connectedStates;

      private Action<StateNode> OnRemoveNode;
      private Action<StateNode> OnCreateConnection;
      private Action<StateNode> Clicked;
      private Action<StateNode> Changed;
      private Action<StateNode> StartState;
      private Action<TransitionDataHolder> OnTransitionClicked;

      // Transition related
      Dictionary<State, TransitionDataHolder> stateTransitionInfo;

      public State NodeState
      {
         get { return state; }
      }

      public Dictionary<State, TransitionDataHolder> StateTransitionInfo
      {
         set { stateTransitionInfo = value; }
      }

      /// <summary>
      /// Used when creating a new state in the editor
      /// </summary>
      /// <param name="position"></param>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="OnClickRemoveState"></param>
      /// <param name="OnClickCreateTransition"></param>
      /// <param name="OnClicked"></param>
      /// <param name="OnChanged"></param>
      /// <param name="OnTransitionClicked"></param>
      /// <param name="OnClickStartState"></param>
      public StateNode(Vector2 position, float width, float height, Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked, Action<StateNode> OnClickStartState)
      {
         state = ScriptableObject.CreateInstance<State>();
         state.StateName = "" + idCount;
         idCount += 1;

         state.Rectangle = new Rect(position.x, position.y, width, height);

         Init(OnClickRemoveState, OnClickCreateTransition, OnClicked, OnChanged, OnTransitionClicked, OnClickStartState);
      }

      /// <summary>
      /// Used when loading a state from an already created asset
      /// </summary>
      /// <param name="state"></param>
      /// <param name="OnClickRemoveState"></param>
      /// <param name="OnClickCreateTransition"></param>
      /// <param name="OnClicked"></param>
      /// <param name="OnChanged"></param>
      /// <param name="OnTransitionClicked"></param>
      /// <param name="OnClickStartState"></param>
      public StateNode(State state, State selectedState, Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked, Action<StateNode> OnClickStartState)
      {
         this.state = state;

         Init(OnClickRemoveState, OnClickCreateTransition, OnClicked, OnChanged, OnTransitionClicked, OnClickStartState, selectedState);
      }

      /// <summary>
      /// Used for shared code between the different constructors
      /// </summary>
      /// <param name="OnClickRemoveState"></param>
      /// <param name="OnClickCreateTransition"></param>
      /// <param name="OnClicked"></param>
      /// <param name="OnChanged"></param>
      public void Init(Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked, Action<StateNode> OnClickStartState, State selectedState = null)
      {
         stateStyle = StateMachineEditor.DefaultStyle;
         if (selectedState != null)
         {
            if(selectedState == state)
            {
               stateStyle = StateMachineEditor.StartStyle;
            }
         }
         
         connectedStates = new List<State>();

         OnRemoveNode = OnClickRemoveState;
         OnCreateConnection = OnClickCreateTransition;
         Clicked = OnClicked;
         Changed = OnChanged;
         StartState = OnClickStartState;

         stateTransitionInfo = new Dictionary<State, TransitionDataHolder>();

         for (int i = 0; i < state.Transitions.Count; i++)
         {
            if(!connectedStates.Contains(state.Transitions[i].NextState))
            {
               connectedStates.Add(state.Transitions[i].NextState);
               TransitionDataHolder t = new TransitionDataHolder();
               t.TransitionsForState = new List<Transition>();
               t.TransitionsForState.Add(state.Transitions[i]);
               stateTransitionInfo.Add(state.Transitions[i].NextState, t);
               UpdateTriangleRotation(state.Transitions[i].NextState);
            }
            else
            {
               stateTransitionInfo[state.Transitions[i].NextState].TransitionsForState.Add(state.Transitions[i]);
            }
         }
      }

      /// <summary>
      /// Updates the rotation that is used by the triangle sprite for transitions
      /// </summary>
      /// <param name="changedState"></param>
      public void UpdateTriangleRotation(State changedState)
      {
         if (connectedStates.Contains(changedState))
         {
            float rotation = Vector2.SignedAngle(new Vector2(0, -1.0f), changedState.Rectangle.center - state.Rectangle.center);
            stateTransitionInfo[changedState].Rotation = rotation;
         }
         else if (state == changedState)
         {
            for(int i = 0; i < connectedStates.Count; i++)
            {
               float rotation = Vector2.SignedAngle(new Vector2(0, -1.0f), connectedStates[i].Rectangle.center - changedState.Rectangle.center);
               stateTransitionInfo[connectedStates[i]].Rotation = rotation;
            }
         }

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
            EditorDraw.DrawLine(state.Rectangle.center, connectedStates[i].Rectangle.center, Color.white, 5);

            EditorDraw.DrawTriangle((state.Rectangle.center + connectedStates[i].Rectangle.center) / 2, stateTransitionInfo[connectedStates[i]].Rotation, Color.white, 5);

            //if (Handles.Button((state.Rectangle.center + connectedStates[i].Rectangle.center) * 0.5f, Quaternion.identity, 4, 8, Handles.CircleHandleCap))
            //{
            //   Event e = Event.current;
            //   if(e.type == EventType.MouseUp && e.button == 0)
            //   {
            //      Debug.Log("Left Up");
            //      if (OnTransitionClicked != null)
            //      {
            //         OnTransitionClicked(stateTransitionInfo[connectedStates[i]]);
            //      }
            //   }
            //   else if(e.type == EventType.MouseDown && e.button == 0)
            //   {
            //      Debug.Log("Left Down");
            //   }
            //   else if (e.type == EventType.MouseUp && e.button == 1)
            //   {
            //      Debug.Log("Right Up");
            //   }
            //   else if (e.type == EventType.MouseDown && e.button == 1)
            //   {
            //      Debug.Log("Right Down");
            //   }
            //}
         }
      }

      /// <summary>
      /// Handles mouse input
      /// </summary>
      /// <param name="e"></param>
      /// <returns></returns>
      public bool ProcessEvent(Event e, bool isStartingState)
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
                     if(isStartingState)
                     {
                        stateStyle = StateMachineEditor.StartSelectedStyle;
                     }
                     else
                     {
                        stateStyle = StateMachineEditor.SelectedStyle;
                     }
                     
                     if (Clicked != null)
                     {
                        Clicked(this);
                     }
                  }
                  else
                  {
                     GUI.changed = true;
                     isSelected = false;
                     if (isStartingState)
                     {
                        stateStyle = StateMachineEditor.StartStyle;
                     }
                     else
                     {
                        stateStyle = StateMachineEditor.DefaultStyle;
                     }
                  }
               }
               if (e.button == 1)
               {
                  if (state.Rectangle.Contains(e.mousePosition))
                  {
                     GUI.changed = true;
                     isSelected = true;
                     if (isStartingState)
                     {
                        stateStyle = StateMachineEditor.StartSelectedStyle;
                     }
                     else
                     {
                        stateStyle = StateMachineEditor.SelectedStyle;
                     }

                     ProcessContextMenu();
                     e.Use();
                  }
                  else
                  {
                     GUI.changed = true;
                     isSelected = false;
                     if (isStartingState)
                     {
                        stateStyle = StateMachineEditor.StartStyle;
                     }
                     else
                     {
                        stateStyle = StateMachineEditor.DefaultStyle;
                     }
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
         genericMenu.AddItem(new GUIContent("Set as start state"), false, OnSetStartState);
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
      /// Calls actions when set as the starting state
      /// </summary>
      private void OnSetStartState()
      {
         if(StartState != null)
         {
            StartState(this);
         }
      }

      public void SetStyle(GUIStyle style)
      {
         stateStyle = style;
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
            t.TransitionsForState = new List<Transition>();
            t.TransitionsForState.Add(transition);
            stateTransitionInfo.Add(endState, t);
         }
         else
         {
            stateTransitionInfo[endState].TransitionsForState.Add(transition);
         }

         UpdateTriangleRotation(endState);
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

      public void RemoveConnectedState(State state, Transition transition)
      {
         stateTransitionInfo.Remove(state);
         this.state.Transitions.Remove(transition);
         connectedStates.Remove(state);
      }
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
      private float rotation;
      private List<Transition> transitionsForState;

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
   }
}