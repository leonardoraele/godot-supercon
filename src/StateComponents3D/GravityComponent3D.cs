using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

[Tool][GlobalClass]
public partial class GravityComponent3D : SuperconStateComponent3D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float Mass

		{ get; set { field = value.AtLeast(0); } }
		= 1f;

	[ExportGroup("Options")]
	[Export(PropertyHint.None, "suffix:px/s")] public float MaxFallSpeed = float.PositiveInfinity;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
		this.Character3D?.ApplyForceAndLimitSpeed(
			this.Character3D.GetGravity() * this.Mass * (float) delta,
			this.MaxFallSpeed
		);
	}
}
