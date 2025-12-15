using Godot;

namespace Raele.Supercon2D.StateComponents;

// TODO Implement braking behavior when trying to go to the opposite direction
// TODO Implement angular acceleration when changing direction
// TODO Implement different speeds and acceleration per axis, for isometric perspective
public partial class MultiAxisControlComponent : SuperconStateComponent
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

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);
		float currentVelocityPxPSec = this.Character.Velocity.Length();
		float targetVelocityPxPSec = this.InputMapping.MovementInput.Length() * this.MaxSpeedPxPSec;
		float accelerationPxPSecSqr = targetVelocityPxPSec > currentVelocityPxPSec
			? this.AccelerationPxPSecSqr
			: this.DecelerationPxPSecSqr;
		float newVelocity = Mathf.MoveToward(
			currentVelocityPxPSec,
			targetVelocityPxPSec,
			accelerationPxPSecSqr * (float) delta
		);
		this.Character.Velocity = this.InputMapping.MovementInput.Normalized() * newVelocity;
	}
}
