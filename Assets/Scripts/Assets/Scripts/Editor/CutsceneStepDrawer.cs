using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CutsceneStep))]
public class CutsceneStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Get properties
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        SerializedProperty conversationProp = property.FindPropertyRelative("conversation");
        SerializedProperty advanceModeProp = property.FindPropertyRelative("advanceMode");
        SerializedProperty cameraTargetProp = property.FindPropertyRelative("cameraTarget");
        SerializedProperty cgImageProp = property.FindPropertyRelative("cgImage");
        SerializedProperty durationProp = property.FindPropertyRelative("duration");
        SerializedProperty customEventProp = property.FindPropertyRelative("customEvent");

        // Calculate heights
        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        Rect lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Draw the Foldout and Type
        property.isExpanded = EditorGUI.Foldout(new Rect(lineRect.x, lineRect.y, 10, lineRect.height), property.isExpanded, GUIContent.none);
        EditorGUI.PropertyField(new Rect(lineRect.x + 15, lineRect.y, lineRect.width - 15, lineRect.height), typeProp, new GUIContent("Step Type: " + typeProp.enumDisplayNames[typeProp.enumValueIndex]));

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            lineRect.y += lineHeight;

            CutsceneStepType type = (CutsceneStepType)typeProp.enumValueIndex;

            switch (type)
            {
                case CutsceneStepType.Dialogue:
                    EditorGUI.PropertyField(lineRect, conversationProp);
                    lineRect.y += lineHeight;
                    EditorGUI.PropertyField(lineRect, advanceModeProp);
                    lineRect.y += lineHeight;
                    EditorGUI.PropertyField(lineRect, cgImageProp, new GUIContent("Optional CG Image"));
                    break;

                case CutsceneStepType.CameraFocus:
                    EditorGUI.PropertyField(lineRect, cameraTargetProp);
                    lineRect.y += lineHeight;
                    EditorGUI.PropertyField(lineRect, durationProp);
                    break;

                case CutsceneStepType.ShowCG:
                    EditorGUI.PropertyField(lineRect, cgImageProp);
                    lineRect.y += lineHeight;
                    EditorGUI.PropertyField(lineRect, durationProp);
                    break;

                case CutsceneStepType.HideCG:
                case CutsceneStepType.FadeIn:
                case CutsceneStepType.FadeOut:
                case CutsceneStepType.Wait:
                    EditorGUI.PropertyField(lineRect, durationProp);
                    break;

                case CutsceneStepType.UnityEvent:
                    EditorGUI.PropertyField(lineRect, customEventProp);
                    break;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

        SerializedProperty typeProp = property.FindPropertyRelative("type");
        float baseHeight = EditorGUIUtility.singleLineHeight + 4;
        float fieldHeight = EditorGUIUtility.singleLineHeight + 2;

        CutsceneStepType type = (CutsceneStepType)typeProp.enumValueIndex;

        switch (type)
        {
            case CutsceneStepType.Dialogue:
                return baseHeight + (fieldHeight * 3);
            case CutsceneStepType.CameraFocus:
            case CutsceneStepType.ShowCG:
                return baseHeight + (fieldHeight * 2);
            case CutsceneStepType.UnityEvent:
                return baseHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("customEvent"));
            default:
                return baseHeight + fieldHeight;
        }
    }
}
