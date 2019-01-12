using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Holds data for the editor
/// Each node handles dragging and removing itself
/// </summary>
public class Node
{
	private Rect rectangle;
	private string title;

	private GUIStyle nodeStyle;
	private GUIStyle defaultStyle;

	private GUIStyle selectedStyle;

	private bool isDragged;
	private bool isSelected;

	private ConnectionPoint inPoint;
	private ConnectionPoint outPoint;

	public Action<Node> OnRemoveNode;

	public Rect Rectangle
	{ get { return rectangle; } }

	public ConnectionPoint InPoint
	{ get { return inPoint; } }

	public ConnectionPoint OutPoint
	{ get { return outPoint; } }

	public Node(Vector2 position, float width, float height, GUIStyle defaultStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode)
	{
		rectangle = new Rect(position.x, position.y, width, height);
		nodeStyle = defaultStyle;
		this.defaultStyle = defaultStyle;
		this.selectedStyle = selectedStyle;
		inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
		outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
		OnRemoveNode = OnClickRemoveNode;
	}

	public void DragNode(Vector2 delta)
	{
		rectangle.position += delta;
	}

	public void Draw()
	{
		inPoint.Draw();
		outPoint.Draw();
		GUI.Box(rectangle, title, nodeStyle);
	}

	public bool ProcessEvent(Event e)
	{
		switch(e.type)
		{
			case EventType.MouseDown:
				if(e.button == 0)
				{
					if(rectangle.Contains(e.mousePosition))
					{
						isDragged = true;
						GUI.changed = true;
						isSelected = true;
						nodeStyle = selectedStyle;
					}
					else
					{
						GUI.changed = true;
						isSelected = false;
						nodeStyle = defaultStyle;
					}
				}
				if(e.button == 1 && isSelected && rectangle.Contains(e.mousePosition))
				{
					ProcessContextMenu();
					e.Use();
				}
				break;

			case EventType.MouseUp:
				isDragged = false;
				break;
			case EventType.MouseDrag:
				if(e.button == 0 && isDragged)
				{
					DragNode(e.delta);
					e.Use();
					return true;
				}
				break;
		

		}
		return false;
	}

	private void ProcessContextMenu()
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
		genericMenu.ShowAsContext();
	}

	private void OnClickRemoveNode()
	{
		if(OnRemoveNode != null)
		{
			OnRemoveNode(this);
		}

	}
}
