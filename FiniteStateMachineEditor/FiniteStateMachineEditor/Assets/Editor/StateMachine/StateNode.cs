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
      Vector2 transitionClickableSize;

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
      public StateNode(State state, Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked, Action<StateNode> OnClickStartState)
      {
         this.state = state;

         Init(OnClickRemoveState, OnClickCreateTransition, OnClicked, OnChanged, OnTransitionClicked, OnClickStartState);
      }

      /// <summary>
      /// Used for shared code between the different constructors
      /// </summary>
      /// <param name="OnClickRemoveState"></param>
      /// <param name="OnClickCreateTransition"></param>
      /// <param name="OnClicked"></param>
      /// <param name="OnChanged"></param>
      public void Init(Action<StateNode> OnClickRemoveState, Action<StateNode> OnClickCreateTransition, Action<StateNode> OnClicked, Action<StateNode> OnChanged, Action<TransitionDataHolder> OnTransitionClicked, Action<StateNode> OnClickStartState)
      {
         stateStyle = StateMachineEditor.DefaultStyle;

         connectedStates = new List<State>();

         OnRemoveNode = OnClickRemoveState;
         OnCreateConnection = OnClickCreateTransition;
         Clicked = OnClicked;
         Changed = OnChanged;
         StartState = OnClickStartState;
         this.OnTransitionClicked = OnTransitionClicked;
         stateTransitionInfo = new Dictionary<State, TransitionDataHolder>();
         transitionClickableSize = new Vector2(20.0f, 20.0f);

         for (int i = 0; i < state.Transitions.Count; i++)
         {
            if (!connectedStates.Contains(state.Transitions[i].NextState))
            {
               connectedStates.Add(state.Transitions[i].NextState);
               TransitionDataHolder t = ScriptableObject.CreateInstance<TransitionDataHolder>();
               t.Init(RemoveTransition, RemoveTransitionFromInspector);
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
            stateTransitionInfo[changedState].ClickableArea = new Rect(((state.Rectangle.center + changedState.Rectangle.center) / 2) - (transitionClickableSize / 2), transitionClickableSize);
         }
         else if (state == changedState)
         {
            for (int i = 0; i < connectedStates.Count; i++)
            {
               float rotation = Vector2.SignedAngle(new Vector2(0, -1.0f), connectedStates[i].Rectangle.center - changedState.Rectangle.center);
               stateTransitionInfo[connectedStates[i]].Rotation = rotation;
               stateTransitionInfo[connectedStates[i]].ClickableArea = new Rect(((changedState.Rectangle.center + connectedStates[i].Rectangle.center) / 2) - (transitionClickableSize / 2), transitionClickableSize);
            }
         }

      }

      /// <summary>
      /// Draws the node
      /// </summary>
      public void Draw()
      {
         GUI.skin.label.alignment = TextAnchor.MiddleCenter;
         GUI.Box(state.Rectangle, "", stateStyle);
         GUI.Label(state.Rectangle, state.StateName);
      }

      /// <summary>
      /// Draws the node using the style for the starting state
      /// </summary>
      public void DrawStartState()
      {
         GUI.skin.label.alignment = TextAnchor.MiddleCenter;
         if (stateStyle == StateMachineEditor.SelectedStyle)
         {
            GUI.Box(state.Rectangle, "", StateMachineEditor.StartSelectedStyle);
         }
         else if(stateStyle == StateMachineEditor.DefaultStyle)
         {
            GUI.Box(state.Rectangle, "", StateMachineEditor.StartStyle);
         }
         GUI.Label(state.Rectangle, state.StateName);
      }

      /// <summary>
      /// Draws a line from the inpoint to the outpoint
      /// </summary>
      public void DrawTransitions()
      {
         for (int i = 0; i < connectedStates.Count; i++)
         {
            Vector2 trianglePosition = (state.Rectangle.center + connectedStates[i].Rectangle.center) / 2;
            if (stateTransitionInfo[connectedStates[i]].Selected)
            {
               Color highlightColor = new Color(0.3f, 0.3f, 1.0f, 0.3f);
               EditorDraw.DrawLineInEditorBounds(state.Rectangle.center, connectedStates[i].Rectangle.center, highlightColor, 8, StateMachineEditor.EditorWidth, StateMachineEditor.EditorHeight);
               EditorDraw.DrawTriangle(trianglePosition, stateTransitionInfo[connectedStates[i]].Rotation, highlightColor, 6);
            }

            EditorDraw.DrawLineInEditorBounds(state.Rectangle.center, connectedStates[i].Rectangle.center, Color.white, 5, StateMachineEditor.EditorWidth, StateMachineEditor.EditorHeight);
            EditorDraw.DrawTriangle(trianglePosition, stateTransitionInfo[connectedStates[i]].Rotation, Color.white, 5);
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
                     stateStyle = StateMachineEditor.SelectedStyle;

                     if (Clicked != null)
                     {
                        Clicked(this);
                     }
                  }
                  else
                  {
                     GUI.changed = true;
                     stateStyle = StateMachineEditor.DefaultStyle;
                  }
               }
               if (e.button == 1)
               {
                  if (state.Rectangle.Contains(e.mousePosition))
                  {
                     GUI.changed = true;
                     stateStyle = StateMachineEditor.SelectedStyle;

                     ProcessContextMenu();
                     e.Use();
                  }
                  else
                  {
                     GUI.changed = true;
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
      /// Processes events for the transitions
      /// </summary>
      /// <param name="e"></param>
      /// <param name="transition"></param>
      /// <returns></returns>
      public void ProcessTransitionEvents(Event e)
      {
         for(int i = 0; i < connectedStates.Count; i++)
         {
            switch (e.type)
            {
               case EventType.MouseDown:
                  if (stateTransitionInfo[connectedStates[i]].ClickableArea.Contains(e.mousePosition))
                  {
                     if (e.button == 0)
                     {
                        stateTransitionInfo[connectedStates[i]].Selected = true;
                        if (OnTransitionClicked != null)
                        {
                           OnTransitionClicked(stateTransitionInfo[connectedStates[i]]);
                        }
                     }
                     else if (e.button == 1)
                     {
                        ProcessTransitionContextMenu(stateTransitionInfo[connectedStates[i]]);
                        e.Use();
                     }
                  }
                  else
                  {
                     stateTransitionInfo[connectedStates[i]].Selected = false;
                  }
                  break;
            }
         }
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

      private void ProcessTransitionContextMenu(TransitionDataHolder transition)
      {
         GenericMenu genericMenu = new GenericMenu();
         genericMenu.AddItem(new GUIContent("Remove Transition"), false, () => RemoveTransition(transition));
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
         if (StartState != null)
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
            TransitionDataHolder t = ScriptableObject.CreateInstance<TransitionDataHolder>();
            t.Init(RemoveTransition, RemoveTransitionFromInspector);
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

      /// <summary>
      /// Removed a connection from this node
      /// </summary>
      /// <param name="transitionToRemove"></param>
      public void RemoveTransition(TransitionDataHolder transition)
      {
         stateTransitionInfo.Remove(transition.TransitionsForState[0].NextState);
         connectedStates.Remove(transition.TransitionsForState[0].NextState);
         for(int i = 0; i < transition.TransitionsForState.Count; i++)
         {
            state.Transitions.Remove(transition.TransitionsForState[i]);
         }
      }

      /// <summary>
      /// Removes a state that this state transitioned into
      /// </summary>
      /// <param name="state"></param>
      /// <param name="transition"></param>
      public void RemoveConnectedState(State state, Transition transition)
      {
         if(stateTransitionInfo.ContainsKey(state))
         {
            stateTransitionInfo.Remove(state);
         }
         this.state.Transitions.Remove(transition);
         connectedStates.Remove(state);
         
      }

      /// <summary>
      /// Used to remove transition to a state from the transition data holder inspector
      /// </summary>
      /// <param name="transition"></param>
      private void RemoveTransitionFromInspector(Transition transition, TransitionDataHolder transitionData)
      {
         state.Transitions.Remove(transition);
         transitionData.TransitionsForState.Remove(transition);
      }
   }
}