using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component modifies the attached <b>Rigidbody</b> component's drag value based on nearby <b>SgtDragSource</b> components in the scene.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Rigidbody))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDragReceiver")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Drag Receiver")]
	public class SgtDragReceiver : MonoBehaviour
	{
		/// <summary>The attached <b>Rigidbody</b> will be given this drag by default.</summary>
		public float Drag { set { drag = value; } get { return drag; } } [SerializeField] private float drag = 5.0f;

		/// <summary>The allows you to choose which <b>SgtDragSource</b> layers are used.</summary>
		public LayerMask Layers { set { layers = value; } get { return layers; } } [SerializeField] private LayerMask layers = -1;

		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		protected virtual void OnEnable()
		{
			cachedRigidbody = GetComponent<Rigidbody>();
			
		}

		protected virtual void FixedUpdate()
		{
			cachedRigidbody.drag = SgtDragSource.GetDrag(transform.position) + drag;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtDragReceiver))]
	public class SgtDragReceiver_Editor : SgtEditor<SgtDragReceiver>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Drag <= 0.0f));
				Draw("drag", "The drag applied to the drag receivers.");
			EndError();
			BeginError(Any(t => t.Layers == 0));
				Draw("layers", "The allows you to choose which <b>SgtDragSource</b> layers are used.");
			EndError();
		}
	}
}
#endif