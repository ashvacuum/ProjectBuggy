using NonECS.BaseWeapons;

using UnityEditor;
using UnityEngine;


namespace Editor
{
    [CustomEditor(typeof(WeaponBase), true)]
    public class WeaponBaseEditor : UnityEditor.Editor
    {
        private SerializedProperty upgradeDataList;
        private int elementToRemove = -1;
        
        protected virtual void OnEnable()
        {
            upgradeDataList = serializedObject.FindProperty("upgradeData");
        }

        protected virtual void DrawCustomFields()
        {
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("+"))
            {
                upgradeDataList.arraySize++;
            }

            elementToRemove = -1;

            for (var i = 0; i < upgradeDataList.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                var data = upgradeDataList.GetArrayElementAtIndex(i);
                var upgradeType = data.FindPropertyRelative("UpgradeType");
                var lifetime = data.FindPropertyRelative("Lifetime");
                var count = data.FindPropertyRelative("Count");
                var penetration = data.FindPropertyRelative("Penetration");
                var speed = data.FindPropertyRelative("Speed");
                var fireRate = data.FindPropertyRelative("FireRate");
                var size = data.FindPropertyRelative("WeaponSize");
                var damage = data.FindPropertyRelative("Damage");
                var knockBack = data.FindPropertyRelative("Knockback");
                var range = data.FindPropertyRelative("Range");
                var critical = data.FindPropertyRelative("Critical");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(upgradeType, new GUIContent($"{i}"));
                
                if (GUILayout.Button("-", GUILayout.Width(60)))
                {
                    elementToRemove = i;
                }
                EditorGUILayout.EndHorizontal();
                
                var currentTypes = (WeaponUpgradeType)upgradeType.intValue;
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Lifetime))
                {
                    EditorGUILayout.PropertyField(lifetime);
                }
            
                if (currentTypes.HasFlag(WeaponUpgradeType.Count))
                {
                    EditorGUILayout.PropertyField(count);
                }
            
                if (currentTypes.HasFlag(WeaponUpgradeType.Penetration))
                {
                    EditorGUILayout.PropertyField(penetration);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Speed))
                {
                    EditorGUILayout.PropertyField(speed);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.FireRate))
                {
                    EditorGUILayout.PropertyField(fireRate);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Size))
                {
                    EditorGUILayout.PropertyField(size);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Damage))
                {
                    EditorGUILayout.PropertyField(damage);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Knockback))
                {
                    EditorGUILayout.PropertyField(knockBack);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Range))
                {
                    EditorGUILayout.PropertyField(range);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Critical))
                {
                    EditorGUILayout.PropertyField(critical);
                }   

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

            }

            if (elementToRemove != -1)
            {
                upgradeDataList.DeleteArrayElementAtIndex(elementToRemove);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
