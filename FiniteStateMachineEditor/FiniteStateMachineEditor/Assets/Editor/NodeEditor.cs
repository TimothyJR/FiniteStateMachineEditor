using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeEditor : EditorWindow
{
	private List<Node> nodes;
	private List<Connection> connections;

	private GUIStyle defaultStyle;
	private GUIStyle selectedStyle;
	private GUIStyle inPointStyle;
	private GUIStyle outPointStyle;

   private bool creatingTransition = false;
	private Node selectedInPoint;
	private Node selectedOutPoint;

	private Vector2 drag;
	private Vector2 offset;

   private float menuBarHeight = 20.0f;
   private Rect menuBar;

   private static NodeEditorSavedObjects savedGraph;

	public static void OpenWindow(NodeEditorSavedObjects openedGraph, string pathToOpenedGraph)
	{
		NodeEditor window = GetWindow<NodeEditor>();
		window.titleContent = new GUIContent("Node Editor");

      savedGraph = openedGraph;
	}

	private void OnEnable()
	{
      LoadGraph();

		defaultStyle = new GUIStyle();
		defaultStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
		defaultStyle.border = new RectOffset(12, 12, 12, 12);

		selectedStyle = new GUIStyle();
		selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
		selectedStyle.border = new RectOffset(12, 12, 12, 12);

		inPointStyle = new GUIStyle();
		inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
		inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
		inPointStyle.border = new RectOffset(4, 4, 12, 12);

		outPointStyle = new GUIStyle();
		outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
		outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
		outPointStyle.border = new RectOffset(4, 4, 12, 12);
	}

   private void LoadGraph()
   {
      nodes = savedGraph.Nodes;
      connections = savedGraph.Connections;
   }

	private void OnGUI()
	{
		DrawGrid(20.0f, 0.2f, Color.gray);
		DrawGrid(100.0f, 0.4f, Color.gray);
      DrawMenuBar();

		DrawConnections();
		DrawConnectionLine(Event.current);
      DrawNodes();

      ProcessNodeEvents(Event.current);
		ProcessEvents(Event.current);

		if(GUI.changed)
		{
			Repaint();
		}
	}

   private void DrawMenuBar()
   {
      menuBar = new Rect(0, 0, position.width, menuBarHeight);

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

	private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
	{
		int widthDivisions = Mathf.CeilToInt(position.width / gridSpacing);
		int heightDivisions = Mathf.CeilToInt(position.height / gridSpacing);

		Handles.BeginGUI();
		Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

		offset += drag * 0.5f;
		Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

		for(int i = 0; i < widthDivisions; i++)
		{
			Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0.0f) + newOffset, new Vector3(gridSpacing * i, position.height, 0.0f) + newOffset);
		}

		for(int i = 0; i < heightDivisions; i++)
		{
			Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * i, 0.0f) + newOffset, new Vector3(position.width, gridSpacing * i, 0.0f) + newOffset);
		}

		Handles.color = Color.white;
		Handles.EndGUI();
	}

	private void DrawNodes()
	{
		if(nodes != null)
		{
			for(int i = 0; i < nodes.Count; i++)
			{
				nodes[i].Draw();
			}
		}
	}

	private void DrawConnections()
	{
		if(connections != null)
		{
			for(int i = 0; i < connections.Count; i++)
			{
				connections[i].Draw();
			}
		}
	}

	private void DrawConnectionLine(Event e)
	{
		if(selectedInPoint != null && selectedOutPoint == null)
		{
			//Handles.DrawBezier(selectedInPoint.Rectangle.center, e.mousePosition, selectedInPoint.Rectangle.center + Vector2.left * 50.0f, e.mousePosition - Vector2.left * 50.0f, Color.white, null, 2.0f);
         Handles.DrawLine(selectedInPoint.Rectangle.center, e.mousePosition);
         GUI.changed = true;
		}

		if(selectedOutPoint != null && selectedInPoint == null)
		{
			Handles.DrawBezier(selectedOutPoint.Rectangle.center, e.mousePosition, selectedOutPoint.Rectangle.center - Vector2.left * 50.0f, e.mousePosition + Vector2.left * 50.0f, Color.white, null, 2.0f);
			GUI.changed = true;
		}
	}

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

	private void ProcessNodeEvents(Event e)
	{
		if(nodes != null)
		{
			// Go through nodes backwards since last node is drawn on top
			for(int i = nodes.Count - 1; i >= 0; i--)
			{
				bool guiChanged = nodes[i].ProcessEvent(e);

				if(guiChanged)
				{
					GUI.changed = true;
				}
			}
		}
	}

	private void OnDrag(Vector2 delta)
	{
		drag = delta;

		if(nodes != null)
		{
			for(int i = 0; i < nodes.Count; i++)
			{
				nodes[i].DragNode(delta);
			}
		}
	}

	private void ProcessContextMenu(Vector2 mousePosition)
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
		genericMenu.ShowAsContext();
	}

	private void OnClickAddNode(Vector2 mousePosition)
	{
		if(nodes == null)
		{
			nodes = new List<Node>();
		}

		nodes.Add(new Node(mousePosition, 200, 50, defaultStyle, selectedStyle, inPointStyle, outPointStyle, OnClickRemoveNode, OnStartConnection, OnNodeClick));
	}

	private void OnStartConnection(Node node)
	{
		selectedInPoint = node;
      creatingTransition = true;
	}

	private void OnNodeClick(Node node)
	{
      if(creatingTransition)
      {
         selectedOutPoint = node;
         if (selectedOutPoint != selectedInPoint)
   		{
   			CreateConnection();
   			ClearConnectionSelection();
   		}
      }
      
   }

	private void OnClickRemoveConnection(Connection connection)
	{
		connections.Remove(connection);
	}

	private void CreateConnection()
	{
		if(connections == null)
		{
			connections = new List<Connection>();
		}

		connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
	}

	private void OnClickRemoveNode(Node node)
	{
		if(connections != null)
		{
			List<Connection> connectionsToRemove = new List<Connection>();

			for(int i = 0; i < connections.Count; i++)
			{
				if(connections[i].InPoint == node || connections[i].OutPoint == node)
				{
					connectionsToRemove.Add(connections[i]);
				}
			}
			for(int i = 0; i < connectionsToRemove.Count; i++)
			{
				connections.Remove(connectionsToRemove[i]);
			}

		}

		nodes.Remove(node);
	}

	private void ClearConnectionSelection()
	{
		selectedInPoint = null;
		selectedOutPoint = null;
	}

   private void Save()
   {
      savedGraph.Nodes = nodes;
      savedGraph.Connections = connections;

      AssetDatabase.SaveAssets();
   }


}
