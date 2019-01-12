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

   private bool creatingTransition = false;
   private Node selectedInPoint;
   private Node selectedOutPoint;

   private Vector2 drag;
   private Vector2 offset;

   private float menuBarHeight = 20.0f;
   private Rect menuBar;

   private static NodeEditorSavedObjects savedGraph;

   public static void OpenWindow(NodeEditorSavedObjects openedGraph)
   {
      NodeEditor window = GetWindow<NodeEditor>();
      window.titleContent = new GUIContent("Node Editor");

      savedGraph = openedGraph;
   }

   /// <summary>
   /// Loads the graph from the scriptable asset and sets styles
   /// </summary>
   private void OnEnable()
   {
      LoadGraph();

      defaultStyle = new GUIStyle();
      defaultStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
      defaultStyle.border = new RectOffset(12, 12, 12, 12);

      selectedStyle = new GUIStyle();
      selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
      selectedStyle.border = new RectOffset(12, 12, 12, 12);
   }

   /// <summary>
   /// Grabs the info from the scriptable object.
   /// </summary>
   private void LoadGraph()
   {
      nodes = savedGraph.Nodes;
      connections = savedGraph.Connections;
   }

   /// <summary>
   /// Updates the editor visuals
   /// </summary>
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

   /// <summary>
   /// Handles drawing the bar at the top of the editor
   /// </summary>
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

   /// <summary>
   /// Draws each node
   /// </summary>
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

   /// <summary>
   /// Draws each connection
   /// </summary>
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

   /// <summary>
   /// Draws the line for the connections
   /// </summary>
   /// <param name="e"></param>
   private void DrawConnectionLine(Event e)
   {
      if(selectedInPoint != null && selectedOutPoint == null && creatingTransition)
      {
         Handles.DrawLine(selectedInPoint.Rectangle.center, e.mousePosition);
         GUI.changed = true;
      }
   }

   /// <summary>
   /// Handles mouse input
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

   /// <summary>
   /// Handles dragging of the nodes
   /// </summary>
   /// <param name="delta"></param>
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

   /// <summary>
   /// Creates a context menu when right clicking on the grid
   /// </summary>
   /// <param name="mousePosition"></param>
   private void ProcessContextMenu(Vector2 mousePosition)
   {
      GenericMenu genericMenu = new GenericMenu();
      genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
      genericMenu.ShowAsContext();
   }

   /// <summary>
   /// Creates a new node on the grid
   /// </summary>
   /// <param name="mousePosition"></param>
   private void OnClickAddNode(Vector2 mousePosition)
   {
      if(nodes == null)
      {
         nodes = new List<Node>();
      }

      nodes.Add(new Node(mousePosition, 200, 50, defaultStyle, selectedStyle, OnClickRemoveNode, OnStartConnection, OnNodeClick));
   }

   /// <summary>
   /// Starts a transition connection
   /// </summary>
   /// <param name="node"></param>
   private void OnStartConnection(Node node)
   {
      selectedInPoint = node;
      creatingTransition = true;
   }

   /// <summary>
   /// Handles completing a transition connection
   /// </summary>
   /// <param name="node"></param>
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

   /// <summary>
   /// Removes a connection when they are clicked
   /// </summary>
   /// <param name="connection"></param>
   private void OnClickRemoveConnection(Connection connection)
   {
      connections.Remove(connection);
   }

   /// <summary>
   /// Creates a connection
   /// </summary>
   private void CreateConnection()
   {
      if(connections == null)
      {
         connections = new List<Connection>();
      }

      connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
   }

   /// <summary>
   /// Removed a node and all of its connections
   /// </summary>
   /// <param name="node"></param>
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

   /// <summary>
   /// Clears out the nodes for creating a connection
   /// </summary>
   private void ClearConnectionSelection()
   {
      selectedInPoint = null;
      selectedOutPoint = null;
   }

   /// <summary>
   /// Save the graph to the scriptable object
   /// </summary>
   private void Save()
   {
      savedGraph.Nodes = nodes;
      savedGraph.Connections = connections;

      AssetDatabase.SaveAssets();
   }


}
