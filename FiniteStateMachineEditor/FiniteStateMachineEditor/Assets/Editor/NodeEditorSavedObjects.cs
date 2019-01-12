using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

/// <summary>
/// This is a scriptable object for saving the data of the node editor.
/// </summary>
[CreateAssetMenu(menuName = "Test/NodeEditorSave")]
public class NodeEditorSavedObjects : ScriptableObject
{
   [SerializeField] private List<Node> nodes;
   [SerializeField] private List<Connection> connections;

   public List<Node> Nodes
   {
      get { return nodes; }
      set { nodes = value; }
   }

   public List<Connection> Connections
   {
      get { return connections; }
      set { connections = value; }
   }

   /// <summary>
   /// Opens up the node editor when you open the scriptable object
   /// </summary>
   /// <param name="instanceID"></param>
   /// <param name="line"></param>
   /// <returns></returns>
   [OnOpenAsset]
   public static bool PullUpNodeEditor(int instanceID, int line)
   {
      NodeEditorSavedObjects editor = EditorUtility.InstanceIDToObject(instanceID) as NodeEditorSavedObjects;
      if(editor != null)
      {
         NodeEditor.OpenWindow((NodeEditorSavedObjects)EditorUtility.InstanceIDToObject(instanceID));
         return true;
      }

      return false;
   }

   
   
}
