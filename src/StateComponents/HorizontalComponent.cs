using System;
using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class HorizontalComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum MovementModeEnum : byte
	{
		PlayerControlled,
		InvertedPlayerInput,
		FacingDirection,
		InvertedFacingDirection,
		AlwaysLeft,
		AlwaysRight,
		SoftStop,
		HardStop,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Which directions the player is allowed to move the character. If either is disabled, the player cannot move the
	/// character to that direction.
	/// </summary>
	// [Export(PropertyHint.Flags, "1:Left,2:Right")] public byte Direction = 3;
	/// <summary>
	/// If either direction is set, the character will move automatically to that direction as if the player was
	/// constantly inputting the directional movement to that direction. Actual player input is ignored in this case.
	/// </summary>
	[Export] public MovementModeEnum MovementMode = MovementModeEnum.PlayerControlled;
	[Export] public float MaxSpeedPxPSec = 600f;
	[Export] public float AccelerationPxPSecSqr = 1200f;
	/// <summary>
	/// Deceleration when no input is given.
	/// </summary>
	[Export] public float SoftDecelerationPxPSecSqr = 1200f;
	/// <summary>
	/// Deceleration when current input is lower than the current speed or opposite to current movement direction.
	/// </summary>
	[Export] public float HardDecelerationPxPSecSqr = 2400f;

	[ExportGroup("Options")]
	/// <summary>
	/// If true, inverts the direction of movement.
	/// (useful to implement things like status effects that reverse controls)
	/// </summary>
	[Export] public bool InvertDirections = false;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 ResolvedInput
		=> this.MovementMode switch
			{
				MovementModeEnum.PlayerControlled => this.Character.InputManager.MovementInput,
				MovementModeEnum.InvertedPlayerInput => this.Character.InputManager.MovementInput * -1,
				MovementModeEnum.FacingDirection => Vector2.Right * this.Character.FacingDirection,
				MovementModeEnum.InvertedFacingDirection => Vector2.Right * this.Character.FacingDirection * -1,
				MovementModeEnum.AlwaysLeft => Vector2.Left,
				MovementModeEnum.AlwaysRight => Vector2.Right,
				_ => Vector2.Zero,
			}
			* (this.InvertDirections ? -1 : 1);

	// -----------------------------------------------------------------------------------------------------------------
	// PUBLIC METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		float targetVelocityX = this.MaxSpeedPxPSec * this.ResolvedInput.X;
		float accelerationX = (
			this.MovementMode == MovementModeEnum.SoftStop ? this.SoftDecelerationPxPSecSqr
			: this.MovementMode == MovementModeEnum.HardStop ? this.HardDecelerationPxPSecSqr
			: Math.Abs(this.Character.Velocity.X) < float.Epsilon
				|| Math.Abs(targetVelocityX) > Math.Abs(this.Character.Velocity.X)
				&& Math.Sign(targetVelocityX) == Math.Sign(this.Character.Velocity.X)
				? this.AccelerationPxPSecSqr
			: Math.Abs(this.ResolvedInput.X) < float.Epsilon ? this.SoftDecelerationPxPSecSqr
			: this.HardDecelerationPxPSecSqr
		) * (float) delta;
		this.Character.AccelerateX(targetVelocityX, accelerationX);
	}
}
