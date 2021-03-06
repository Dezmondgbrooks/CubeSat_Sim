using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component monitors the attached Camera for modifications in roll angle, and stores the total change.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCamera")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera")]
	public class SgtCamera : SgtLinkedBehaviour<SgtCamera>
	{
		public static event System.Action<Camera> OnCameraPreCull;

		public static event System.Action<Camera> OnCameraDraw;

		public static event System.Action<Camera> OnCameraPreRender;

		public static event System.Action<Camera> OnCameraPostRender;

		/// <summary>The amount of degrees this camera has rolled (used to counteract billboard non-rotation).</summary>
		public float RollAngle { set { rollAngle = value; } get { return rollAngle; } } [FSA("RollAngle")] [SerializeField] private float rollAngle;

		/// <summary>A quaternion of the current roll angle.</summary>
		public Quaternion RollQuaternion { get { return rollQuaternion; } } [SerializeField] private Quaternion rollQuaternion = Quaternion.identity;

		/// <summary>A matrix of the current roll angle.</summary>
		public Matrix4x4 RollMatrix { get { return rollMatrix; } } [SerializeField] private Matrix4x4 rollMatrix = Matrix4x4.identity;

		// The change in position of this GameObject over the past frame
		[System.NonSerialized]
		private Vector3 deltaPosition;

		// The current velocity of this GameObject per second
		public Vector3 Velocity { get { return velocity; } } [System.NonSerialized] private Vector3 velocity;

		// Previous frame rotation
		[System.NonSerialized]
		private Quaternion oldRotation = Quaternion.identity;

		// Previous frame position
		[System.NonSerialized]
		private Vector3 oldPosition;

		// The camera this camera is attached to
		[System.NonSerialized]
		private Camera cachedCamera;

		[System.NonSerialized]
		private bool cachedCameraSet;

		public Camera CachedCamera
		{
			get
			{
				if (cachedCameraSet == false)
				{
					cachedCamera    = GetComponent<Camera>();
					cachedCameraSet = true;
				}

				return cachedCamera;
			}
		}

		static SgtCamera()
		{
			Camera.onPreCull += (camera) =>
				{
					if (OnCameraPreCull != null) OnCameraPreCull(camera);
					if (OnCameraDraw != null) OnCameraDraw(camera);
				};

			Camera.onPreRender += (camera) =>
				{
					if (OnCameraPreRender != null) OnCameraPreRender(camera);
				};

			Camera.onPostRender += (camera) =>
				{
					if (OnCameraPostRender != null) OnCameraPostRender(camera);
				};
			
#if UNITY_2019_1_OR_NEWER
			UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += (context, camera) =>
				{
					if (OnCameraPreCull != null) OnCameraPreCull(camera);
					if (OnCameraDraw != null) OnCameraDraw(camera);
					if (OnCameraPreRender != null) OnCameraPreRender(camera);
				};

			UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += (context, camera) =>
				{
					if (OnCameraPostRender != null) OnCameraPostRender(camera);
				};
#elif UNITY_2018_1_OR_NEWER
			UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering += (camera) =>
				{
					if (OnCameraPreCull != null) OnCameraPreCull(camera);
					if (OnCameraDraw != null) OnCameraDraw(camera);
					if (OnCameraPreRender != null) OnCameraPreRender(camera);
				};
#endif
		}

		// Find the camera attached to a specific camera, if it exists
		public static bool TryFind(Camera unityCamera, ref SgtCamera foundCamera)
		{
			var camera = FirstInstance;

			for (var i = 0; i < InstanceCount; i++)
			{
				if (camera.CachedCamera == unityCamera)
				{
					foundCamera = camera; return true;
				}

				camera = camera.NextInstance;
			}

			return false;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			oldRotation = transform.rotation;
			oldPosition = transform.position;

			SgtHelper.OnSnap += HandleSnap;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			SgtHelper.OnSnap -= HandleSnap;
		}

		protected virtual void LateUpdate()
		{
			var newRotation   = transform.rotation;
			var newPosition   = transform.position;
			var deltaRotation = Quaternion.Inverse(oldRotation) * newRotation;

			rollAngle      = (rollAngle - deltaRotation.eulerAngles.z) % 360.0f;
			rollQuaternion = Quaternion.Euler(0.0f, 0.0f, rollAngle);
			rollMatrix     = Matrix4x4.Rotate(rollQuaternion);
			deltaPosition  = oldPosition - newPosition;
			velocity       = SgtHelper.Reciprocal(Time.deltaTime) * deltaPosition;
			oldRotation    = newRotation;
			oldPosition    = newPosition;
		}

		private void HandleSnap(Vector3 delta)
		{
			oldPosition += delta;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCamera))]
	public class SgtCamera_Editor : SgtEditor<SgtCamera>
	{
		protected override void OnInspector()
		{
			Draw("rollAngle", "The amount of degrees this camera has rolled (used to counteract billboard non-rotation).");
		}
	}
}
#endif