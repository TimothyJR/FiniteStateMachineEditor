﻿using UnityEngine;

// Class used to draw lines in editor
// Based on the script found at http://wiki.unity3d.com/index.php/DrawLine

namespace FStateMachine
{
	public class EditorDraw
	{
		// Texture for drawing a line
		private static Texture2D lineTexture;

		// Texture for drawing a triangle
		private static Texture2D triangleTexture;

		/// <summary>
		/// Draws a line between two points
		/// </summary>
		/// <param name="pointA"></param>
		/// <param name="pointB"></param>
		/// <param name="color"></param>
		/// <param name="width"></param>
		public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
		{
			Matrix4x4 savedGUIMatrix = GUI.matrix;

			if (lineTexture == null)
			{
				lineTexture = Resources.Load<Texture2D>("StateMachine/FiniteStateMachineTile");
			}

			Color savedGUIColor = GUI.color;
			GUI.color = color;

			float angle = Vector3.Angle(pointB - pointA, Vector2.right);

			if (pointA.y > pointB.y)
			{
				angle = -angle;
			}

			// Scale and rotate
			GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
			GUIUtility.RotateAroundPivot(angle, pointA);

			GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTexture);


			// Revert the color and matrix
			GUI.matrix = savedGUIMatrix;
			GUI.color = savedGUIColor;

		}

		/// <summary>
		/// Draws line within the bounds of an editor to prevent the line from being invisible
		/// </summary>
		/// <param name="pointA"></param>
		/// <param name="pointB"></param>
		/// <param name="color"></param>
		/// <param name="width"></param>
		/// <param name="boundsX"></param>
		/// <param name="boundsY"></param>
		public static void DrawLineInEditorBounds(Vector2 pointA, Vector2 pointB, Color color, float width, float boundsX,  float boundsY)
		{
			Matrix4x4 savedGUIMatrix = GUI.matrix;

			if (lineTexture == null)
			{
				lineTexture = Resources.Load<Texture2D>("StateMachine/FiniteStateMachineTile");
			}

			Color savedGUIColor = GUI.color;
			GUI.color = color;

			// ScaleAroundPivot does not work properly when pointA is outside the editor bounds
			// So move to a point along the line within the bounds
			if(pointA.x > boundsX)
			{
				if(pointB.x > boundsX)
				{
					return;
				}

				float slope = (pointA.y - pointB.y) / (pointA.x - pointB.x);
				// b = y - m * x
				float b = pointA.y - slope * pointA.x;
				// Line is y = m * x + b
				// X is boundsX
				pointA = new Vector2(boundsX - 1, slope * (boundsX - 1) + b);

			}
			else if(pointA.x < 0)
			{
				if (pointA.x < 0)
				{
					return;
				}

				float slope = (pointA.y - pointB.y) / (pointA.x - pointB.x);
				// b = y - m * x
				float b = pointA.y - slope * pointA.x;
				// Line is y = m * x + b
				// X is 0
				pointA = new Vector2(0, b);
			}

			if(pointA.y > boundsY)
			{
				if(pointB.y > boundsY)
				{
					return;
				}

				float slope = (pointA.y - pointB.y) / (pointA.x - pointB.x);
				// b = y - m * x
				float b = pointA.y - slope * pointA.x;
				// x = (y - b) / m
				// y is boundsY
				pointA = new Vector2(((boundsY - 1) - b) / slope, (boundsY - 1));
			}
			else if(pointA.y < 0)
			{
				if(pointB.y < 0)
				{
					return;
				}

				float slope = (pointA.y - pointB.y) / (pointA.x - pointB.x);
				// b = y - m * x
				float b = pointA.y - slope * pointA.x;
				// x = (y - b) / m
				// y is 0
				pointA = new Vector2((0 - b) / slope, 0);
			}

			float angle = Vector3.Angle(pointB - pointA, Vector2.right);

			if (pointA.y > pointB.y)
			{
				angle = -angle;
			}

			// Scale and rotate
			GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
			GUIUtility.RotateAroundPivot(angle, pointA);

			GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTexture);


			// Revert the color and matrix
			GUI.matrix = savedGUIMatrix;
			GUI.color = savedGUIColor;
		}

		/// <summary>
		/// Draws a triangle at the specified position and rotation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <param name="color"></param>
		/// <param name="size"></param>
		public static void DrawTriangle(Vector2 position, float rotation, Color color, float size)
		{
			Matrix4x4 savedGUIMatrix = GUI.matrix;

			if (triangleTexture == null)
			{
				triangleTexture = Resources.Load<Texture2D>("StateMachine/FiniteStateMachineTriangle");
			}

			Color savedGUIColor = GUI.color;
			GUI.color = color;

			// Scale and rotate
			GUIUtility.ScaleAroundPivot(new Vector2(size, size), position);
			GUIUtility.RotateAroundPivot(rotation, position);

			GUI.DrawTexture(new Rect(position.x - size / 2, position.y - size / 2, size, size), triangleTexture, ScaleMode.ScaleToFit, true);
			// Revert the color and matrix
			GUI.matrix = savedGUIMatrix;
			GUI.color = savedGUIColor;
		}
	}
}