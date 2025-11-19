using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public static class HierarchyColorizer
{
    static HierarchyColorizer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        // Solo si el nombre contiene ===
        if (obj.name.Contains("==="))
        {
            // Fondo sutil
            EditorGUI.DrawRect(selectionRect, new Color(1f, 0f, 0f, 0.25f));

            // Evitar el texto original: desplazamos el label fuera de vista
            selectionRect.x += 18f; // opcional: dejar espacio por si hay ícono

            // Dibujar texto custom centrado
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0f, 0f) }
            };

            EditorGUI.LabelField(selectionRect, "", style);
        }
    }
}
#endif