using Godot;
using System;
using System.Linq;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconBody2D : CharacterBody2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public SuperconInputMapping InputMapping
	{
		get => field ??= new();
		set => field = value;
	}

	[Export] public SuperconState? DefaultState;

	[ExportGroup("Horizontal Flipping When Facing Left", "FlipH")]
	[Export] public Node2D? FlipHFlipNodeScaleX;
	[Export(PropertyHint.NodeType, $"{nameof(Sprite2D)},{nameof(AnimatedSprite2D)}")] public Node? FlipHFlipSprite;

	// /// <summary>
	// /// If enabled, constantly updates the character's transform so that the positive X axis is aligned with the
	// /// character's facing direction.
	// /// </summary>
	// [Export] public bool TransformFollowsFacingDirection = false;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void StateChangedEventHandler(SuperconState newState, SuperconState? oldState);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector2 LastOnFloorPosition { get; private set; }
	public TimeSpan TimeOnFloor { get; private set; } = TimeSpan.Zero;
	public TimeSpan TimeOnCeiling { get; private set; } = TimeSpan.Zero;
	public TimeSpan TimeOnWall { get; private set; } = TimeSpan.Zero;
	public SuperconStateMachine StateMachine = new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsFacingLeft => this.FacingDirection < 0;
	public bool IsFacingRight => this.FacingDirection > 0;
	public bool IsFacingNeutral => this.FacingDirection == 0;
	public bool IsOnSlope => this.IsOnFloor() && Math.Abs(this.GetFloorNormal().AngleTo(Vector2.Up)) > Mathf.Epsilon;

	/// <summary>
	/// Determines the direction the character is facing. Any value lower than 0 means the character is facing left,
	/// any value greater than 0 means the character is facing right, and 0 means the character is not facing any.
	/// A value of 0 might be used if, for example, the character is facing the camera or away from the camera.
	/// </summary>
	public int FacingDirection { get; set => field = Math.Sign(value); } = 1;

	public float VelocityX
	{
		get => this.Velocity.X;
		set => this.Velocity = new Vector2(value, this.Velocity.Y);
	}

	public float VelocityY
	{
		get => this.Velocity.Y;
		set => this.Velocity = new Vector2(this.Velocity.X, value);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void StaeChangedEventHandler(SuperconState? newState, SuperconState? oldState);

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.ResetState();
		this.StateMachine.TransitionCompleted += transition =>
		{
			GD.PrintS(Time.GetTicksMsec(), $"[{nameof(SuperconBody2D)}] 🔀 State changed: {transition.StateOut?.Name ?? "<null>"} → {transition.StateIn?.Name ?? "<null>"}");
			this.EmitSignalStateChanged(transition.StateIn, transition.StateOut);
		};
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			if (this.DefaultState == null)
			{
				this.DefaultState = this.GetChildren().OfType<SuperconState>().FirstOrDefault();
			}
			else
			{
				this.SetProcess(false);
			}
			return;
		}
		base._Process(delta);
		this.InputMapping.Update();
		this.UpdateLastOnFloorPosition();
		this.UpdateFacing();
		this.UpdateContactTrackers(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint())
		{
			this.SetPhysicsProcess(false);
			return;
		}
		base._PhysicsProcess(delta);
		this.CallDeferred(MethodName.MoveAndSlide);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void UpdateLastOnFloorPosition()
	{
		if (this.IsOnFloor() && this.Position.DistanceSquaredTo(this.LastOnFloorPosition) > 64)
		{
			this.LastOnFloorPosition = this.Position;
		}
	}

	private void UpdateFacing()
	{
		if (
			Math.Abs(this.Velocity.X) > Mathf.Epsilon
			&& Math.Abs(this.InputMapping.MovementInput.X) > Mathf.Epsilon
			&& Math.Sign(this.Velocity.X) == Math.Sign(this.InputMapping.MovementInput.X)
		)
		{
			this.FacingDirection = Math.Sign(this.Velocity.X);
		}
		this.FlipHFlipNodeScaleX?.Scale = new Vector2(
			Math.Abs(this.FlipHFlipNodeScaleX.Scale.X) * -this.FacingDirection,
			this.FlipHFlipNodeScaleX.Scale.Y
		);
		this.FlipHFlipSprite?.Set("flip_h", this.IsFacingLeft);
	}

	private void UpdateContactTrackers(double delta)
	{
		this.TimeOnFloor = this.IsOnFloor() ? this.TimeOnFloor + TimeSpan.FromSeconds(delta) : TimeSpan.Zero;
		this.TimeOnCeiling = this.IsOnCeiling() ? this.TimeOnCeiling + TimeSpan.FromSeconds(delta) : TimeSpan.Zero;
		this.TimeOnWall = this.IsOnWall() ? this.TimeOnWall + TimeSpan.FromSeconds(delta) : TimeSpan.Zero;
	}

	/// <summary>
	/// Applies the given force to the character's velocity.
	/// </summary>
	public void ApplyForce(Vector2 forcePxPSec) => this.Velocity += forcePxPSec;

	/// <summary>
	/// Applies the given force to the character's velocity, then limits the resulting velocity's magnitude along the
	/// direction of the force to the given maximum speed.
	/// </summary>
	public void ApplyForce(Vector2 forcePxPSec, float maxSpeedPxPSec)
	{
		Vector2 addedVelocity = this.Velocity + forcePxPSec;
		Vector2 forceParallelVelocity = addedVelocity
			* new Vector2(
				(float) Math.Cos(Math.Atan2(forcePxPSec.Y, forcePxPSec.X)),
				(float) Math.Sin(Math.Atan2(forcePxPSec.Y, forcePxPSec.X))
			)
			.Abs();
		Vector2 forceOrthogonalVelocity = addedVelocity - forceParallelVelocity;
		this.Velocity = forceParallelVelocity.Normalized()
			* Math.Min(forceParallelVelocity.Length(), maxSpeedPxPSec)
			+ forceOrthogonalVelocity;
	}

	/// <summary>
	/// Accelerates the character toward the given target velocity by rotating its velocity vector and changing its
	/// magnitude by the given angular and linear acceleration values.
	/// </summary>
	public void AccelerateArc(Vector2 targetVelocity, float angularRotationRad, float linearAccelerationPxPSec)
		=> this.Velocity =
			Vector2.Right.Rotated(
				Mathf.MoveToward(targetVelocity.Angle(), this.Velocity.Angle(), angularRotationRad)
			)
			* Mathf.MoveToward(targetVelocity.Length(), this.Velocity.Length(), linearAccelerationPxPSec);

	// /// <summary>
	// /// Accelerates the character toward the given target velocity. The acceleration is applied to each axis.
	// /// If the character is already moving faster than the target velocity, the acceleration is applied in the opposite
	// /// direction to slow down the character.
	// /// If the character is already moving at the target velocity, no acceleration is applied.
	// ///
	// /// Params <code>accelerationX</code> and <code>accelerationY</code> must be positive. If they are negative, the
	// /// character will accelerate away from the target velocity.
	// ///
	// /// This method only changes the character's Velocity. You still have to call <code>MoveAndSlide</code> or similar
	// /// to actually move the character.
	// /// </summary>
	// public void AccelerateXY(float targetVelocityX, float targetVelocityY, float accelerationX, float accelerationY)
	// 	=> this.Velocity = new Vector2(
	// 		Mathf.MoveToward(this.Velocity.X, targetVelocityX, accelerationX),
	// 		Mathf.MoveToward(this.Velocity.Y, targetVelocityY, accelerationY)
	// 	);

	/// <summary>
	/// Changes the X velocity by moving it toward the target X velocity by the given acceleration.
	/// The character's Y velocity is not affected.
	/// </summary>
	public void AccelerateX(float targetVelocityX, float accelerationX)
	{
		this.Velocity = new Vector2(
			Mathf.MoveToward(this.Velocity.X, targetVelocityX, accelerationX),
			this.Velocity.Y
		);
	}

	/// <summary>
	/// Changes the Y velocity by moving it toward the target Y velocity by the given acceleration.
	/// The character's X velocity is not affected.
	/// </summary>
	public void AccelerateY(float targetVelocityY, float accelerationY)
	{
		this.Velocity = new Vector2(
			this.Velocity.X,
			Mathf.MoveToward(this.Velocity.Y, targetVelocityY, accelerationY)
		);
	}

	public void AccelerateDirection(Vector2 direction, float targetSpeedPxPSec, float accelerationPxPSec)
	{
		this.SetDirectionalVelocity(
			direction,
			Mathf.MoveToward(
				this.GetDirectionalVelocity(direction).Length(),
				targetSpeedPxPSec,
				accelerationPxPSec
			)
		);
	}

	public void SetDirectionalVelocity(Vector2 velocity)
		=> this.Velocity = velocity
			+ this.GetDirectionalVelocity(velocity.Orthogonal());

	public void SetDirectionalVelocity(Vector2 direction, float magnitude)
		=> this.Velocity = direction.Normalized() * magnitude
			+ this.GetDirectionalVelocity(direction.Orthogonal());

	public Vector2 GetDirectionalVelocity(Vector2 direction)
		=> this.Velocity
			* new Vector2(
				(float) Math.Cos(Math.Atan2(direction.Y, direction.X)),
				(float) Math.Sin(Math.Atan2(direction.Y, direction.X))
			)
			.Abs();

	// -----------------------------------------------------------------------------------------------------------------
	// STATE MACHINE PROXY METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void ResetState() => this.StateMachine.QueueTransition(this.DefaultState);
	public void QueueTransition(string stateName) => this.StateMachine.QueueTransition(this.GetParent().GetNode(stateName) as SuperconState);
	public void QueueTransition(SuperconState state) => this.StateMachine.QueueTransition(state);
}
