using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component can be added alongside the <b>SgtPlanet</b> component to give it an animated water surface texture.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtPlanet))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtPlanetWaterGradient")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Planet Water Gradient")]
	public class SgtPlanetWaterGradient : MonoBehaviour
	{
		/// <summary>The color of shallow water.</summary>
		public Color Shallow { set { shallow = value; DirtyTexture(); } get { return shallow; } } [SerializeField] private Color shallow = new Color(0.75f, 0.75f, 1.0f);

		/// <summary>The color of deep water.</summary>
		public Color Deep { set { deep = value; DirtyTexture(); } get { return deep; } } [SerializeField] private Color deep = new Color(0.25f, 0.25f, 1.0f);

		/// <summary>The way the color transitions between shallow and deep.</summary>
		public SgtEase.Type Ease { set { if (ease != value) { ease = value; DirtyTexture(); } } get { return ease; } } [SerializeField] private SgtEase.Type ease = SgtEase.Type.Smoothstep;

		/// <summary>This allows you to push the color toward the shallow or deep end.</summary>
		public float Sharpness { set { if (sharpness != value) { sharpness = value; DirtyTexture(); } } get { return sharpness; } } [SerializeField] private float sharpness = 1.0f;

		/// <summary>The scale of the depth.</summary>
		public float Scale { set { scale = value; DirtyScale(); } get { return scale; } } [SerializeField] private float scale = 10.0f;

		[System.NonSerialized]
		private SgtPlanet cachedPlanet;

		[System.NonSerialized]
		private bool cachedPlanetSet;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		public SgtPlanet CachedPlanet
		{
			get
			{
				if (cachedPlanetSet == false)
				{
					cachedPlanet    = GetComponent<SgtPlanet>();
					cachedPlanetSet = true;
				}

				return cachedPlanet;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTexture();
			UpdateScale();
		}

		protected virtual void OnDisable()
		{
			cachedPlanet.Properties.Clear(Shader.PropertyToID("_WaterGradient"));
		}

		protected virtual void OnDestroy()
		{
			if (generatedTexture != null)
			{
				generatedTexture = SgtHelper.Destroy(generatedTexture);
			}
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			UpdateTexture();
		}
#endif

		public void DirtyTexture()
		{
			UpdateTexture();
		}

		public void DirtyScale()
		{
			UpdateScale();
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated texture as an asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Water Gradient");

			if (importer != null)
			{
				importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
				importer.wrapMode           = TextureWrapMode.Clamp;
				importer.filterMode         = FilterMode.Bilinear;
				importer.anisoLevel         = 16;

				importer.SaveAndReimport();
			}
		}
#endif

		private void UpdateTexture()
		{
			if (generatedTexture == null)
			{
				generatedTexture = new Texture2D(64, 1, TextureFormat.RGB24, false);

				generatedTexture.wrapMode = TextureWrapMode.Clamp;
			}

			for (var i = 0; i < 64; i++)
			{
				var t = SgtEase.Evaluate(ease, SgtHelper.Sharpness(i / 63.0f, sharpness));

				generatedTexture.SetPixel(i, 0, Color.Lerp(shallow, deep, t));
			}

			generatedTexture.Apply();

			CachedPlanet.Properties.SetTexture(Shader.PropertyToID("_WaterGradient"), generatedTexture);
		}

		private void UpdateScale()
		{
			CachedPlanet.Properties.SetFloat(Shader.PropertyToID("_WaterGradientScale"), scale);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtPlanetWaterGradient))]
	public class SgtPlanetWaterGradient_Editor : SgtEditor<SgtPlanetWaterGradient>
	{
		protected override void OnInspector()
		{
			var dirtyTexture = false;
			var dirtyScale = false;

			Draw("shallow", ref dirtyTexture, "The color of shallow water.");
			Draw("deep", ref dirtyTexture, "The color of deep water.");
			Draw("ease", ref dirtyTexture, "The way the color transitions between shallow and deep.");
			Draw("sharpness", ref dirtyTexture, "This allows you to push the color toward the shallow or deep end.");
			Draw("scale", ref dirtyScale, "The scale of the depth.");

			if (dirtyTexture == true) DirtyEach(t => t.DirtyTexture());
			if (dirtyScale == true) DirtyEach(t => t.DirtyScale());
		}
	}
}
#endif