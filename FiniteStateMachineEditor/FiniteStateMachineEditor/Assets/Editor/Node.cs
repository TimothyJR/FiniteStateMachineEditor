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

   public Action<Node> OnRemoveNode;
   public Action<Node> OnCreateConnection;
   public Action<Node> Clicked;

   public Rect Rectangle
   { get { return rectangle; } }


   public Node(Vector2 position, float width, float height, GUIStyle defaultStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<Node> OnClickRemoveNode, Action<Node> OnClickCreateConnection, Action<Node> OnClicked)
   {
      rectangle = new Rect(position.x, position.y, width, height);
      nodeStyle = defaultStyle;
      this.defaultStyle = defaultStyle;
      this.selectedStyle = selectedStyle;

      OnRemoveNode = OnClickRemoveNode;
      OnCreateConnection = OnClickCreateConnection;
      Clicked = OnClicked;
   }

   public void DragNode(Vector2 delta)
   {
      rectangle.position += delta;
   }

   public void Draw()
   {
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
                  if(Clicked != null)
                  {
                     Clicked(this);
                  }
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
      genericMenu.AddItem(new GUIContent("Create connection"), false, OnClickCreateConnection);
      genericMenu.ShowAsContext();
   }

   private void OnClickRemoveNode()
   {
      if(OnRemoveNode != null)
      {
         OnRemoveNode(this);
      }

   }

   private void OnClickCreateConnection()
   {
      if(OnCreateConnection != null)
      {
         OnCreateConnection(this);
      }
   }
}
