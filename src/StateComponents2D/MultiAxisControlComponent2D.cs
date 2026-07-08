using Godot;

namespace Raele.Supercon.StateComponents2D;

// TODO Implement braking behavior when trying to go to the opposite direction
// TODO Implement angular acceleration when changing direction
// TODO Implement different speeds and acceleration per axis, for isometric perspective
[GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_multi_axis_control.png")]
public partial class MultiAxisControlComponent2D : SuperconStateComponent2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxSpeedPxPSec = 200f;
	[Export] public float AccelerationPxPSecSqr = 400f;
	[Export] public float DecelerationPxPSecSqr = 800f;

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
		float currentVelocityPxPSec = this.Character2D?.Velocity.Length() ?? 0;
		float targetVelocityPxPSec = this.Character2D?.InputController.RawDirectionalInput.Length() * this.MaxSpeedPxPSec ?? 0;
		float accelerationPxPSecSqr = targetVelocityPxPSec > currentVelocityPxPSec
			? this.AccelerationPxPSecSqr
			: this.DecelerationPxPSecSqr;
		float newVelocity = Mathf.MoveToward(
			currentVelocityPxPSec,
			targetVelocityPxPSec,
			accelerationPxPSecSqr * (float) delta
		);
		this.Character2D?.Velocity = this.Character2D?.InputController.RawDirectionalInput.Normalized() * newVelocity ?? Vector2.Zero;
	}
}
