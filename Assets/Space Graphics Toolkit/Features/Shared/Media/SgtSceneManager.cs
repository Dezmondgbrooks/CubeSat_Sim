using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is used to modify the current scene to make it look consistent between different rendering pipelines.</summary>
	[ExecuteInEditMode]
	public class SgtSceneManager : MonoBehaviour
	{
		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the camera exposure to match the other pipelines.</summary>
		public bool ChangeExposureInHDRP { set { changeExposureInHDRP = value; } get { return changeExposureInHDRP; } } [SerializeField] private bool changeExposureInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the background to match the other pipelines.</summary>
		public bool ChangeVisualEnvironmentInHDRP { set { changeVisualEnvironmentInHDRP = value; } get { return changeVisualEnvironmentInHDRP; } } [SerializeField] private bool changeVisualEnvironmentInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the fog to match the other pipelines.</summary>
		public bool ChangeFogInHDRP { set { changeFogInHDRP = value; } get { return changeFogInHDRP; } } [SerializeField] private bool changeFogInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then any lights missing the <b>HDAdditionalLightData</b> component will have it added.</summary>
		public bool UpgradeLightsInHDRP { set { upgradeLightsInHDRP = value; } get { return upgradeLightsInHDRP; } } [SerializeField] private bool upgradeLightsInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then any cameras missing the <b>HDAdditionalCameraData</b> component will have it added.</summary>
		public bool UpgradeCamerasInHDRP { set { upgradeCamerasInHDRP = value; } get { return upgradeCamerasInHDRP; } } [SerializeField] private bool upgradeCamerasInHDRP = true;


		protected virtual void OnEnable()
		{
			var pipe = SgtShaderBundle.DetectProjectPipeline();

			if (SgtShaderBundle.IsHDRP(pipe) == true)
			{
				if (changeExposureInHDRP == true || changeVisualEnvironmentInHDRP == true || changeFogInHDRP == true)
				{
					TryCreateVolume();
				}

				if (upgradeLightsInHDRP == true)
				{
					TryUpgradeLights();
				}

				if (upgradeCamerasInHDRP == true)
				{
					TryUpgradeCameras();
				}
			}
		}

		private void TryCreateVolume()
		{
	#if __HDRP__
			var volume = GetComponent<Volume>();

			if (volume == null)
			{
				volume = gameObject.AddComponent<Volume>();
			}

			var profile = volume.profile;

			if (profile == null)
			{
				profile = ScriptableObject.CreateInstance<VolumeProfile>();

				profile.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
			}

			if (profile.components.Count == 0)
			{
				name = "SgtSceneManager (Volume Added)";

				if (changeExposureInHDRP == true)
				{
					var exposure = profile.Add<UnityEngine.Rendering.HighDefinition.Exposure>(true);

					exposure.mode.overrideState = true;

					exposure.fixedExposure.overrideState = true;
					exposure.fixedExposure.value         = 15.0f;
				}

				if (changeVisualEnvironmentInHDRP == true)
				{
					var visualEnvironment = profile.Add<UnityEngine.Rendering.HighDefinition.VisualEnvironment>(true);

					visualEnvironment.skyType.overrideState = true;
					visualEnvironment.skyType.value         = 0;
				}

				if (changeFogInHDRP == true)
				{
					var fog = profile.Add<UnityEngine.Rendering.HighDefinition.Fog>(true);

					fog.enabled.overrideState = true;
				}
			}

			volume.profile = profile;
	#endif
		}

		private void TryUpgradeLights()
		{
	#if __HDRP__
			foreach (var light in FindObjectsOfType<Light>())
			{
				if (light.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>() == null)
				{
					light.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
				}
			}
	#endif
		}

		private void TryUpgradeCameras()
		{
	#if __HDRP__
			foreach (var camera in FindObjectsOfType<Camera>())
			{
				if (camera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() == null)
				{
					var hdCamera = camera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
					
					hdCamera.backgroundColorHDR = Color.black;
				}
			}
	#endif
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CustomEditor(typeof(SgtSceneManager))]
	public class SgtSceneManager_Editor : SgtEditor<SgtSceneManager>
	{
		protected override void OnInspector()
		{
			Draw("changeExposureInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to this GameObject that adjusts the camera exposure to match the other pipelines.");
			Draw("changeVisualEnvironmentInHDRP", "If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to this GameObject that adjusts the background to match the other pipelines.");
			Draw("changeFogInHDRP", "If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the fog to match the other pipelines.");
			Draw("upgradeLightsInHDRP", "If you enable this setting and your project is running with HDRP then any lights missing the <b>HDAdditionalLightData</b> component will have it added.");
			Draw("upgradeCamerasInHDRP", "If you enable this setting and your project is running with HDRP then any cameras missing the <b>HDAdditionalCameraData</b> component will have it added.");
		}
	}
}
#endif