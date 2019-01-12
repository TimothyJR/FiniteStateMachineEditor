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

   public Node InPoint
   { get { return inPoint; } }

   public Node OutPoint
   { get { return outPoint; } }

   public Connection(Node inPoint, Node outPoint, Action<Connection> OnClickedRemoveConnection)
   {
      this.inPoint = inPoint;
      this.outPoint = outPoint;
      this.OnClickedRemoveConnection = OnClickedRemoveConnection;
   }

   public void Draw()
   {
      //Handles.DrawBezier(inPoint.Rectangle.center, outPoint.Rectangle.center, inPoint.Rectangle.center + Vector2.left * 50f, outPoint.Rectangle.center - Vector2.left * 50f, Color.white, null, 2.0f);
      Handles.DrawLine(inPoint.Rectangle.center, outPoint.Rectangle.center);
      if(Handles.Button((inPoint.Rectangle.center + outPoint.Rectangle.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
      {
         if(OnClickedRemoveConnection != null)
         {
            OnClickedRemoveConnection(this);
         }
      }
   }
}
