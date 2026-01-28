// using System;
// using Godot;

namespace Raele.Supercon2D.StateComponents;

// [GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_slope.png")]
public partial class SlopeComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	// [ExportGroup("Slope Up Resistance")]
	// [Export] public float SlopeDecelerationPxPSecSqr = 0f;
	// [Export] public float SlopeMaxSpeedPxPSec = float.PositiveInfinity;
	// [Export(PropertyHint.ExpEasing)] public float SlopeSpeedDeclineFactor = 1f;

	// [ExportGroup("Slope Down Acceleration")]
	// [Export] public float SlideAccelerationPxPSecSqr = 0f;
	// [Export] public float SlideMaxSpeedPxPSec = float.PositiveInfinity;

	// [ExportGroup("Options")]
	// [Export(PropertyHint.Range, "0,1,0.05")] public float NormalizationRate = 1f;

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _PhysicsProcessActive(double delta)
	// {
	// 	base._PhysicsProcessActive(delta);

	// 	if (this.Character.IsOnSlope && !this.Character.Velocity.IsZeroApprox())
	// 	{
	// 		Vector2 floorNormal = this.Character.GetFloorNormal();
	// 		float currentSpeed = this.Character.Velocity.Length();
	// 		float projectedSpeed = (this.Character.Velocity * Vector2.Right).Project(floorNormal.Rotated(Mathf.Pi / 2)).Length();
	// 		float normalizedSpeed = Mathf.Lerp(projectedSpeed, currentSpeed, this.NormalizationRate);
	// 		int directionH = Math.Sign(this.Character.Velocity.Dot(Vector2.Right)); // -1 = left, 1 = right
	// 		int directionV = Math.Sign((Vector2.Right * directionH).Dot(floorNormal)); // -1 = up, 1 = down
	// 		float targetSpeed = directionV switch
	// 		{
	// 			< 0 => Math.Min(normalizedSpeed, this.SlopeMaxSpeedPxPSec),
	// 			> 0 => Math.Max(normalizedSpeed, this.SlideMaxSpeedPxPSec),
	// 			_ => normalizedSpeed,
	// 		};
	// 		float acceleration = directionV switch
	// 		{
	// 			< 0 => this.SlopeDecelerationPxPSecSqr * (1f - floorNormal.Dot(Vector2.Up)),
	// 			> 0 => this.SlideAccelerationPxPSecSqr * Math.Abs(floorNormal.Dot(Vector2.Up)),
	// 			_ => 0f,
	// 		};
	// 		float newVelocity = this.Character.Velocity.Dot(Vector2.Up) >= 0
	// 			// Moving upward on the slope
	// 			? Mathf.MoveToward(
	// 				normalizedSpeed,
	// 				Math.Min(this.Character.Velocity.Length(), this.SlopeMaxSpeedPxPSec),
	// 				this.SlopeDecelerationPxPSecSqr
	// 					* (1f - floorNormal.Dot(Vector2.Up))
	// 					* (float) delta
	// 			)
	// 			// Moving downward on the slope
	// 			: Mathf.MoveToward(
	// 				normalizedSpeed,
	// 				Math.Max(this.Character.Velocity.Length(), this.SlideMaxSpeedPxPSec),
	// 				this.SlideAccelerationPxPSecSqr
	// 					* Math.Abs(floorNormal.Dot(Vector2.Up))
	// 					* (float) delta
	// 			);
	// 		Vector2 floorDirection = floorNormal.Rotated(Mathf.Pi / 2);
	// 		this.Character.Velocity = floorDirection
	// 			* newVelocity
	// 			* Math.Sign(this.Character.Velocity.Dot(floorDirection)) switch {
	// 				1 => 1,
	// 				-1 => -1,
	// 				_ => 1
	// 			};
	// 	}
	// }
}
