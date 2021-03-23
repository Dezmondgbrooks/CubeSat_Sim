using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to control the specified thrusters with the specified control axes.</summary>
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Thruster Controls")]
	public class SgtThrusterControls : MonoBehaviour
	{
		[System.Serializable]
		public class Control
		{
			[Tooltip("The fingers or keys used to control these thrusters.")]
			public SgtInputManager.Bind Bind;

			public bool Inverse;

			public bool Bidirectional;

			public List<SgtThruster> Positive;

			public List<SgtThruster> Negative;
		}

		/// <summary>Is this component currently listening for inputs?</summary>
		public bool Listen { set { listen = value; } get { return listen; } } [SerializeField] private bool listen = true;

		/// <summary>Fingers that began touching the screen on top of these UI layers will be ignored.</summary>
		public LayerMask GuiLayers { set { guiLayers = value; } get { return guiLayers; } } [SerializeField] private LayerMask guiLayers = 1 << 5;

		public List<Control> Controls { get { if (controls == null) controls = new List<Control>(); return controls; } } [FSA("binds")] [SerializeField] private List<Control> controls;

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
			if (controls != null)
			{
				for (var i = controls.Count - 1; i >= 0; i--)
				{
					var control = controls[i];

					if (control != null)
					{
						var throttle = control.Bind.GetValue(fingers);

						if (control.Inverse == true)
						{
							throttle = -throttle;
						}

						if (control.Bidirectional == false)
						{
							if (throttle < 0.0f)
							{
								throttle = 0.0f;
							}
						}

						for (var j = control.Positive.Count - 1; j >= 0; j--)
						{
							var thruster = control.Positive[j];

							if (thruster != null)
							{
								thruster.Throttle = throttle;
							}
						}

						for (var j = control.Negative.Count - 1; j >= 0; j--)
						{
							var thruster = control.Negative[j];

							if (thruster != null)
							{
								thruster.Throttle = throttle;
							}
						}
					}
				}
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
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtThrusterControls))]
	public class SgtThrusterControl_Editor : SgtEditor<SgtThrusterControls>
	{
		protected override void OnInspector()
		{
			Draw("controls");
		}
	}
}
#endif