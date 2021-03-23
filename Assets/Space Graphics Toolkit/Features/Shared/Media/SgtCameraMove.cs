using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to move the current GameObject based on WASD/mouse/finger drags. NOTE: This requires the SgtInputManager in your scene to function.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCameraMove")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera Move")]
	public class SgtCameraMove : MonoBehaviour
	{
		public enum RotationType
		{
			None,
			Acceleration,
			MainCamera
		}

		/// <summary>Is this component currently listening for inputs?</summary>
		public bool Listen { set { listen = value; } get { return listen; } } [SerializeField] private bool listen = true;

		/// <summary>Fingers that began touching the screen on top of these UI layers will be ignored.</summary>
		public LayerMask GuiLayers { set { guiLayers = value; } get { return guiLayers; } } [SerializeField] private LayerMask guiLayers = 1 << 5;

		/// <summary>How quickly the position goes to the target value (-1 = instant).</summary>
		public float Damping { set { damping = value; } get { return damping; } } [FSA("Dampening")] [SerializeField] private float damping = 10.0f;

		/// <summary>If you want movements to apply to Rigidbody.velocity, set it here.</summary>
		public Rigidbody Target { set { target = value; } get { return target; } } [FSA("Target")] [SerializeField] private Rigidbody target;

		/// <summary>If the target is something like a spaceship, rotate it based on movement?</summary>
		public RotationType TargetRotation { set { targetRotation = value; } get { return targetRotation; } } [FSA("TargetRotation")] [SerializeField] private RotationType targetRotation;

		/// <summary>The speed of the velocity rotation.</summary>
		public float TargetDamping { set { targetDamping = value; } get { return targetDamping; } } [FSA("targetDampening")] [SerializeField] private float targetDamping = 1.0f;

		/// <summary>The movement speed will be multiplied by this when near to planets.</summary>
		public float SpeedMin { set { speedMin = value; } get { return speedMin; } } [SerializeField] private float speedMin = 1.0f;

		/// <summary>The movement speed will be multiplied by this when far from planets.</summary>
		public float SpeedMax { set { speedMax = value; } get { return speedMax; } } [SerializeField] private float speedMax = 1.0f;

		/// <summary>The higher you set this, the faster the <b>SpeedMin</b> value will be reached when approaching planets.</summary>
		public float SpeedRange { set { speedRange = value; } get { return speedRange; } } [SerializeField] private float speedRange = 100.0f;

		/// <summary>The keys/fingers required to move left/right.</summary>
		public SgtInputManager.Bind BindHorizontal { set { bindHorizontal = value; } get { return bindHorizontal; } } [SerializeField] private SgtInputManager.Bind bindHorizontal = new SgtInputManager.Bind(2, SgtInputManager.FingerGesture.HorizontalDrag, 1.0f, KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow, 100.0f);

		/// <summary>The keys/fingers required to move backward/forward.</summary>
		public SgtInputManager.Bind BindDepth { set { bindDepth = value; } get { return bindDepth; } } [SerializeField] private SgtInputManager.Bind bindDepth = new SgtInputManager.Bind(2, SgtInputManager.FingerGesture.HorizontalDrag, 1.0f, KeyCode.S, KeyCode.W, KeyCode.DownArrow, KeyCode.UpArrow, 100.0f);

		/// <summary>The keys/fingers required to move down/up.</summary>
		public SgtInputManager.Bind BindVertical { set { bindVertical = value; } get { return bindVertical; } } [SerializeField] private SgtInputManager.Bind bindVertical = new SgtInputManager.Bind(3, SgtInputManager.FingerGesture.HorizontalDrag, 1.0f, KeyCode.F, KeyCode.R, KeyCode.None, KeyCode.None, 100.0f);

		[System.NonSerialized]
		private Vector3 remainingDelta;

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
			if (target == null && listen == true)
			{
				AddToDelta();
				DampenDelta();
			}
		}

		protected virtual void FixedUpdate()
		{
			if (target != null && listen == true)
			{
				AddToDelta();
				DampenDelta();
			}
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

		private float GetSpeedMultiplier()
		{
			if (speedMax > 0.0f)
			{
				var distance = float.PositiveInfinity;

				SgtHelper.InvokeCalculateDistance(transform.position, ref distance);

				var distance01 = Mathf.InverseLerp(speedMin * speedRange, speedMax * speedRange, distance);

				return Mathf.Lerp(speedMin, speedMax, distance01);
			}

			return 0.0f;
		}

		private void AddToDelta()
		{
			// Get delta from binds
			var delta = default(Vector3);

			delta.x = bindHorizontal.GetValue(fingers);
			delta.y = bindVertical  .GetValue(fingers);
			delta.z = bindDepth     .GetValue(fingers);

			// Store old position
			var oldPosition = transform.position;

			// Translate
			transform.Translate(delta * GetSpeedMultiplier() * Time.deltaTime, Space.Self);

			// Add to remaining
			var acceleration = transform.position - oldPosition;

			remainingDelta += acceleration;

			// Revert position
			transform.position = oldPosition;

			// Rotate to acceleration?
			if (target != null && targetRotation != RotationType.None && delta != Vector3.zero)
			{
				var factor   = SgtHelper.DampenFactor(targetDamping, Time.deltaTime);
				var rotation = target.transform.rotation;

				switch (targetRotation)
				{
					case RotationType.Acceleration:
					{
						rotation = Quaternion.LookRotation(acceleration, target.transform.up);
					}
					break;

					case RotationType.MainCamera:
					{
						var camera = Camera.main;

						if (camera != null)
						{
							rotation = camera.transform.rotation;
						}
					}
					break;
				}

				target.transform.rotation = Quaternion.Slerp(target.transform.rotation, rotation, factor);
				target.angularVelocity    = Vector3.Lerp(target.angularVelocity, Vector3.zero, factor);
			}
		}

		private void DampenDelta()
		{
			// Dampen remaining delta
			var factor   = SgtHelper.DampenFactor(damping, Time.deltaTime);
			var newDelta = Vector3.Lerp(remainingDelta, Vector3.zero, factor);

			// Translate by difference
			if (target != null)
			{
				target.velocity += remainingDelta - newDelta;
			}
			else
			{
				transform.position += remainingDelta - newDelta;
			}

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
	[CustomEditor(typeof(SgtCameraMove))]
	public class SgtCameraMove_Editor : SgtEditor<SgtCameraMove>
	{
		protected override void OnInspector()
		{
			Draw("listen", "Is this component currently listening for inputs?");
			Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");
			Draw("damping", "How quickly the position goes to the target value (-1 = instant).");

			Separator();

			Draw("target", "If you want movements to apply to Rigidbody.velocity, set it here.");
			Draw("targetRotation", "If the target is something like a spaceship, rotate it based on movement?");
			Draw("targetDamping", "The speed of the velocity rotation.");

			Separator();

			Draw("speedMin", "The movement speed will be multiplied by this when near to planets.");
			Draw("speedMax", "The movement speed will be multiplied by this when far from planets.");
			Draw("speedRange", "The higher you set this, the faster the <b>SpeedMin</b> value will be reached when approaching planets.");

			Separator();

			Draw("bindHorizontal", "The keys/fingers required to move right/left.");
			Draw("bindDepth", "The keys/fingers required to move backward/forward.");
			Draw("bindVertical", "The keys/fingers required to move down/up.");
		}
	}
}
#endif