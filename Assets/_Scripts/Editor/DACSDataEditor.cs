// using UnityEditor;
// using UnityEditorInternal;
// using UnityEngine;

// [CustomEditor(typeof(DACS_P_ScriptableObject))]
// public class DACSDataEditor : Editor
// {
//     private ReorderableList reorderableList;
//     private SerializedProperty DACSDataList;
//     private bool[] foldouts;

//     private void OnEnable()
//     {
//         DACSDataList = serializedObject.FindProperty("P_ScriptableObject");
//         foldouts = new bool[DACSDataList.arraySize];

//         reorderableList = new ReorderableList(serializedObject, DACSDataList, true, true, true, true)
//         {
//             drawElementCallback = (rect, index, active, focused) =>
//             {
//                 var elementProperty = DACSDataList.GetArrayElementAtIndex(index);
//                 var elementRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

//                 foldouts[index] = EditorGUI.Foldout(elementRect, foldouts[index], "ID : " + index);

//                 if (foldouts[index])
//                 {
//                     EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUI.GetPropertyHeight(elementProperty)), elementProperty);
//                 }
//             },
//             drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "DACS DataList"),
//             elementHeightCallback = (index) =>
//             {
//                 return foldouts[index] ? EditorGUI.GetPropertyHeight(DACSDataList.GetArrayElementAtIndex(index)) + EditorGUIUtility.standardVerticalSpacing * 2 + EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
//             },
//             onAddCallback = (list) =>
//             {
//                 DACSDataList.arraySize++;
//                 foldouts = new bool[DACSDataList.arraySize]; // 新しい要素のためにfoldoutsを拡張する
//                 serializedObject.ApplyModifiedProperties();
//             }
//         };
//     }

//     public override void OnInspectorGUI()
//     {
//         serializedObject.Update();
//         reorderableList.DoLayoutList();
//         serializedObject.ApplyModifiedProperties();
//     }
// }