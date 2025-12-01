using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class SmoothSlopeComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportGroup("Slope Up Resistance")]
	[Export] public float SlopeDecelerationPxPSecSqr = 0f;
	[Export] public float SlopeMaxSpeedPxPSec = float.PositiveInfinity;

	[ExportGroup("Slope Down Acceleration")]
	[Export] public float SlideAccelerationPxPSecSqr = 0f;
	[Export] public float SlideMaxSpeedPxPSec = float.PositiveInfinity;

	[ExportGroup("Options")]
	[Export] public bool NormalizeSpeed = true;

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);

		if (this.Character.IsOnSlope)
		{
			float currentVelocity = this.Character.Velocity.Length();
			float projectedVelocity = this.NormalizeSpeed
				? currentVelocity
				: (this.Character.Velocity * Vector2.Right).Project(Vector2.Right.Rotated(this.Character.GetFloorAngle())).Length();
			float newVelocity = this.Character.Velocity.Dot(Vector2.Up) >= 0
				// Moving upward on the slope
				? Mathf.MoveToward(
					projectedVelocity,
					Math.Min(this.Character.Velocity.Length(), this.SlopeMaxSpeedPxPSec),
					this.SlopeDecelerationPxPSecSqr
						* Math.Abs(this.Character.GetFloorNormal().Dot(Vector2.Right))
						* (float) delta
				)
				// Moving downward on the slope
				: Mathf.MoveToward(
					projectedVelocity,
					Math.Max(this.Character.Velocity.Length(), this.SlideMaxSpeedPxPSec),
					this.SlideAccelerationPxPSecSqr
						* Math.Abs(this.Character.GetFloorNormal().Dot(Vector2.Right))
						* (float) delta
				);
			this.Character.Velocity = Vector2.Right.Rotated(this.Character.GetFloorAngle()) * newVelocity;
		}
	}
}
