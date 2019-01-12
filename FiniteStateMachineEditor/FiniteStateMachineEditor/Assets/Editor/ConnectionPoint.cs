using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{

	private Rect rect;
	private ConnectionPointType type;
	private Node node;
	private GUIStyle style;

	public Node ConnectedNode
	{ get { return node; } }

	private Action<ConnectionPoint> OnClickConnectionPoint;

	public Rect Rectangle
	{ get { return rect; } }

	public ConnectionPoint(Node node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
	{
		this.node = node;
		this.type = type;
		this.style = style;
		this.OnClickConnectionPoint = OnClickConnectionPoint;
		rect = new Rect(0.0f, 0.0f, 10.0f, 20.0f);
	}

	public void Draw()
	{
		rect.y = node.Rectangle.y + (node.Rectangle.height * 0.5f) - rect.height * 0.5f;

		switch(type)
		{
			case ConnectionPointType.In:
				rect.x = node.Rectangle.x - rect.width + 8f;
				break;
			case ConnectionPointType.Out:
				rect.x = node.Rectangle.x + node.Rectangle.width - 8f;
				break;
		}

		if(GUI.Button(rect, "", style))
		{
			if(OnClickConnectionPoint != null)
			{
				OnClickConnectionPoint(this);
			}
		}
	}


}
