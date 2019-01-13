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
   public Action<Node> Changed;

   public Rect Rectangle
   { get { return rectangle; } }


   public Node(Vector2 position, float width, float height, GUIStyle defaultStyle, GUIStyle selectedStyle, Action<Node> OnClickRemoveNode, Action<Node> OnClickCreateConnection, Action<Node> OnClicked, Action<Node> OnChanged)
   {
      rectangle = new Rect(position.x, position.y, width, height);
      nodeStyle = defaultStyle;
      this.defaultStyle = defaultStyle;
      this.selectedStyle = selectedStyle;

      OnRemoveNode = OnClickRemoveNode;
      OnCreateConnection = OnClickCreateConnection;
      Clicked = OnClicked;
      Changed = OnChanged;
   }

   /// <summary>
   /// Moves the node if it is being dragged
   /// </summary>
   /// <param name="delta"></param>
   public void DragNode(Vector2 delta)
   {
      rectangle.position += delta;
   }

   /// <summary>
   /// Draws the node
   /// </summary>
   public void Draw()
   {
      GUI.Box(rectangle, title, nodeStyle);
   }

   /// <summary>
   /// Handles mouse input
   /// </summary>
   /// <param name="e"></param>
   /// <returns></returns>
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
            if(e.button == 1)
            {
               if(rectangle.Contains(e.mousePosition))
               {
                  GUI.changed = true;
                  isSelected = true;
                  nodeStyle = selectedStyle;
                  ProcessContextMenu();
                  e.Use();
               }
               else
               {
                  GUI.changed = true;
                  isSelected = false;
                  nodeStyle = defaultStyle;
               }
               
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
               OnNodeChange();
               return true;
            }
            break;
      

      }
      return false;
   }

   /// <summary>
   /// Creates context menu when right clicking on nodes
   /// </summary>
   private void ProcessContextMenu()
   {
      GenericMenu genericMenu = new GenericMenu();
      genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
      genericMenu.AddItem(new GUIContent("Create connection"), false, OnClickCreateConnection);
      genericMenu.ShowAsContext();
   }

   /// <summary>
   /// Calls actions for node removal
   /// </summary>
   private void OnClickRemoveNode()
   {
      if(OnRemoveNode != null)
      {
         OnRemoveNode(this);
      }

   }

   /// <summary>
   /// Calls actions for creating connections
   /// </summary>
   private void OnClickCreateConnection()
   {
      if(OnCreateConnection != null)
      {
         OnCreateConnection(this);
      }
   }

   /// <summary>
   /// Calls actions when the node is changed
   /// </summary>
   private void OnNodeChange()
   {
      if(Changed != null)
      {
         Changed(this);
      }
   }
}