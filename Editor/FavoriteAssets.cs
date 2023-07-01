using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class FavoriteAssets : EditorWindow
{
    List<Object> recentObjects = new List<Object>();
    int maxObjects = 5;
    ReorderableList reorderableList;
    Vector2 scrollPos;
    bool isStoringEnabled = false;
    Object itemToRemove = null;

    static FavoriteAssets()
    {
        EditorApplication.delayCall += () =>
        {
            GetWindow(typeof(FavoriteAssets));
        };
    }

    [MenuItem("Window/Favorite Assets")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FavoriteAssets));
    }

    void OnEnable()
    {
        reorderableList = new ReorderableList(recentObjects, typeof(Object), true, false, false, false);
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2;
            EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                recentObjects[index], typeof(Object), true);
            if (GUI.Button(new Rect(rect.x + rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), "Remove"))
            {
                itemToRemove = recentObjects[index];
            }
        };
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Recently selected or favorite assets", EditorStyles.boldLabel);

        int newMaxObjects = EditorGUILayout.DelayedIntField("Maximum number assets : ", maxObjects);
        if (newMaxObjects != maxObjects)
        {
            maxObjects = newMaxObjects;
            TrimList();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
        {
            recentObjects.Clear();
        }

        if (GUILayout.Button(isStoringEnabled ? "Stop Storing" : "Start Storing"))
        {
            isStoringEnabled = !isStoringEnabled;
        }
        GUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        reorderableList.DoLayoutList();
        EditorGUILayout.EndScrollView();
        
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));

        GUIStyle centeredStyle = GUI.skin.GetStyle("Box");
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Box(dropArea, "Drag & Drop here!");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (!recentObjects.Contains(draggedObject))
                        {
                            recentObjects.Add(draggedObject);
                        }
                        TrimList();
                    }
                }
                break;
        }

        if (itemToRemove != null)
        {
            recentObjects.Remove(itemToRemove);
            itemToRemove = null;
            Repaint();
        }
    }

    void TrimList()
    {
        while (recentObjects.Count > maxObjects)
        {
            recentObjects.RemoveAt(0);
        }
    }

    void OnSelectionChange()
    {
        if (!isStoringEnabled || Selection.activeObject == null)
        {
            return;
        }

        if (recentObjects.Contains(Selection.activeObject))
        {
            return;
        }

        if (recentObjects.Count >= maxObjects)
        {
            recentObjects.RemoveAt(0);
        }

        recentObjects.Add(Selection.activeObject);
        Repaint();
    }
}
