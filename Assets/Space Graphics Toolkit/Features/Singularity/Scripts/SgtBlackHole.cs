using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a singularity/black hole.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtBlackHole")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Black Hole")]
	public class SgtBlackHole : MonoBehaviour
	{
		/// <summary>The higher you set this, the smaller the spatial distortion will be.</summary>
		public float Pinch { set  { pinch = value; } get { return pinch; } } [SerializeField] private float pinch = 10.0f;

		/// <summary>The higher you set this, the more space will bend around the black hole.</summary>
		public float Warp { set  { warp = value; } get { return warp; } } [SerializeField] [Range(0.0f, 15.0f)] private float warp = 4.0f;

		/// <summary>This allows you to control the overall size of the hole relative to the pinch.</summary>
		public float HoleSize { set { holeSize = value; } get { return holeSize; } } [SerializeField] float holeSize = 0.5f;

		/// <summary>This allows you to control how sharp/abrupt the transition between space and the event horizon is.</summary>
		public float HoleSharpness { set { holeSharpness = value; } get { return holeSharpness; } } [SerializeField] float holeSharpness = 10.0f;

		/// <summary>This allows you to control the color of the black hole past the event horizon.</summary>
		public Color HoleColor { set { holeColor = value; } get { return holeColor; } } [SerializeField] Color holeColor = Color.black;

		/// <summary>The color of the tint.</summary>
		public Color TintColor { set { tintColor = value; } get { return tintColor; } } [SerializeField] Color tintColor = Color.red;

		/// <summary>This allows you to control how sharp/abrupt the transition between the event horizon and the tinted space is.</summary>
		public float TintSharpness { set { tintSharpness = value; } get { return tintSharpness; } } [SerializeField] float tintSharpness = 4.0f;

		/// <summary>This allows you to fade the edges of the black hole. This is useful if you have multiple black holes near each other.</summary>
		public float FadePower { set { fadePower = value; } get { return fadePower; } } [SerializeField] float fadePower = 10.0f;

		[System.NonSerialized]
		private Material generatedMaterial;

		public MeshFilter CachedMeshFilter { get { CacheMeshFilter(); return cachedMeshFilter; } }  [System.NonSerialized] private MeshFilter cachedMeshFilter;

		public MeshRenderer CachedMeshRenderer { get { CacheMeshRenderer(); return cachedMeshRenderer; } } [System.NonSerialized] private MeshRenderer cachedMeshRenderer;

		private void CacheMeshFilter()
		{
			if (cachedMeshFilter == null)
			{
				cachedMeshFilter = GetComponent<MeshFilter>();
			}
		}

		private void CacheMeshRenderer()
		{
			if (cachedMeshRenderer == null)
			{
				cachedMeshRenderer = GetComponent<MeshRenderer>();
			}
		}

		public static SgtBlackHole Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBlackHole Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Black Hole", layer, parent, localPosition, localRotation, localScale);
			var instance   = gameObject.AddComponent<SgtBlackHole>();

			return instance;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Black Hole", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = SgtHelper.GetSelectedParent();
			var instance = Create(parent != null ? parent.gameObject.layer : 0, parent);

			instance.cachedMeshFilter.sharedMesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

			SgtHelper.SelectAndPing(instance);
		}
#endif

		protected virtual void OnEnable()
		{
			CacheMeshFilter();
			CacheMeshRenderer();

			if (generatedMaterial == null)
			{
				generatedMaterial = SgtHelper.CreateTempMaterial("Black Hole (Generated)", SgtHelper.ShaderNamePrefix + "BlackHole");
			}

			cachedMeshRenderer.sharedMaterial = generatedMaterial;
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedMaterial);
		}

		protected void OnWillRenderObject()
		{
			generatedMaterial.SetFloat(SgtShader._PinchPower, pinch);
			generatedMaterial.SetFloat(SgtShader._PinchScale, warp);
			generatedMaterial.SetVector(SgtShader._WorldPosition, SgtHelper.NewVector4(transform.position, 1.0f));

			generatedMaterial.SetFloat(SgtShader._HolePower, holeSharpness);
			generatedMaterial.SetColor(SgtShader._HoleColor, holeColor);
			generatedMaterial.SetFloat(SgtShader._HoleSize, holeSize);

			generatedMaterial.SetFloat(SgtShader._TintPower, tintSharpness);
			generatedMaterial.SetColor(SgtShader._TintColor, tintColor);

			generatedMaterial.SetFloat(SgtShader._FadePower, fadePower);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtBlackHole))]
	public class SgtBlackHole_Editor : SgtEditor<SgtBlackHole>
	{
		protected override void OnInspector()
		{
			Draw("pinch", "The higher you set this, the smaller the spatial distortion will be.");
			Draw("warp", "The higher you set this, the more space will bend around the black hole.");

			Separator();

			BeginError(Any(t => t.HoleSize <= 0.0f));
				Draw("holeSize", "This allows you to control the overall size of the hole relative to the pinch.");
			EndError();
			BeginError(Any(t => t.HoleSharpness <= 0.0f));
				Draw("holeSharpness", "This allows you to control how sharp/abrupt the transition between space and the event horizon is.");
			EndError();
			Draw("holeColor", "This allows you to control the color of the black hole past the event horizon.");

			Separator();

			BeginError(Any(t => t.TintSharpness < 0.0f));
				Draw("tintSharpness", "How sharp the tint color gradient is.");
			EndError();
			Draw("tintColor", "The color of the tint.");

			Separator();
			
			Draw("fadePower", "This allows you to fade the edges of the black hole. This is useful if you have multiple black holes near each other.");
		}
	}
}
#endif