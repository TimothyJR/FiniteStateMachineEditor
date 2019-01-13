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
   Vector2[] currentTrianglePoints;

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

      currentTrianglePoints = new Vector2[3];
      UpdateTriangle();
   }

   /// <summary>
   /// Draws a line from the inpoint to the outpoint
   /// </summary>
   public void Draw()
   {
      Handles.DrawLine(inPoint.Rectangle.center, outPoint.Rectangle.center);

      Handles.DrawLine(currentTrianglePoints[0], currentTrianglePoints[1]);
      Handles.DrawLine(currentTrianglePoints[1], currentTrianglePoints[2]);
      Handles.DrawLine(currentTrianglePoints[2], currentTrianglePoints[0]);

      if (Handles.Button((inPoint.Rectangle.center + outPoint.Rectangle.center) * 0.5f, Quaternion.identity, 4, 8, Handles.CircleHandleCap))
      {
         if(OnClickedRemoveConnection != null)
         {
            OnClickedRemoveConnection(this);
         }
      }
   }

   /// <summary>
   /// Updates the triangle. Primarily used to initialize the triangle.
   /// </summary>
   public void UpdateTriangle()
   {
         float rotation = Vector2.SignedAngle(-trianglePoints[0], inPoint.Rectangle.center - outPoint.Rectangle.center);
         CalculateTriangle((inPoint.Rectangle.center + outPoint.Rectangle.center) * 0.5f, 10.0f, rotation);
   }

   /// <summary>
   /// Updates the triangle. Primarily used when the editor has changes.
   /// </summary>
   public void UpdateTriangle(Node node)
   {
      if(node == inPoint || node == outPoint)
      {
         float rotation = Vector2.SignedAngle(-trianglePoints[0], inPoint.Rectangle.center - outPoint.Rectangle.center);
         CalculateTriangle((inPoint.Rectangle.center + outPoint.Rectangle.center) * 0.5f, 10.0f, rotation);
      }
   }

   /// <summary>
   /// Draws a triangle pointing towards the outPoint node
   /// </summary>
   /// <param name="position"></param>
   /// <param name="size"></param>
   /// <param name="rotationAngle"></param>
   private void CalculateTriangle(Vector2 position, float size, float rotationAngle)
   {
      float sin = Mathf.Sin(rotationAngle * Mathf.Deg2Rad);
      float cos = Mathf.Cos(rotationAngle * Mathf.Deg2Rad);


      currentTrianglePoints[0] = (new Vector2(trianglePoints[0].x * cos - trianglePoints[0].y * sin, trianglePoints[0].x * sin + trianglePoints[0].y * cos) * size) + position;
      currentTrianglePoints[1] = (new Vector2(trianglePoints[1].x * cos - trianglePoints[1].y * sin, trianglePoints[1].x * sin + trianglePoints[1].y * cos) * size) + position;
      currentTrianglePoints[2] = (new Vector2(trianglePoints[2].x * cos - trianglePoints[2].y * sin, trianglePoints[2].x * sin + trianglePoints[2].y * cos) * size) + position;


   }
}