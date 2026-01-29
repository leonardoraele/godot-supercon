using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

public partial class GravityComponent : SuperconStateComponent3D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.None, "suffix:px/s")] public float MaxFallSpeed = float.PositiveInfinity;
	[Export] public float GravityMultiplier = 5f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector3 GravityDirection;
	public float GravityMagnitude; // m/s²

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		this.GravityDirection = ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3().Normalized();
		this.GravityMagnitude = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		this.Character?.ApplyForceWithMaxSpeed(
			this.GravityDirection * this.GravityMagnitude * (float) delta * this.GravityMultiplier,
			this.MaxFallSpeed
		);
	}
}
