using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace StateMachine
{
	public class StateMachineEditor : EditorWindow
	{
		private List<StateNode> states;
		private StateNode startingState;
		private StateNode anyStateNode;

		private static StateMachine currentSM;
		private static GUIStyle defaultStyle;
		private static GUIStyle selectedStyle;
		private static GUIStyle startStyle;
		private static GUIStyle startSelectedStyle;
		private static GUIStyle anyStateStyle;
		private static GUIStyle anyStateSelectedStyle;

		private Vector2 drag;
		private static float previousWidth;
		private static float previousHeight;

		private Rect menuBar;
		private float menuBarHeight = 20.0f;
		private float nodeWidth = 200.0f;
		private float nodeHeight = 50.0f;

		StateNode transitionStartState;
		StateNode transitionEndState;
		bool creatingTransition;

		bool subComponentEventOccured;

		public static GUIStyle DefaultStyle
		{ get { return defaultStyle; } }

		public static GUIStyle SelectedStyle
		{ get { return selectedStyle; } }

		public static GUIStyle StartStyle
		{ get { return startStyle; } }

		public static GUIStyle StartSelectedStyle
		{ get { return startSelectedStyle; } }

		public static GUIStyle AnyStateStyle
		{ get { return anyStateStyle; } }

		public static GUIStyle AnyStateSelectedStyle
		{ get { return anyStateSelectedStyle; } }

		public static float EditorWidth
		{ get { return previousWidth; } }

		public static float EditorHeight
		{ get { return previousHeight; } }

		public bool CreatingTransition
		{ set { creatingTransition = value; } }

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

			anyStateStyle = new GUIStyle();
			anyStateStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
			anyStateStyle.border = new RectOffset(12, 12, 12, 12);

			anyStateSelectedStyle = new GUIStyle();
			anyStateSelectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5 on.png") as Texture2D;
			anyStateSelectedStyle.border = new RectOffset(12, 12, 12, 12);

			LoadGraph();

			menuBar = new Rect(0, 0, position.width, menuBarHeight);

			Selection.activeObject = currentSM;
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

			ProcessTransitionEvents(Event.current);
			ProcessStateEvents(Event.current);
			ProcessEvents(Event.current);


			if (GUI.changed)
			{
				Repaint();
			}
		}

		/// <summary>
		/// Saves data to the scriptable object
		/// This makes the save button redundant
		/// This is necessary to prevent the state machine from having unused transitions remain in the file
		/// </summary>
		private void OnDestroy()
		{
			AssetDatabase.SaveAssets();
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

			if (position.height != previousHeight)
			{
				previousHeight = position.height;
			}
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

			currentSM.GraphOffset += drag * 0.5f;
			Vector3 newOffset = new Vector3(currentSM.GraphOffset.x % gridSpacing, currentSM.GraphOffset.y % gridSpacing, 0);

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

			if(anyStateNode != null)
			{
				anyStateNode.DrawAnyState();
			}

			if (startingState != null)
			{
				startingState.DrawStartState();
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

			if(anyStateNode != null)
			{
				anyStateNode.DrawTransitions();
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
				EditorDraw.DrawLine(transitionStartState.NodeState.Rectangle.center, e.mousePosition, Color.white, 3);
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
						if(!subComponentEventOccured)
						{
							Selection.activeObject = currentSM;
						}

					}
					else if(e.button == 1)
					{
						creatingTransition = false;
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
			bool guiChanged;
			// Go through nodes backwards since last node is drawn on top
			for (int i = states.Count - 1; i >= 0; i--)
			{

				guiChanged = states[i].ProcessEvent(e, this);

				if (guiChanged)
				{
					GUI.changed = true;
				}
			}

			if(anyStateNode != null)
			{
				guiChanged = anyStateNode.ProcessEvent(e, this);

				if (guiChanged)
				{
					GUI.changed = true;
				}
			}
		}

		private void ProcessTransitionEvents(Event e)
		{
			subComponentEventOccured = false;

			for (int i = 0; i < states.Count; i++)
			{
				states[i].ProcessTransitionEvents(e, this);
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
				states[i].UpdateTriangleRotation(states[i].NodeState);
			}

			if(anyStateNode != null)
			{
				anyStateNode.DragState(delta);
				anyStateNode.UpdateTriangleRotation(anyStateNode.NodeState);
			}
		}

		/// <summary>
		/// Creates a context menu when you right click on the grid
		/// </summary>
		/// <param name="mousePosition"></param>
		private void ProcessContextMenu(Vector2 mousePosition)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add State"), false, () => OnClickAddState(mousePosition));
			genericMenu.AddItem(new GUIContent("Go to Graph Origin"), false, ResetOffset);
			if (currentSM.AnyState == null)
			{
				genericMenu.AddItem(new GUIContent("Add Any State Node"), false, () => CreateAnyState(mousePosition));
			}

			genericMenu.ShowAsContext();
		}

		/// <summary>
		/// Creates a new node on the grid
		/// </summary>
		/// <param name="mousePosition"></param>
		private void OnClickAddState(Vector2 mousePosition)
		{
			states.Add(new StateNode(mousePosition, nodeWidth, nodeHeight, OnClickRemoveState, OnStartTransition, OnStateClick, OnStateChange, OnTransitionClicked, OnSetStartState));
			states[states.Count - 1].NodeState.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(states[states.Count - 1].NodeState, currentSM);
		}

		/// <summary>
		/// Brings the graph back to the origin
		/// </summary>
		private void ResetOffset()
		{
			OnDrag(-currentSM.GraphOffset);
		}

		/// <summary>
		/// Create a state that has transitions that are checked regardless of which state you are in
		/// </summary>
		/// <param name="mousePosition"></param>
		private void CreateAnyState(Vector2 mousePosition)
		{
			AnyState anyState = ScriptableObject.CreateInstance<AnyState>();
			anyState.StateName = "Any State";
			anyState.Rectangle = new Rect(mousePosition.x, mousePosition.y, nodeWidth, nodeHeight);

			anyStateNode = new StateNode(anyState, OnClickRemoveState, OnStartTransition, OnStateClick, OnStateChange, OnTransitionClicked, null);
			currentSM.AnyState = anyState;
			anyState.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(anyState, currentSM);
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
				Selection.activeObject = state.NodeState;
				subComponentEventOccured = true;
			}
			else
			{
				transitionEndState = state;
				if (transitionEndState != transitionStartState)
				{
					if(transitionEndState != anyStateNode)
					{
						CreateConnection();
						ClearConnectionSelection();
					}
					else
					{
						ClearConnectionSelection();
					}
				}
			}
		}

		/// <summary>
		/// Creates a connection
		/// </summary>
		private void CreateConnection()
		{
			transitionStartState.CreateTransition(transitionEndState);
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
				if (states[i] != state)
				{
					for (int j = 0; j < states[i].NodeState.Transitions.Count; j++)
					{
						if (states[i].NodeState.Transitions[j].NextState == state.NodeState)
						{
							states[i].RemoveConnectedState(state.NodeState, states[i].NodeState.Transitions[j]);
						}
					}
				}
			}

			for(int i = 0; i < state.NodeState.Transitions.Count; i++)
			{
				DestroyImmediate(state.NodeState.Transitions[i], true);
			}

			states.Remove(state);
			DestroyImmediate(state.NodeState, true);
		}

		/// <summary>
		/// Sets a state as the initial state in the state machine
		/// </summary>
		/// <param name="state"></param>
		private void OnSetStartState(StateNode state)
		{
			state.SetStyle(startStyle);
			if(startingState != null)
			{
				startingState.SetStyle(defaultStyle);
			}
			startingState = state;
			currentSM.CurrentState = state.NodeState;
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
					states[i].UpdateTriangleRotation(state.NodeState);
				}
			}

			if(anyStateNode != null)
			{
				anyStateNode.UpdateTriangleRotation(state.NodeState);
			}
		}

		/// <summary>
		/// If a transition is clicked
		/// </summary>
		/// <param name="transitions"></param>
		private void OnTransitionClicked(TransitionDataHolder transitions)
		{
			Selection.activeObject = transitions;
			subComponentEventOccured = true;
		}

		/// <summary>
		/// Loads data from the scriptable object
		/// </summary>
		private void LoadGraph()
		{
			states = new List<StateNode>();
			if (currentSM != null)
			{
				// Load data
				Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(currentSM));
				Dictionary<State, StateNode> stateToNodeRelation = new Dictionary<State, StateNode>();

				foreach (Object obj in assets)
				{
					if(obj != null)
					{
						if (obj.GetType() == typeof(State))
						{
							// Create all the states
							State state = (State)obj;
							StateNode newNode = new StateNode(state, OnClickRemoveState, OnStartTransition, OnStateClick, OnStateChange, OnTransitionClicked, OnSetStartState);
							states.Add(newNode);
							stateToNodeRelation.Add(state, newNode);
							if (state == currentSM.CurrentState)
							{
								startingState = newNode;
							}

						}
					}
				}

				if(currentSM.AnyState != null)
				{
					anyStateNode = new StateNode(currentSM.AnyState, OnClickRemoveState, OnStartTransition, OnStateClick, OnStateChange, OnTransitionClicked, null);
					for(int i = 0; i < currentSM.AnyState.Transitions.Count; i++)
					{
						anyStateNode.LoadTransition(stateToNodeRelation[currentSM.AnyState.Transitions[i].NextState], currentSM.AnyState.Transitions[i]);
					}
				}

				// Create all the transitions
				for(int i = 0; i < states.Count; i++)
				{
					for(int j = 0; j < states[i].NodeState.Transitions.Count; j++)
					{
						states[i].LoadTransition(stateToNodeRelation[states[i].NodeState.Transitions[j].NextState], states[i].NodeState.Transitions[j]);
					}
				}
			}
		}

		/// <summary>
		/// Saves data to the scriptable object
		/// </summary>
		private void Save()
		{
			AssetDatabase.SaveAssets();
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