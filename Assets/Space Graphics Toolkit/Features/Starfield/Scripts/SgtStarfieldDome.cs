using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a starfield with a dome like distribution.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtStarfieldDome")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Starfield Dome")]
	public class SgtStarfieldDome : SgtStarfield
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMaterial(); } } get { return seed; } } [SerializeField] [SgtSeed] private int seed;

		/// <summary>The radius of the starfield.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMaterial(); } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>This allows you to control if the stars should cluster near the horizon, or near the azimuth.</summary>
		public float Bias { set { if (bias != value) { bias = value; DirtyMaterial(); } } get { return bias; } } [SerializeField] private float bias = 1.0f;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount { set { if (starCount != value) { starCount = value; DirtyMaterial(); } } get { return starCount; } } [SerializeField] private int starCount = 1000;

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors { get { if (starColors == null) starColors = new Gradient(); return starColors; } } [SerializeField] private Gradient starColors;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin { set { if (starRadiusMin != value) { starRadiusMin = value; DirtyMaterial(); } } get { return starRadiusMin; } } [SerializeField] private float starRadiusMin = 0.01f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax { set { if (starRadiusMax != value) { starRadiusMax = value; DirtyMaterial(); } } get { return starRadiusMax; } } [SerializeField] private float starRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias { set { if (starRadiusBias != value) { starRadiusBias = value; DirtyMaterial(); } } get { return starRadiusBias; } } [SerializeField] private float starRadiusBias = 1.0f;

		/// <summary>The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.</summary>
		public float StarPulseMax { set { if (starPulseMax != value) { starPulseMax = value; DirtyMaterial(); } } get { return starPulseMax; } } [SerializeField] [Range(0.0f, 1.0f)] private float starPulseMax = 1.0f;

		public static SgtStarfieldDome Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldDome Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Starfield Dome", layer, parent, localPosition, localRotation, localScale);
			var starfield  = gameObject.AddComponent<SgtStarfieldDome>();

			return starfield;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Starfield/Dome", false, 10)]
		private static void CreateMenuItem()
		{
			var parent    = SgtHelper.GetSelectedParent();
			var starfield = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(starfield);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, radius);
		}
#endif

		protected override int BeginQuads()
		{
			SgtHelper.BeginRandomSeed(seed);

			if (starColors == null)
			{
				starColors = SgtHelper.CreateGradient(Color.white);
			}

			return starCount;
		}

		protected override void NextQuad(ref SgtStarfieldStar star, int starIndex)
		{
			var sphere = Random.insideUnitSphere;

			sphere.y = Mathf.Abs(sphere.y) * bias;

			star.Variant     = Random.Range(int.MinValue, int.MaxValue);
			star.Color       = starColors.Evaluate(Random.value);
			star.Radius      = Mathf.Lerp(starRadiusMin, starRadiusMax, SgtHelper.Sharpness(Random.value, starRadiusBias));
			star.Angle       = Random.Range(-180.0f, 180.0f);
			star.Position    = sphere.normalized * radius;
			star.PulseRange  = Random.value * starPulseMax;
			star.PulseSpeed  = Random.value;
			star.PulseOffset = Random.value;
		}

		protected override void EndQuads()
		{
			SgtHelper.EndRandomSeed();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtStarfieldDome))]
	public class SgtStarfieldDome_Editor : SgtStarfield_Editor<SgtStarfieldDome>
	{
		protected override void OnInspector()
		{
			var dirtyMaterial        = false;
			var dirtyMesh = false;

			DrawMaterial(ref dirtyMaterial);

			Separator();

			DrawMainTex(ref dirtyMaterial);
			DrawLayout(ref dirtyMesh);

			Separator();

			DrawPointMaterial(ref dirtyMaterial);

			Separator();

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the starfield.");
			EndError();
			Draw("bias", ref dirtyMesh, "This allows you to control if the stars should cluster near the horizon, or near the azimuth.");

			Separator();

			BeginError(Any(t => t.StarCount < 0));
				Draw("starCount", ref dirtyMesh, "The amount of stars that will be generated in the starfield.");
			EndError();
			Draw("starColors", ref dirtyMesh, "Each star is given a random color from this gradient.");
			BeginError(Any(t => t.StarRadiusMin < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				Draw("starRadiusMin", ref dirtyMesh, "The minimum radius of stars in the starfield.");
			EndError();
			BeginError(Any(t => t.StarRadiusMax < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				Draw("starRadiusMax", ref dirtyMesh, "The maximum radius of stars in the starfield.");
			EndError();
			Draw("starRadiusBias", ref dirtyMesh, "How likely the size picking will pick smaller stars over larger ones (1 = default/linear).");
			Draw("starPulseMax", ref dirtyMesh, "The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.");

			RequireCamera();

			serializedObject.ApplyModifiedProperties();

			if (dirtyMaterial == true) DirtyEach(t => t.DirtyMaterial());
			if (dirtyMesh     == true) DirtyEach(t => t.DirtyMesh    ());
		}
	}
}
#endif