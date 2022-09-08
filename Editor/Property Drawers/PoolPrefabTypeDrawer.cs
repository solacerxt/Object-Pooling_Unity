#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace solacerxt.Pooling
{
    [CustomPropertyDrawer(typeof(PoolPrefabTypeAttribute))]
    public class PoolPrefabTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (PoolPrefabTypeAttribute) attribute;
            var type = attr.Value;

            if (!IsTypeOf(type, typeof(Pool.Poolable)))
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.LabelField(position, "Invalid pool base type");
                Debug.LogError($"[PoolBaseType(typeof({type}))] Invalid pool base type: Type should be derived from Pool.Poolable");
                return;
            }

            var previous = property.objectReferenceValue;
            EditorGUI.ObjectField(position, property, typeof(Pool), label);

            if (property.objectReferenceValue is null) return;

            var poolType = ((Pool)property.objectReferenceValue).Type;

            if (IsTypeOf(poolType, type)) return;

            EditorUtility.DisplayDialog(
                title: $"Invalid pool type: {poolType}",
                message: $"Pool<{poolType}> is invalid. You can set only pool of type derived from {type}",
                ok: "OK"
            );

            if (previous is not null && IsTypeOf(previous.GetType(), type))
                property.objectReferenceValue = previous;
            else property.objectReferenceValue = null;
        }

        private bool IsTypeOf(Type a, Type b) => a == b || a.IsSubclassOf(b);
    }
}
#endif
