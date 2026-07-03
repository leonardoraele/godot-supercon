using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class SuperconBody2D : CharacterBody2D, ISuperconStateMachineOwner
{
	//==================================================================================================================
	// STATICs
	//==================================================================================================================

	public static readonly Vector2 DEFAULT_GROUNDED_FACING_DIRECTION = Vector2.Right;

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public SuperconInputController InputController
	{
		get => field ??= new();
		set;
	}

	[Export] public SuperconState? RestState
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }

	[ExportGroup("Facing")]
	/// <summary>
	/// If this field is set, the node will be flipped horizontally (if CharacterBody2D.motion_mode is GROUNDED) or
	/// rotated accordingly (if CharacterBody2D.motion_mode is FLOATING) based on the <see cref="FacingDirection"/> the
	/// character is facing at any given moment.
	/// </summary>
	[Export] public Node2D? FacingNode;
	/// <summary>
	/// If CharacterBody2D.motion_mode is set to FLOATING, then this field determines the direction where the FacingNode
	/// is facing at its resting pose. It will be used as a reference to calculate the rotation needed to make the
	/// FacingNode face the direction specified by the FacingDirection property.
	/// </summary>
	[Export] public Vector2 RestDirection = Vector2.Down;
	/// <summary>
	/// If this field is set to a Sprite2D or AnimatedSprote2D, the sprite will be flipped horizontally when the
	/// character is facing left.
	/// </summary>
	[Export(PropertyHint.NodeType, $"{nameof(Sprite2D)},{nameof(AnimatedSprite2D)}")] public Node? FlipSpriteH;

	// /// <summary>
	// /// If enabled, constantly updates the character's transform so that the positive X axis is aligned with the
	// /// character's facing direction.
	// /// </summary>
	// [Export] public bool TransformFollowsFacingDirection = false;

	[ExportGroup("Debug", "Debug")]
	[Export] public bool DebugPrintStateChanges
	{
		get => this.StateMachine.DebugPrintContext != null;
		set => this.StateMachine.DebugPrintContext = value ? this : null;
	}
	[ExportSubgroup("Teleport To Mouse", "DebugTeleportToMouse")]
	[Export(PropertyHint.GroupEnable)] public bool DebugTeleportToMouseEnabled = false;
	[Export(PropertyHint.InputName)] public string DebugTeleportToMouseInputAction = "ui_home";
	[Export] public bool DebugTeleportToMouseDraggable = false;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	/// <summary>
	/// Determines the direction the character is facing. Any value lower than 0 means the character is facing left,
	/// any value greater than 0 means the character is facing right, and 0 means the character is not facing any.
	/// A value of 0 might be used if, for example, the character is facing the camera or away from the camera.
	/// </summary>
	public Vector2 FacingDirection
		{
			get;
			set => field
				= this.MotionMode == MotionModeEnum.Grounded ? Vector2.Right * Math.Sign(value.X)
					: value.IsEqualApprox(Vector2.Zero) ? Vector2.Zero
					: value.Normalized();
		}
		= Vector2.Zero;

	public Vector2 LastOnFloorPosition { get; private set; }
	public TimeSpan TimeOnFloor { get; private set; } = TimeSpan.Zero;
	public TimeSpan TimeOnCeiling { get; private set; } = TimeSpan.Zero;
	public TimeSpan TimeOnWall { get; private set; } = TimeSpan.Zero;
	public SuperconStateMachine StateMachine { get; } = new();

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	public SuperconState? ActiveState => this.StateMachine.ActiveState;
	public bool IsOnSlope => this.IsOnFloor() && Math.Abs(this.GetFloorNormal().AngleTo(Vector2.Up)) > Mathf.Epsilon;

	public int HorizontalFacingDirection
	{
		get => Math.Sign(this.FacingDirection.X);
		set => this.FacingDirection = Vector2.Right * Math.Sign(value);
	}

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

	//==================================================================================================================
	// SIGNALS
	//==================================================================================================================

	[Signal] public delegate void StateChangedEventHandler(SuperconState? newState, SuperconState? oldState);

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	Node ISuperconStateMachineOwner.AsNode() => this;
	public ISuperconStateMachineOwner AsStateMachineOwner() => this;

	public override void _Ready()
	{
		base._Ready();
		this.FacingDirection = this.MotionMode == MotionModeEnum.Grounded ? DEFAULT_GROUNDED_FACING_DIRECTION
			: this.MotionMode == MotionModeEnum.Floating ? this.RestDirection
			: Vector2.Zero;
		this.AsStateMachineOwner().ResetState();
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			this.SetProcess(false);
			return;
		}
		base._Process(delta);
		this.InputController.Update();
		this.UpdateLastOnFloorPosition();
		this.UpdateFacing();
		this.UpdateContactTrackers(delta);
		this.UpdateDebugTeleportToMouse();
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

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.Concat(this.RestState == null ? [$"{nameof(this.RestState)} is not set. The character will not have a default state to start from."] : [])
			.ToArray();

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.RestDirection):
				property["usage"] = this.MotionMode == MotionModeEnum.Floating
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.None;
				break;
			default:
				if (property["name"].AsString() == CharacterBody2D.PropertyName.MotionMode)
				{
					property["usage"] = property["usage"].AsInt64() | (long) PropertyUsageFlags.UpdateAllIfModified;
				}
				break;
		}
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	private void UpdateLastOnFloorPosition()
	{
		if (this.IsOnFloor() && this.Position.DistanceSquaredTo(this.LastOnFloorPosition) > 64)
		{
			this.LastOnFloorPosition = this.Position;
		}
	}

	private void UpdateFacing()
	{
		if (this.FacingNode != null)
		{
			if (this.MotionMode == MotionModeEnum.Grounded)
			{
				// Godot doesn't directly support negative scaling, but we can achieve the same effect by directly
				// modifying the transform matrix. Note that, since we manipulated the transform directly, the Scale and
				// Rotation accessor properties will return incorrect values while the transform is flipped.
				// Specifically, Scale.X will be 1, Scale.Y will be -1, and Rotation will be 2*Pi. Besides the fact that
				// those properties become unreliable, the sprite is flipped correctly and child transforms are affected
				// as expected, so it seems to work fine.
				float scaleX = this.HorizontalFacingDirection == 0 ? 1 : this.HorizontalFacingDirection;
				float scaleY = 1;
				// This operation overrides scale, rotation, and skew of the transform, but translation is preserved.
				this.FacingNode.Transform = this.FacingNode.Transform with
				{
					X = new Vector2(scaleX, 0),
					Y = new Vector2(0, scaleY),
				};
			}
			else if (this.MotionMode == MotionModeEnum.Floating)
			{
				this.FacingNode.Rotation = this.FacingDirection.Angle() - this.RestDirection.Angle();
			}
		}
		this.FlipSpriteH?.Set("flip_h", this.HorizontalFacingDirection < 0);
	}

	private void UpdateContactTrackers(double delta)
	{
		this.TimeOnFloor = this.IsOnFloor() ? this.TimeOnFloor + TimeSpan.FromSeconds(delta) : TimeSpan.Zero;
		this.TimeOnCeiling = this.IsOnCeiling() ? this.TimeOnCeiling + TimeSpan.FromSeconds(delta) : TimeSpan.Zero;
		this.TimeOnWall = this.IsOnWall() ? this.TimeOnWall + TimeSpan.FromSeconds(delta) : TimeSpan.Zero;
	}

	private void UpdateDebugTeleportToMouse()
	{
		if (
			OS.IsDebugBuild()
			&& this.DebugTeleportToMouseEnabled
			&& this.DebugTeleportToMouseDraggable
				? Input.IsActionPressed(this.DebugTeleportToMouseInputAction)
				: Input.IsActionJustPressed(this.DebugTeleportToMouseInputAction)
		)
		{
			this.GlobalPosition = this.GetGlobalMousePosition();
			this.Velocity = Vector2.Zero;
		}
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
		if (Math.Sign(forcePxPSec.Dot(forceParallelVelocity)) < 0)
		{
			this.Velocity = addedVelocity;
			return;
		}
		Vector2 forceOrthogonalVelocity = addedVelocity - forceParallelVelocity;
		this.Velocity = forceParallelVelocity.Normalized() * Math.Min(forceParallelVelocity.Length(), maxSpeedPxPSec)
			+ forceOrthogonalVelocity;
	}

	/// <summary>
	/// Accelerates the character toward the given target velocity by rotating its velocity vector and changing its
	/// magnitude by the given angular and linear acceleration values.
	/// </summary>
	public void AccelerateArc(Vector2 targetVelocity, float angularRotationRad, float linearAccelerationPxPSec)
		=> this.Velocity =
			Vector2.Right.Rotated(
				Mathf.MoveToward(this.Velocity.Angle(), targetVelocity.Angle(), angularRotationRad)
			)
			* Mathf.MoveToward(this.Velocity.Length(), targetVelocity.Length(), linearAccelerationPxPSec);

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

	//==================================================================================================================
	// STATE MACHINE METHODS
	//==================================================================================================================

	// TODO // FIXME Causes infinite recursion
	// public void QueueTransition(string stateName, Variant data = default) => this.AsStateMachineOwner().QueueTransition(stateName, data);
	// public void QueueTransition(SuperconState? state, Variant data = default) => this.AsStateMachineOwner().QueueTransition(state, data);
	// public void ResetState() => this.AsStateMachineOwner().ResetState();
	// public void Stop() => this.AsStateMachineOwner().Stop();
}
