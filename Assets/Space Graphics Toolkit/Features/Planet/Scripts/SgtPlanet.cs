using System.Collections.Generic;
using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a planet that has been displaced with a heightmap, and has a dynamic water level.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtPlanet")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Planet")]
	public class SgtPlanet : MonoBehaviour, IOverridableSharedMaterial
	{
		/// <summary>The sphere mesh used to render the planet.</summary>
		public Mesh Mesh { set { mesh = value; DirtyMesh(); } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>The rendering layer used to render the planet.</summary>
		//public int RenderingLayer { set { renderingLayer = value; } get { return renderingLayer; } } [SerializeField] [SgtLayer] private int renderingLayer;

		/// <summary>If you want the generated mesh to have a matching collider, you can specify it here.</summary>
		public MeshCollider MeshCollider { set { meshCollider = value; DirtyMesh(); } get { return meshCollider; } } [SerializeField] private MeshCollider meshCollider;

		/// <summary>The radius of the planet in local space.</summary>
		public float Radius { set { radius = value; DirtyMesh(); } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The material used to render the planet. For best results, this should use the SGT Planet shader.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		/// <summary>If you want to apply a shared material (e.g. atmosphere) to this terrain, then specify it here.</summary>
		public SgtSharedMaterial SharedMaterial { set { sharedMaterial = value; } get { return sharedMaterial; } } [SerializeField] private SgtSharedMaterial sharedMaterial;

		/// <summary>The current water level.
		/// 0 = Radius.
		/// 1 = Radius + Displacement.</summary>
		public float WaterLevel { set { waterLevel = value; DirtyMesh(); } get { return waterLevel; } } [Range(-2.0f, 2.0f)] [SerializeField] private float waterLevel;

		/// <summary>Should the planet mesh be displaced using the heightmap in the planet material?</summary>
		public bool Displace { set { displace = value; DirtyMesh(); } get { return displace; } } [SerializeField] private bool displace;

		/// <summary>The maximum height displacement applied to the planet mesh when the heightmap alpha value is 1.</summary>
		public float Displacement { set { displacement = value; DirtyMesh(); } get { return displacement; } } [SerializeField] private float displacement = 0.1f;

		/// <summary>If you enable this then the water will not rise, instead the terrain will shrink down.</summary>
		public bool ClampWater { set { clampWater = value; DirtyMesh(); } get { return clampWater; } } [SerializeField] private bool clampWater;

		public event SgtSharedMaterial.OverrideSharedMaterialSignature OnOverrideSharedMaterial;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private List<Vector3> generatedPositions = new List<Vector3>();

		[System.NonSerialized]
		private List<Vector4> generatedCoords = new List<Vector4>();

		[System.NonSerialized]
		private SgtProperties properties = new SgtProperties();

		[System.NonSerialized]
		private bool dirtyMesh;

		[System.NonSerialized]
		private Texture2D lastHeightmap;

		public SgtProperties Properties
		{
			get
			{
				return properties;
			}
		}

		public Texture2D MaterialHeightmap
		{
			get
			{
				return material != null ? material.GetTexture(SgtShader._HeightMap) as Texture2D : null;
			}
		}

		public bool MaterialHasWater
		{
			get
			{
				return material != null ? material.GetFloat(SgtShader._HasWater) == 1.0f : false;
			}
		}

		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		public void RegisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e)
		{
			OnOverrideSharedMaterial += e;
		}

		public void UnregisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e)
		{
			OnOverrideSharedMaterial -= e;
		}

		/// <summary>This method causes the planet mesh to update based on the current settings. You should call this after you finish modifying them.</summary>
		[ContextMenu("Rebuild")]
		public void Rebuild()
		{
			dirtyMesh     = false;
			generatedMesh = SgtHelper.Destroy(generatedMesh);

			if (mesh != null)
			{
				generatedMesh = Instantiate(mesh);

				generatedMesh.GetVertices(generatedPositions);
				generatedMesh.GetUVs(0, generatedCoords);

				var count = generatedMesh.vertexCount;

				lastHeightmap = MaterialHeightmap;
#if UNITY_EDITOR
				SgtHelper.MakeTextureReadable(lastHeightmap);
#endif

				for (var i = 0; i < count; i++)
				{
					var height = radius;
					var vector = generatedPositions[i].normalized;
					var coord  = generatedCoords[i];

					if (vector.y > 0.0f)
					{
						coord.z = -vector.x * 0.5f;
						coord.w = vector.z * 0.5f;
					}
					else
					{
						coord.z = vector.x * 0.5f;
						coord.w = vector.z * 0.5f;
					}

					generatedCoords[i] = coord;

					generatedPositions[i] = vector * Sample(vector);
				}

				generatedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * (radius + displacement) * 2.0f);

				generatedMesh.SetVertices(generatedPositions);
				generatedMesh.SetUVs(0, generatedCoords);

				generatedMesh.RecalculateNormals();
				generatedMesh.RecalculateTangents();

				if (meshCollider != null)
				{
					meshCollider.sharedMesh = null;
					meshCollider.sharedMesh = generatedMesh;
				}
			}
		}

		public static SgtPlanet Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtPlanet Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Planet", layer, parent, localPosition, localRotation, localScale);
			var instance   = gameObject.AddComponent<SgtPlanet>();

			return instance;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Planet", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = SgtHelper.GetSelectedParent();
			var instance = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(instance);
		}
#endif

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;

			SgtHelper.OnCalculateDistance += HandleCalculateDistance;
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;

			SgtHelper.OnCalculateDistance -= HandleCalculateDistance;
		}

		protected virtual void LateUpdate()
		{
			if (generatedMesh == null || dirtyMesh == true)
			{
				Rebuild();
			}

			if (generatedMesh != null && material != null)
			{
				Properties.SetFloat(SgtShader._WaterLevel, waterLevel);

				// Write direction of nearest light?
				if (material.GetFloat("_HasNight") == 1.0f)
				{
					var mask   = 1 << gameObject.layer;
					var lights = SgtLight.Find(true, mask, transform.position);

					SgtLight.FilterOut(transform.position);

					if (lights.Count > 0)
					{
						var position  = Vector3.zero;
						var direction = Vector3.forward;
						var color     = Color.white;

						SgtLight.Calculate(lights[0], transform.position, default(Transform), default(Transform), ref position, ref direction, ref color);

						properties.SetVector(Shader.PropertyToID("_NightDirection"), -direction);
					}
				}
			}
		}

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtHelper.CanDraw(gameObject, camera) == false) return;

			//var layer = SgtHelper.GetRenderingLayers(gameObject, renderingLayer);
			var layer = gameObject.layer;

			Graphics.DrawMesh(generatedMesh, transform.localToWorldMatrix, material, layer, camera, 0, properties);

			var finalSharedMaterial = sharedMaterial;

			if (OnOverrideSharedMaterial != null)
			{
				OnOverrideSharedMaterial.Invoke(ref finalSharedMaterial, camera);
			}

			if (SgtHelper.Enabled(finalSharedMaterial) == true && finalSharedMaterial.Material != null)
			{
				Graphics.DrawMesh(generatedMesh, transform.localToWorldMatrix, finalSharedMaterial.Material, layer, camera, 0, properties);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedMesh);
		}

		private void HandleCalculateDistance(Vector3 worldPosition, ref float distance)
		{
			var localPosition = transform.InverseTransformPoint(worldPosition);

			localPosition = localPosition.normalized * Sample(localPosition);

			var surfacePosition = transform.TransformPoint(localPosition);
			var thisDistance    = Vector3.Distance(worldPosition, surfacePosition);

			if (thisDistance < distance)
			{
				distance = thisDistance;
			}
		}

		private float Sample(Vector3 vector)
		{
			var final = radius;

			if (displace == true && lastHeightmap != null)
			{
				var uv   = SgtHelper.CartesianToPolarUV(vector);
				var land = lastHeightmap.GetPixelBilinear(uv.x, uv.y).a;

				if (clampWater == true)
				{
					final += displacement * Mathf.InverseLerp(Mathf.Clamp01(waterLevel), 1.0f, land);
				}
				else
				{
					final += displacement * Mathf.Max(land, waterLevel);
				}
			}

			return final;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtPlanet))]
	public class SgtPlanet_Editor : SgtEditor<SgtPlanet>
	{
		protected override void OnInspector()
		{
			var dirtyMesh = false;

			BeginError(Any(t => t.Mesh == null));
				Draw("mesh", ref dirtyMesh, "The sphere mesh used to render the planet.");
			EndError();
			//Draw("renderingLayer", "The rendering layer used to render the planet.");
			Draw("meshCollider", ref dirtyMesh, "If you want the generated mesh to have a matching collider, you can specify it here.");
			BeginError(Any(t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the planet in local space.");
			EndError();

			Separator();

			BeginError(Any(t => t.Material == null));
				Draw("material", "The material used to render the planet. For best results, this should use the SGT Planet shader.");
			EndError();
			Draw("sharedMaterial", "If you want to apply a shared material (e.g. atmosphere) to this terrain, then specify it here.");

			Separator();

			if (Any(t => t.MaterialHeightmap != null))
			{
				if (Any(t => t.MaterialHasWater == true))
				{
					Draw("waterLevel", ref dirtyMesh, "The current water level.\n\n0 = Radius.\n\n1 = Radius + Displacement.");
				}
				Draw("displace", ref dirtyMesh, "Should the planet mesh be displaced using the heightmap in the planet material?");
				if (Any(t => t.Displace == true))
				{
					BeginIndent();
						BeginError(Any(t => t.Displacement == 0.0f));
							Draw("displacement", ref dirtyMesh, "The maximum height displacement applied to the planet mesh when the heightmap alpha value is 1.");
						EndError();
						Draw("clampWater", ref dirtyMesh, "If you enable this then the water will not rise, instead the terrain will shrink down.");
					EndIndent();
				}
			}

			if (Any(t => t.MaterialHasWater == true && t.GetComponent<SgtPlanetWaterGradient>() == null))
			{
				Separator();

				if (HelpButton("This material has water, but you have no WaterGradient component.", MessageType.Info, "Fix", 50) == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtPlanetWaterGradient>(t.gameObject));
				}
			}

			if (Any(t => t.MaterialHasWater == true && t.GetComponent<SgtPlanetWaterTexture>() == null))
			{
				Separator();

				if (HelpButton("This material has water, but you have no WaterTexture component.", MessageType.Info, "Fix", 50) == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtPlanetWaterTexture>(t.gameObject));
				}
			}

			if (dirtyMesh == true) DirtyEach(t => t.DirtyMesh());
		}
	}
}
#endif