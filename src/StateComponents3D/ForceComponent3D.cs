using Godot;

namespace Raele.Supercon.StateComponents3D;

[GlobalClass]
public partial class ForceComponent3D : SuperconStateComponent3D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// The direction to which the force is applied.
	/// </summary>
	[Export] public Vector3 Direction = Vector3.Up;
	/// <summary>
	/// If true, the direction is considered in local space.
	/// </summary>
	[Export] public bool LocalDirection = false;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Magnitude = 1f;

	[ExportGroup("Additional Options")]
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxSpeed = float.PositiveInfinity;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector3 GlobalDirection => this.LocalDirection && this.Character != null
		? (this.Character.GlobalBasis * this.Direction).Normalized()
		: this.Direction.Normalized();

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		this.Character?.ApplyForceAndLimitSpeed(this.GlobalDirection * this.Magnitude * (float) delta, this.MaxSpeed);
	}
}
