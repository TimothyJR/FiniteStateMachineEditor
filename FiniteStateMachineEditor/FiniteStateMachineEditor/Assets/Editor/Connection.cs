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
   private Node inPoint;
   private Node outPoint;
   private Action<Connection> OnClickedRemoveConnection;

   Vector2[] trianglePoints;

   public Node InPoint
   { get { return inPoint; } }

   public Node OutPoint
   { get { return outPoint; } }

   public Connection(Node inPoint, Node outPoint, Action<Connection> OnClickedRemoveConnection)
   {
      this.inPoint = inPoint;
      this.outPoint = outPoint;
      this.OnClickedRemoveConnection = OnClickedRemoveConnection;

      trianglePoints = new Vector2[3];
      trianglePoints[0] = new Vector3(0, 1.3f);
      trianglePoints[1] = new Vector2(1.15f, -0.7f);
      trianglePoints[2] = new Vector2(-1.15f, -0.7f);
   }

   /// <summary>
   /// Draws a line from the inpoint to the outpoint
   /// </summary>
   public void Draw()
   {
      Handles.DrawLine(inPoint.Rectangle.center, outPoint.Rectangle.center);

      Vector2 position = (inPoint.Rectangle.center + outPoint.Rectangle.center) * 0.5f;
      float rotation = Vector2.SignedAngle(-trianglePoints[0], inPoint.Rectangle.center - outPoint.Rectangle.center);
      DrawTriangle(position, 10.0f, rotation);

      if (Handles.Button(position, Quaternion.identity, 4, 8, Handles.CircleHandleCap))
      {
         if(OnClickedRemoveConnection != null)
         {
            OnClickedRemoveConnection(this);
         }
      }
   }

   private void DrawTriangle(Vector2 position, float size, float rotationAngle)
   {
      float sin = Mathf.Sin(rotationAngle * Mathf.Deg2Rad);
      float cos = Mathf.Cos(rotationAngle * Mathf.Deg2Rad);

      Vector2[] rotatedPoints = new Vector2[3];
      
      rotatedPoints[0] = (new Vector2(trianglePoints[0].x * cos - trianglePoints[0].y * sin, trianglePoints[0].x * sin + trianglePoints[0].y * cos) * size) + position;
      rotatedPoints[1] = (new Vector2(trianglePoints[1].x * cos - trianglePoints[1].y * sin, trianglePoints[1].x * sin + trianglePoints[1].y * cos) * size) + position;
      rotatedPoints[2] = (new Vector2(trianglePoints[2].x * cos - trianglePoints[2].y * sin, trianglePoints[2].x * sin + trianglePoints[2].y * cos) * size) + position;

      Handles.DrawLine(rotatedPoints[0], rotatedPoints[1]);
      Handles.DrawLine(rotatedPoints[1], rotatedPoints[2]);
      Handles.DrawLine(rotatedPoints[2], rotatedPoints[0]);
   }
}