using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Flyfe.Editor
{
    public class FindMissingScripts : EditorWindow
    {
        [MenuItem("Flyfe/Find Objects With Missing Scripts")]
        public static void FindMissing()
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;

            foreach (GameObject go in allObjects)
            {
                Component[] components = go.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c == null)
                    {
                        Debug.LogError("Missing Script found on GameObject: " + go.name, go);
                        count++;
                        break; 
                    }
                }
            }

            Debug.Log($"<color=cyan><b>Search Complete:</b></color> Found {count} objects with missing scripts.");
        }
    }
}
