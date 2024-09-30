using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class RecentsWindow : EditorWindow
    {
        private class RecentObjectData
        {
            public Object obj;
            public bool favorite;
        }
        
        private List<Object> _objects;
        private List<Object> _objects_favorites;
        private List<RecentObjectData> _sortedBuffer;

        private GUIStyle _favoriteObjectGUIStyle;

        [MenuItem("Ligofff/Recents Window")]
        public static void ShowWindow()
        {
            GetWindow<RecentsWindow>("Recents");
        }

        private void OnEnable()
        {
            _objects = new List<Object>();
            _objects_favorites = new List<Object>();
            RecentsHandler.OnChanged += OnChanged;
            OnChanged();
            Repaint();
        }

        private void OnChanged()
        {
            _objects = RecentsHandler.GetObjects();
            _objects_favorites = RecentsHandler.GetObjects_Favorites();
            _sortedBuffer = _objects
                .Concat(_objects_favorites)
                .Distinct()
                .OrderByDescending(obj => _objects_favorites.Contains(obj))
                .Select(obj => new RecentObjectData(){obj = obj, favorite = _objects_favorites.Contains(obj)})
                .ToList();
            Repaint();
        }

        private void OnGUI()
        {
            if (_favoriteObjectGUIStyle == null)
            {
                _favoriteObjectGUIStyle = new GUIStyle(EditorStyles.objectField);
                _favoriteObjectGUIStyle.normal.textColor = Color.yellow;
            }
            
            DrawHeader();

            if (_sortedBuffer.Count == 0)
            {
                EditorGUILayout.LabelField("No recents found.");
            }
            
            foreach (var obj in _sortedBuffer)
            {
                DrawObject(obj);
            }
        }

        private void DrawHeader()
        {
            var rect = EditorGUILayout.GetControlRect();
            var buttonRect = rect;
            buttonRect.size = new Vector2(120, buttonRect.size.y);
            buttonRect.position += new Vector2(rect.size.x - 120, 0f);

            if (GUI.Button(buttonRect, "Clear"))
            {
                RecentsHandler.ClearAllRecents();
            }
        }

        private void DrawObject(RecentObjectData data)
        {
            if (data.obj == null) return;

            EditorGUILayout.BeginHorizontal();
                
            var controlRect = EditorGUILayout.GetControlRect();
            var rect = controlRect;
            rect.size = new Vector2(controlRect.size.y, controlRect.size.y);

            if (data.favorite)
            {
                GUI.backgroundColor = Color.yellow;
                if (GUI.Button(rect, "*"))
                {
                    RecentsHandler.RemoveObjectFromFavorites(data.obj);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                if (GUI.Button(rect, "*"))
                {
                    RecentsHandler.AddObjectToFavorites(data.obj);
                }
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

            if (data.favorite)
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUI.ObjectField(rect2, data.obj, typeof(Object), false);
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUI.ObjectField(rect2, data.obj, typeof(Object), false);
            }
            
            GUI.enabled = true;


            EditorGUILayout.EndHorizontal(); 
        }
    }
}