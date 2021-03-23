using UnityEngine;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to override the attached <b>Light.intensity</b> value based on the rendering pipeline.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtLightOverride")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Light Override")]
	public class SgtLightOverride : MonoBehaviour
	{
		public float IntensityInStandard { set  { intensityInStandard = value; } get { return intensityInStandard; } } [SerializeField] private float intensityInStandard = -1.0f;

		public float IntensityInURP { set  { intensityInURP = value; } get { return intensityInURP; } } [SerializeField] private float intensityInURP = -1.0f;

		public float IntensityInHDRP { set  { intensityInHDRP = value; } get { return intensityInHDRP; } } [SerializeField] private float intensityInHDRP = -1.0f;

		[System.NonSerialized]
		private Light cachedLight;

		[System.NonSerialized]
		private bool cachedLightSet;

		protected virtual void Update()
		{
			var pipe = SgtShaderBundle.DetectProjectPipeline();

			if (SgtShaderBundle.IsStandard(pipe) == true)
			{
				ApplyIntensity(intensityInStandard);
			}
			else if (SgtShaderBundle.IsURP(pipe) == true)
			{
				ApplyIntensity(intensityInURP);
			}
			else if (SgtShaderBundle.IsHDRP(pipe) == true)
			{
				ApplyIntensity(intensityInHDRP);
			}
		}

		private void ApplyIntensity(float intensity)
		{
			if (intensity >= 0.0f)
			{
				if (cachedLightSet == false)
				{
					cachedLight    = GetComponent<Light>();
					cachedLightSet = true;
				}

				cachedLight.intensity = intensity;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtLightOverride))]
	public class SgtLightOverride_Editor : SgtEditor<SgtLightOverride>
	{
		protected override void OnInspector()
		{
			Draw("intensityInStandard");
			Draw("intensityInURP");
			Draw("intensityInHDRP");
		}
	}
}
#endif