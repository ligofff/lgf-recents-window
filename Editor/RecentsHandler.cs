using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor
{
    public static class RecentsHandler
    {
        public static int MaxRecents = 20;
        public static int MaxFavorites = 20;

        private static string PrefsIdRecents = "RecentsWindowData";
        private static string PrefsIdFavorites = "RecentsWindowData_Favorites";

        private static List<string> IdsBuffer;
        private static List<string> IdsBuffer_Favorites;
        
        public static event Action OnChanged;
        
        [InitializeOnLoadMethod]
        public static void RecentsHandler_Init()
        {
            if (!EditorPrefs.HasKey(PrefsIdRecents))
                EditorPrefs.SetString(PrefsIdRecents, "");
            
            if (!EditorPrefs.HasKey(PrefsIdFavorites))
                EditorPrefs.SetString(PrefsIdFavorites, "");
            
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
            
            OnChanged?.Invoke();
        }

        public static void AddObjectToFavorites(Object obj)
        {
            if (obj == null) return;
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(obj)) return;
            if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj))) return;
            
            var objectId = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            
            PullRecentsBuffer();
            
            if (IdsBuffer_Favorites.Contains(objectId))
                IdsBuffer_Favorites.Remove(objectId);

            IdsBuffer_Favorites.Insert(0, objectId);
            IdsBuffer_Favorites = IdsBuffer_Favorites.Take(MaxFavorites).ToList();
            
            PushRecentsBuffer();
            
            OnChanged?.Invoke();
        }
        
        public static void RemoveObjectFromFavorites(Object obj)
        {
            if (obj == null) return;
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(obj)) return;
            if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj))) return;
            
            var objectId = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            
            PullRecentsBuffer();
            
            if (IdsBuffer_Favorites.Contains(objectId))
                IdsBuffer_Favorites.Remove(objectId);
            
            PushRecentsBuffer();
            
            OnChanged?.Invoke();
        }

        public static void ClearAllRecents()
        {
            PullRecentsBuffer();

            IdsBuffer.Clear();
            
            PushRecentsBuffer();
            
            OnChanged?.Invoke();
        }

        private static void PushRecentsBuffer()
        {
            EditorPrefs.SetString(PrefsIdRecents, string.Join(";", IdsBuffer));
            EditorPrefs.SetString(PrefsIdFavorites, string.Join(";", IdsBuffer_Favorites));
        }

        private static void PullRecentsBuffer()
        {
            IdsBuffer = EditorPrefs.GetString(PrefsIdRecents).Split(';').ToList();
            IdsBuffer_Favorites = EditorPrefs.GetString(PrefsIdFavorites).Split(';').ToList();
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

        public static List<Object> GetObjects_Favorites()
        {
            var objects = new List<Object>();
            foreach (var id in IdsBuffer_Favorites)
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