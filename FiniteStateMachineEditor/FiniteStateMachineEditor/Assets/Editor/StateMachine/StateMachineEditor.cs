using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StateMachine
{
   public class StateMachineEditor : EditorWindow
   {
      private List<StateNode> states;
      private List<StateNode> addedStates;
      private List<StateNode> removedStates;

      private static StateMachine currentSM;
      private static GUIStyle defaultStyle;
      private static GUIStyle selectedStyle;
      private static GUIStyle startStyle;
      private static GUIStyle startSelectedStyle;

      private Vector2 offset;
      private Vector2 drag;
      private float previousWidth;
      private float previousHeight;

      private Rect menuBar;
      private float menuBarHeight = 20.0f;

      StateNode transitionStartState;
      StateNode transitionEndState;
      bool creatingTransition;

      public static GUIStyle DefaultStyle
      { get { return defaultStyle; } }

      public static GUIStyle SelectedStyle
      { get { return selectedStyle; } }

      public static GUIStyle StartStyle
      { get { return startStyle; } }

      public static GUIStyle StartSelectedStyle
      { get { return startSelectedStyle; } }

      /// <summary>
      /// Opens a StateMachine scriptable object when it is double clicked in inspector
      /// </summary>
      /// <param name="openedSM"></param>
      public static void OpenWindow(StateMachine openedSM)
      {
         currentSM = openedSM;
         StateMachineEditor window = GetWindow<StateMachineEditor>();
         window.titleContent = new GUIContent("State Machine Editor");
      }

      /// <summary>
      /// Loads state machine graph and set styles.
      /// </summary>
      private void OnEnable()
      {
         previousWidth = position.width;
         previousHeight = position.height;

         LoadGraph();

         defaultStyle = new GUIStyle();
         defaultStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
         defaultStyle.border = new RectOffset(12, 12, 12, 12);

         selectedStyle = new GUIStyle();
         selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
         selectedStyle.border = new RectOffset(12, 12, 12, 12);

         startStyle = new GUIStyle();
         startStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
         startStyle.border = new RectOffset(12, 12, 12, 12);

         startSelectedStyle = new GUIStyle();
         startSelectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D;
         startSelectedStyle.border = new RectOffset(12, 12, 12, 12);

         menuBar = new Rect(0, 0, position.width, menuBarHeight);


      }

      /// <summary>
      /// Updates the editor visuals
      /// </summary>
      private void OnGUI()
      {
         HandleResolutionChange();

         DrawGrid(20.0f, 0.2f, Color.gray);
         DrawGrid(100.0f, 0.4f, Color.gray);

         DrawTransitions();
         DrawConnectionLine(Event.current);
         DrawStateNodes();

         DrawMenuBar();

         ProcessStateEvents(Event.current);
         ProcessEvents(Event.current);

         if (GUI.changed)
         {
            Repaint();
         }
      }

      /// <summary>
      /// Makes the menu bar change size only when the resolution is changed
      /// </summary>
      private void HandleResolutionChange()
      {
         if (position.width != previousWidth)
         {
            menuBar.width = position.width;

            previousWidth = position.width;
         }

         // Use this if there is a need for something to change with the height
         //if (position.height != previousHeight)
         //{
         //   previousHeight = position.height;
         //}
      }


      /// <summary>
      /// Draws a grid as the background of the editor to give a sense of space
      /// </summary>
      /// <param name="gridSpacing"></param>
      /// <param name="gridOpacity"></param>
      /// <param name="gridColor"></param>
      private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
      {
         int widthDivisions = Mathf.CeilToInt(position.width / gridSpacing);
         int heightDivisions = Mathf.CeilToInt(position.height / gridSpacing);

         Handles.BeginGUI();
         Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

         offset += drag * 0.5f;
         Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

         for (int i = 0; i < widthDivisions; i++)
         {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0.0f) + newOffset, new Vector3(gridSpacing * i, position.height, 0.0f) + newOffset);
         }

         for (int i = 0; i < heightDivisions; i++)
         {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * i, 0.0f) + newOffset, new Vector3(position.width, gridSpacing * i, 0.0f) + newOffset);
         }

         Handles.color = Color.white;
         Handles.EndGUI();
      }

      /// <summary>
      /// Draws the menu bar at the top of the editor
      /// </summary>
      private void DrawMenuBar()
      {
         GUILayout.BeginArea(menuBar, EditorStyles.toolbar);
         GUILayout.BeginHorizontal();

         if (GUILayout.Button(new GUIContent("Save"), EditorStyles.toolbarButton, GUILayout.Width(35.0f)))
         {
            Save();
         }

         GUILayout.Space(5.0f);

         GUILayout.EndHorizontal();
         GUILayout.EndArea();
      }

      /// <summary>
      /// Draw the states in the editor
      /// </summary>
      private void DrawStateNodes()
      {
         for(int i = 0; i < states.Count; i++)
         {
            states[i].Draw();
         }
      }

      /// <summary>
      /// Goes through the nodes and draws all of their connections
      /// Must be done separately from the DrawNodes loop in order for the nodes to draw over connections
      /// </summary>
      private void DrawTransitions()
      {

         for (int i = 0; i < states.Count; i++)
         {
            states[i].DrawTransitions();
         }

      }

      /// <summary>
      /// Draws the line for the connections
      /// </summary>
      /// <param name="e"></param>
      private void DrawConnectionLine(Event e)
      {
         if (transitionStartState != null && transitionEndState == null && creatingTransition)
         {
            Handles.DrawLine(transitionStartState.NodeState.Rectangle.center, e.mousePosition);
            GUI.changed = true;
         }
      }

      /// <summary>
      /// Handles inputs
      /// </summary>
      /// <param name="e"></param>
      private void ProcessEvents(Event e)
      {
         drag = Vector2.zero;
         switch(e.type)
         {
            case EventType.MouseDown:
               if(e.button == 0)
               {
                  creatingTransition = false;
               }
               else if(e.button == 1)
               {
                  ProcessContextMenu(e.mousePosition);
               }
               break;
            case EventType.MouseDrag:
               if(e.button == 0)
               {
                  OnDrag(e.delta);
                  GUI.changed = true;
               }
               break;
         }
      }

      /// <summary>
      /// Gives mouse input to the nodes
      /// </summary>
      /// <param name="e"></param>
      private void ProcessStateEvents(Event e)
      {

         // Go through nodes backwards since last node is drawn on top
         for (int i = states.Count - 1; i >= 0; i--)
         {
            bool guiChanged = states[i].ProcessEvent(e);

            if (guiChanged)
            {
               GUI.changed = true;
            }
         }

      }

      /// <summary>
      /// Handles dragging of the nodes
      /// </summary>
      /// <param name="delta"></param>
      private void OnDrag(Vector2 delta)
      {
         drag = delta;


         for (int i = 0; i < states.Count; i++)
         {
            states[i].DragState(delta);
         }


         for (int i = 0; i < states.Count; i++)
         {
            states[i].UpdateTriangle(states[i].NodeState);
         }
      }

      /// <summary>
      /// Creates a context menu when you right click on the grid
      /// </summary>
      /// <param name="mousePosition"></param>
      private void ProcessContextMenu(Vector2 mousePosition)
      {
         GenericMenu genericMenu = new GenericMenu();
         genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddState(mousePosition));
         genericMenu.ShowAsContext();
      }

      /// <summary>
      /// Creates a new node on the grid
      /// </summary>
      /// <param name="mousePosition"></param>
      private void OnClickAddState(Vector2 mousePosition)
      {
         states.Add(new StateNode(mousePosition, 200, 50, OnClickRemoveState, OnStartTransition, OnStateClick, OnStateChange, OnTransitionClicked));
         addedStates.Add(states[states.Count - 1]);
      }

      /// <summary>
      /// Starts a transition connection
      /// </summary>
      /// <param name="state"></param>
      private void OnStartTransition(StateNode state)
      {
         transitionStartState = state;
         creatingTransition = true;
      }

      /// <summary>
      /// Handles completing a transition connection
      /// </summary>
      /// <param name="state"></param>
      private void OnStateClick(StateNode state)
      {
         if(!creatingTransition)
         {
            // Bring up state info in inspector
         }
         else
         {
            transitionEndState = state;
            if (transitionEndState != transitionStartState)
            {
               CreateConnection();
               ClearConnectionSelection();
            }
         }
      }

      /// <summary>
      /// Creates a connection
      /// </summary>
      private void CreateConnection()
      {
         transitionStartState.CreateTransition(transitionEndState.NodeState);
      }

      /// <summary>
      /// Clears out the nodes for creating a connection
      /// </summary>
      private void ClearConnectionSelection()
      {
         transitionStartState = null;
         transitionEndState = null;
      }

      /// <summary>
      /// Removed a node and all of its connections
      /// </summary>
      /// <param name="state"></param>
      private void OnClickRemoveState(StateNode state)
      {
         for (int i = 0; i < states.Count; i++)
         {
            if (states[i] == state)
            {
         
            }
            else
            {
               for (int j = 0; j < states[i].NodeState.Transitions.Count; j++)
               {
                  if (states[i].NodeState.Transitions[j].NextState == state.NodeState)
                  {
                     states[i].NodeState.Transitions.Remove(states[i].NodeState.Transitions[j]);
                  }
               }
            }
         }
         states.Remove(state);
      }

      /// <summary>
      /// When a node is changed, update the connector so the triangle updates
      /// </summary>
      /// <param name="node"></param>
      private void OnStateChange(StateNode state)
      {
         for(int i = 0; i < states.Count; i++)
         {
            for (int j = 0; j < states[i].NodeState.Transitions.Count; j++)
            {
               states[i].UpdateTriangle(state.NodeState);
            }
         }
      }

      /// <summary>
      /// If a transition is clicked
      /// </summary>
      /// <param name="transitions"></param>
      private void OnTransitionClicked(TransitionDataHolder transitions)
      {

      }

      /// <summary>
      /// Loads data from the scriptable object
      /// </summary>
      private void LoadGraph()
      {
         states = new List<StateNode>();
         addedStates = new List<StateNode>();
         removedStates = new List<StateNode>();
         if (currentSM != null)
         {
            // Load data
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(currentSM));

            foreach (Object obj in assets)
            {
               if(obj.GetType() == typeof(State))
               {
                  states.Add(new StateNode((State)obj, OnClickRemoveState, OnStartTransition, OnStateClick, OnStateChange, OnTransitionClicked));
               }
            }
         }
      }

      /// <summary>
      /// Saves data to the scriptable object
      /// </summary>
      private void Save()
      {
         for(int i = 0; i < addedStates.Count; i++)
         {
            addedStates[i].NodeState.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(addedStates[i].NodeState, currentSM);
         }

         for(int i = 0; i < removedStates.Count; i++)
         {
            DestroyImmediate(removedStates[i].NodeState);
         }
         removedStates.Clear();

         AssetDatabase.SaveAssets();
      }
   }
}