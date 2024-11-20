using NonECS.BaseWeapons;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(ProjectileWeaponBase))]
    public class ProjectileEditor : WeaponBaseEditor
    {
        SerializedProperty yourSpecificField;

        protected override void OnEnable()
        {
            base.OnEnable();
            yourSpecificField = serializedObject.FindProperty("ProjectilePrefab");
        }

        protected override void DrawCustomFields()
        {
            EditorGUILayout.PropertyField(yourSpecificField);
        }
    }
}
