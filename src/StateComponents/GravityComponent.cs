using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class GravityComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxDownwardSpeedPxPSec = 1000f;
	[Export] public float AccelerationPxPSecSqr = 50f;

	[ExportGroup("Options")]
	/// <summary>
	/// This is the downward velocity applied to the character when it is on the floor. It can be used to ensure the
	/// character stays grounded on fast slopes or moving platforms.
	/// </summary>
	[Export] public float OnFloorDownwardVelocityPxPSec = 0f;

	// -----------------------------------------------------------------------------------------------------------------
	// PUBLIC METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);

		if (this.Character.IsOnFloor())
		{
			if (this.Character.IsOnSlope)
			{
				this.Character.Velocity = (Vector2.Right * this.Character.Velocity).Rotated(this.Character.GetFloorAngle());
				// this.Character.VelocityY = this.Character.MovementSettings.DownwardVelocityOnFloor;
			}
			else
			{
				this.Character.VelocityY = this.OnFloorDownwardVelocityPxPSec;
			}
		}
		else
		{
			float velocityY = this.MaxDownwardSpeedPxPSec;
			float accelerationY = this.AccelerationPxPSecSqr;
			this.Character.AccelerateY(velocityY, accelerationY);
		}
	}
}
