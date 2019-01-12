using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

/// <summary>
/// Holds a connection between two nodes and the direction of that connection.
/// </summary>
public class Connection
{
	private ConnectionPoint inPoint;
	private ConnectionPoint outPoint;
	private Action<Connection> OnClickedRemoveConnection;

	public ConnectionPoint InPoint
	{ get { return inPoint; } }

	public ConnectionPoint OutPoint
	{ get { return outPoint; } }

	public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickedRemoveConnection)
	{
		this.inPoint = inPoint;
		this.outPoint = outPoint;
		this.OnClickedRemoveConnection = OnClickedRemoveConnection;
	}

	public void Draw()
	{
		Handles.DrawBezier(inPoint.Rectangle.center, outPoint.Rectangle.center, inPoint.Rectangle.center + Vector2.left * 50f, outPoint.Rectangle.center - Vector2.left * 50f, Color.white, null, 2.0f);

		if(Handles.Button((inPoint.Rectangle.center + outPoint.Rectangle.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
		{
			if(OnClickedRemoveConnection != null)
			{
				OnClickedRemoveConnection(this);
			}
		}
	}
}
