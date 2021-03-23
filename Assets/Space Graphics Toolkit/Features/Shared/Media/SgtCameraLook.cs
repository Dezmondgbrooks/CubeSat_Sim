using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to rotate the current GameObject based on mouse/finger drags. NOTE: This requires the SgtInputManager in your scene to function.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCameraLook")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera Look")]
	public class SgtCameraLook : MonoBehaviour
	{
		/// <summary>Is this component currently listening for inputs?</summary>
		public bool Listen { set { listen = value; } get { return listen; } } [SerializeField] private bool listen = true;

		/// <summary>Fingers that began touching the screen on top of these UI layers will be ignored.</summary>
		public LayerMask GuiLayers { set { guiLayers = value; } get { return guiLayers; } } [SerializeField] private LayerMask guiLayers = 1 << 5;

		/// <summary>How quickly the rotation transitions from the current to the target value (-1 = instant).</summary>
		public float Damping { set { damping = value; } get { return damping; } } [FSA("Dampening")] [SerializeField] private float damping = 10.0f;

		/// <summary>The keys/fingers required to pitch down/up.</summary>
		public SgtInputManager.Bind BindPitch { set { bindPitch = value; } get { return bindPitch; } } [SerializeField] private SgtInputManager.Bind bindPitch = new SgtInputManager.Bind(1, SgtInputManager.FingerGesture.VerticalDrag, -10.0f, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, 45.0f);

		/// <summary>The keys/fingers required to yaw left/right.</summary>
		public SgtInputManager.Bind BindYaw { set { bindYaw = value; } get { return bindYaw; } } [SerializeField] private SgtInputManager.Bind bindYaw = new SgtInputManager.Bind(1, SgtInputManager.FingerGesture.HorizontalDrag, 10.0f, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, 45.0f);

		/// <summary>The keys/fingers required to roll left/right.</summary>
		public SgtInputManager.Bind BindRoll { set { bindRoll = value; } get { return bindRoll; } } [SerializeField] private SgtInputManager.Bind bindRoll = new SgtInputManager.Bind(2, SgtInputManager.FingerGesture.Twist, 1.0f, KeyCode.E, KeyCode.Q, KeyCode.None, KeyCode.None, 45.0f);

		[System.NonSerialized]
		private Quaternion remainingDelta = Quaternion.identity;

		[System.NonSerialized]
		private List<SgtInputManager.Finger> fingers = new List<SgtInputManager.Finger>();

		protected virtual void OnEnable()
		{
			SgtInputManager.EnsureThisComponentExists();

			SgtInputManager.OnFingerDown += HandleFingerDown;
			SgtInputManager.OnFingerUp   += HandleFingerUp;
		}

		protected virtual void OnDisable()
		{
			SgtInputManager.OnFingerDown -= HandleFingerDown;
			SgtInputManager.OnFingerUp   -= HandleFingerUp;
		}

		protected virtual void Update()
		{
			if (listen == true)
			{
				AddToDelta();
			}

			DampenDelta();
		}

		private void HandleFingerDown(SgtInputManager.Finger finger)
		{
			if (SgtInputManager.PointOverGui(finger.ScreenPosition, guiLayers) == true)
			{
				return;
			}

			fingers.Add(finger);
		}

		private void HandleFingerUp(SgtInputManager.Finger finger)
		{
			fingers.Remove(finger);
		}

		private void AddToDelta()
		{
			// Get delta from binds
			var delta = default(Vector3);

			delta.x = bindPitch.GetValue(fingers);
			delta.y = bindYaw  .GetValue(fingers);
			delta.z = bindRoll .GetValue(fingers);

			// Store old rotation
			var oldRotation = transform.localRotation;

			// Rotate
			transform.Rotate(delta.x * Time.deltaTime, delta.y * Time.deltaTime, 0.0f, Space.Self);

			transform.Rotate(0.0f, 0.0f, delta.z * Time.deltaTime, Space.Self);

			// Add to remaining
			remainingDelta *= Quaternion.Inverse(oldRotation) * transform.localRotation;

			// Revert rotation
			transform.localRotation = oldRotation;
		}

		private void DampenDelta()
		{
			// Dampen remaining delta
			var factor   = SgtHelper.DampenFactor(damping, Time.deltaTime);
			var newDelta = Quaternion.Slerp(remainingDelta, Quaternion.identity, factor);

			// Rotate by difference
			transform.localRotation = transform.localRotation * Quaternion.Inverse(newDelta) * remainingDelta;

			// Update remaining
			remainingDelta = newDelta;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCameraLook))]
	public class SgtCameraLook_Editor : SgtEditor<SgtCameraLook>
	{
		protected override void OnInspector()
		{
			Draw("listen", "Is this component currently listening for inputs?");
			Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");
			Draw("damping", "How quickly the rotation transitions from the current to the target value (-1 = instant).");

			Separator();

			Draw("bindPitch", "The keys/fingers required to pitch down/up.");
			Draw("bindYaw", "The keys/fingers required to yaw left/right.");
			Draw("bindRoll", "The keys/fingers required to roll left/right.");
		}
	}
}
#endif