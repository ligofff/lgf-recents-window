using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class RecentsWindow : EditorWindow
    {
        private List<Object> _objects;

        [MenuItem("Window/Recents")]
        public static void ShowWindow()
        {
            GetWindow<RecentsWindow>("Recents");
        }

        private void OnEnable()
        {
            _objects = new List<Object>();
            RecentsHandler.OnRecentsChanged += OnRecentsChanged;
            Repaint();
        }

        private void OnRecentsChanged()
        {
            _objects = RecentsHandler.GetObjects();
            this.Repaint();
        }

        private void OnGUI()
        {
            var favoriteObjectGUIStyle = new GUIStyle(EditorStyles.objectField);
            favoriteObjectGUIStyle.normal.textColor = Color.yellow;
            
            foreach (var obj in _objects)
            {
                if (obj == null) continue;

                EditorGUILayout.BeginHorizontal();
                
                var controlRect = EditorGUILayout.GetControlRect();
                var rect = controlRect;
                rect.size = new Vector2(controlRect.size.y, controlRect.size.y);
                if (GUI.Button(rect, "*"))
                {
                    
                }

                var rect2 = controlRect;
                rect2.position += new Vector2(rect.size.x + 5, 0f);
                rect2.size -= new Vector2(rect.size.x + 10, 0f);
                /*
                if (GUI.Button(rect2, new GUIContent(obj.name, AssetPreview.GetMiniTypeThumbnail(obj.GetType())),
                        objectGUIStyle))
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
                */

                GUI.enabled = false;
                EditorGUI.ObjectField(rect2, obj, typeof(Object), false);
                GUI.enabled = true;


                EditorGUILayout.EndHorizontal();
            }
        }
    }
}