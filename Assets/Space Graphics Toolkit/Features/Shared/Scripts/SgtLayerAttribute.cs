using UnityEngine;

namespace SpaceGraphicsToolkit
{
	public class SgtLayerAttribute : PropertyAttribute
	{
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CustomPropertyDrawer(typeof(SgtLayerAttribute))]
	public class SgtLayerAttribute_Drawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
				property.intValue = EditorGUI.LayerField(position, label, property.intValue);
			EditorGUI.EndProperty();
		}
	}
}
#endif