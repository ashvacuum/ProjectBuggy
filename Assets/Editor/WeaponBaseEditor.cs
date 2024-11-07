using NonECS.BaseWeapons;

using UnityEditor;
using UnityEngine;


namespace Editor
{
    [CustomEditor(typeof(WeaponBase), true)]
    public class WeaponBaseEditor : UnityEditor.Editor
    {
        private SerializedProperty upgradeDataList;
        
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

            for (int i = 0; i < upgradeDataList.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                SerializedProperty data = upgradeDataList.GetArrayElementAtIndex(i);
                SerializedProperty UpgradeType = data.FindPropertyRelative("UpgradeType");
                SerializedProperty Lifetime = data.FindPropertyRelative("Lifetime");
                SerializedProperty Count = data.FindPropertyRelative("Count");
                SerializedProperty Penetration = data.FindPropertyRelative("Penetration");
                SerializedProperty Speed = data.FindPropertyRelative("Speed");
                SerializedProperty FireRate = data.FindPropertyRelative("FireRate");
                SerializedProperty Size = data.FindPropertyRelative("WeaponSize");
                SerializedProperty Damage = data.FindPropertyRelative("Damage");
                SerializedProperty Knockback = data.FindPropertyRelative("Knockback");
                SerializedProperty Range = data.FindPropertyRelative("Range");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(UpgradeType, new GUIContent($"{i}"));
                
                if (GUILayout.Button("-", GUILayout.Width(60)))
                {
                    data.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                WeaponUpgradeType currentTypes = (WeaponUpgradeType)UpgradeType.intValue;
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Lifetime))
                {
                    EditorGUILayout.PropertyField(Lifetime);
                }
            
                if (currentTypes.HasFlag(WeaponUpgradeType.Count))
                {
                    EditorGUILayout.PropertyField(Count);
                }
            
                if (currentTypes.HasFlag(WeaponUpgradeType.Penetration))
                {
                    EditorGUILayout.PropertyField(Penetration);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Speed))
                {
                    EditorGUILayout.PropertyField(Speed);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.FireRate))
                {
                    EditorGUILayout.PropertyField(FireRate);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Size))
                {
                    EditorGUILayout.PropertyField(Size);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Damage))
                {
                    EditorGUILayout.PropertyField(Damage);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Knockback))
                {
                    EditorGUILayout.PropertyField(Knockback);
                }
                
                if (currentTypes.HasFlag(WeaponUpgradeType.Range))
                {
                    EditorGUILayout.PropertyField(Range);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
