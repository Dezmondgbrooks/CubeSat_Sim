using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for all warp styles.</summary>
	public abstract class SgtFloatingWarp : MonoBehaviour
	{
		/// <summary>The point that will be warped.</summary>
		public SgtFloatingPoint Point { set { point = value; } get { return point; } } [FSA("Point")] [SerializeField] protected SgtFloatingPoint point;

		/// <summary>Allows you to warp to the target object.</summary>
		public void WarpTo(SgtFloatingTarget target)
		{
			if (target != null)
			{
				WarpTo(target.CachedPoint.Position, target.WarpDistance);
			}
		}

		/// <summary>Allows you to warp to the target point with the specified separation distance.</summary>
		public void WarpTo(SgtPosition position, double distance)
		{
			// Make sure we don't warp directly onto the star
			var direction = SgtPosition.Direction(point.Position, position);

			position.LocalX -= direction.x * distance;
			position.LocalY -= direction.y * distance;
			position.LocalZ -= direction.z * distance;
			position.SnapLocal();

			WarpTo(position);
		}

		public abstract bool CanAbortWarp
		{
			get;
		}

		public abstract void WarpTo(SgtPosition position);

		public abstract void AbortWarp();
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingWarp))]
	public abstract class SgtFloatingWarp_Editor<T> : SgtEditor<T>
		where T : SgtFloatingWarp
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Point == null));
				Draw("point", "The point that will be warped.");
			EndError();
		}
	}
}
#endif