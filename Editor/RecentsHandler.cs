using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor
{
    public static class RecentsHandler
    {
        public static int MaxRecents = 10;

        private static string PrefsId = "RecentsWindowData";

        private static List<string> IdsBuffer;
        
        public static event Action OnRecentsChanged;
        
        [InitializeOnLoadMethod]
        public static void RecentsHandler_Init()
        {
            if (!EditorPrefs.HasKey(PrefsId))
                EditorPrefs.SetString(PrefsId, "");
            
            PullRecentsBuffer();

            Selection.selectionChanged += SelectionChanged;
        }

        private static void SelectionChanged()
        {
            AddObjectToRecents(Selection.activeObject);
        }

        public static void AddObjectToRecents(Object obj)
        {
            if (obj == null) return;
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(obj)) return;
            if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj))) return;

            var objectId = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            
            PullRecentsBuffer();
            
            if (IdsBuffer.Contains(objectId))
                IdsBuffer.Remove(objectId);

            IdsBuffer.Insert(0, objectId);
            IdsBuffer = IdsBuffer.Take(MaxRecents).ToList();
            
            PushRecentsBuffer();
            
            OnRecentsChanged?.Invoke();
        }

        private static void PushRecentsBuffer()
        {
            EditorPrefs.SetString(PrefsId, string.Join(";", IdsBuffer));
        }

        private static void PullRecentsBuffer()
        {
            IdsBuffer = EditorPrefs.GetString(PrefsId).Split(';').ToList();
        }

        public static List<Object> GetObjects()
        {
            var objects = new List<Object>();
            foreach (var id in IdsBuffer)
            {
                if (GlobalObjectId.TryParse(id, out var globalObjectId))
                {
                    objects.Add(GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId));
                }
            }

            return objects;
        }

        public static void Refresh()
        {
            PullRecentsBuffer();
        }
    }
}