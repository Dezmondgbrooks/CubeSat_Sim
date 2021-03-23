using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to define a sphere shape that can be used by other components to perform actions confined to the volume.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShapeSphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shape Sphere")]
	public class SgtShapeSphere : SgtShape
	{
		/// <summary>The radius of this sphere in local space.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [FSA("Radius")] [SerializeField] private float radius = 1.0f;

		/// <summary>If this is set, then any point within <b>Radius</b> will have maximum density, and it will fall off to zero within the <b>height</b> range.</summary>
		public float Height { set { height = value; } get { return height; } } [SerializeField] private float height;

		/// <summary>The transition style between minimum and maximum density.</summary>
		public SgtEase.Type Ease { set { ease = value; } get { return ease; } } [FSA("Ease")] [SerializeField] private SgtEase.Type ease = SgtEase.Type.Smoothstep;

		/// <summary>How quickly the density increases when inside the sphere.</summary>
		public float Sharpness { set { sharpness = value; } get { return sharpness; } } [FSA("Sharpness")] [SerializeField] private float sharpness = 1.0f;

		public float RadiusInner
		{
			get
			{
				return height == 0.0f ? 0.0f : radius;
			}
		}

		public float RadiusOuter
		{
			get
			{
				return radius + height;
			}
		}

		public override float GetDensity(Vector3 worldPoint)
		{
			var distance   = transform.InverseTransformPoint(worldPoint).magnitude;
			var distance01 = Mathf.InverseLerp(RadiusOuter, RadiusInner, distance);

			return SgtHelper.Sharpness(SgtEase.Evaluate(ease, distance01), sharpness);
		}

		public static SgtShapeSphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeSphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject  = SgtHelper.CreateGameObject("Shape Sphere", layer, parent, localPosition, localRotation, localScale);
			var shapeSphere = gameObject.AddComponent<SgtShapeSphere>();

			return shapeSphere;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Shape/Sphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent      = SgtHelper.GetSelectedParent();
			var shapeSphere = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(shapeSphere);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color  = new Color(1.0f, 1.0f, 1.0f, 0.25f);

			Gizmos.DrawWireSphere(Vector3.zero, RadiusInner);
			Gizmos.DrawWireSphere(Vector3.zero, RadiusOuter);

			for (var i = 0; i < 11; i++)
			{
				var distance   = Mathf.Lerp(RadiusInner, RadiusOuter, i * 0.1f);
				var worldPoint = transform.TransformPoint(0.0f, 0.0f, distance);

				Gizmos.color = new Color(1.0f, 1.0f, 1.0f, GetDensity(worldPoint));

				Gizmos.DrawWireSphere(Vector3.zero, distance);
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShapeSphere))]
	public class SgtShapeSphere_Editor : SgtEditor<SgtShapeSphere>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Radius <= 0.0f));
				Draw("radius", "The radius of this sphere in local coordinates.");
			EndError();
			Draw("height", "If this is set, then any point within Radius will have maximum density, and it will fall off to zero within the height range.");
			Draw("ease", "The transition style between minimum and maximum density.");
			Draw("sharpness", "How quickly the density increases when inside the sphere.");
		}
	}
}
#endif