using Godot;
using Raele.GodotUtils.Debug;
using Raele.GodotUtils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Raele.Supercon;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body-3d.png")]
public partial class SuperconBody3D : CharacterBody3D, ISuperconStateMachineOwner
{
	//==================================================================================================================
	// STATICs
	//==================================================================================================================

	public static readonly Vector3 DEFAULT_FACING_DIRECTION = Vector3.Forward;

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public SuperconState? RestState
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }

	/// <summary>
	/// Determines how directional inputs are handled when there are changes in camera angle.
	///
	/// This property changes how <see cref="GlobalMovementInput"/> is updated. If your character controls don't rely on
	/// global input, this property is irrelevant. For example, games that use "tank controls" don't rely on the camera
	/// perspective for character control.
	///
	/// For games that alternate bewtween different camera modes, (static and dynamic) it is recommended that this
	/// property is properly updated whenever a different camera mode is used. e.g. set this to StaticCamera in sections
	/// where the camera is static and then set it to DynamicCameraCut when the camera becomes dynamic again.
	/// </summary>
	[Export] public CameraModeEnum CameraMode = CameraModeEnum.DynamicCamera;

	[Export] public SuperconInputController? InputController;

	[ExportGroup("Debug", "Debug")]
	[Export] public bool DebugPrintStateChanges
	{
		get => this.StateMachine.DebugPrintContext != null;
		set => this.StateMachine.DebugPrintContext = value ? this : null;
	}
	[ExportSubgroup("3D Drawings", "Debug")]
	[Export] public bool DebugDrawInput = false;
	[Export] public bool DebugDrawVelocity = false;
	[Export] public bool DebugDrawCollisions = false;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	public readonly SuperconStateMachine StateMachine = new();

	/// <summary>
	/// Number of seconds the character has been on the floor. If negative, then it's the number of seconds since the
	/// character has left the floor.
	/// </summary>
	public float TimeOnFloor = float.NegativeInfinity;
	/// <summary>
	/// Number of seconds the character has been on the ceiling. If negative, then it's the number of seconds since the
	/// character has left the ceiling.
	/// </summary>
	public float TimeOnCeiling = float.NegativeInfinity;
	/// <summary>
	/// Number of seconds the character has been on a wall. If negative, then it's the number of seconds since the
	/// character has left the wall.
	/// </summary>
	public float TimeOnWall = float.NegativeInfinity;

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	/// <summary>
	/// This is the character's velocity relative to their basis of rotation.
	/// </summary>
	public Vector3 LocalVelocity {
		get => this.ToLocal(this.Velocity);
		set => this.Velocity = this.ToGlobal(value);
	}
	public float ForwardSpeed {
		get => this.LocalVelocity.Z * -1;
		set => this.LocalVelocity = this.LocalVelocity with { Z = value * -1 };
	}
	public float LateralSpeed {
		get => this.LocalVelocity.X;
		set => this.LocalVelocity = this.LocalVelocity with { X = value };
	}
	public float VerticalSpeed {
		get => this.Velocity.Y;
		set => this.Velocity = this.Velocity with { Y = value };
	}

	//==================================================================================================================
	// SIGNALS
	//==================================================================================================================

	// [Signal] public delegate void StateChangedEventHandler(SuperconState? newState, SuperconState? oldState);

	//==================================================================================================================
	// INTERNAL TYPES
	//==================================================================================================================

	public enum CameraModeEnum {
		/// <summary>
		/// In this mode, the camera angle will be considered at every frame. This means
		/// <see cref="GlobalMovementInput"/> will be updated every frame according to the currently active camera, even
		/// if the camera moves, rotates, or another camera becomes active, the input direction will be updated every
		/// frame based to the new camera parameters.
		///
		/// For example, if the player is pressing input Forward while the camera rotates, the input will point to the
		/// forward direction of the camera at every frame as it rotates. Likewise, if a camera cut happens, the input
		/// will point toward the forward direction of the new active camera.
		///
		/// This mode is best suited for games where the camera moves during gameplay, specially if the player is able
		/// to control the camera.
		///
		/// For games that feature dynamic cameras and also performs camera cuts, see <see cref="DynamicCameraCut"/>.
		/// </summary>
		DynamicCamera,
		/// <summary>
		/// In this mode, the InputController will remember the angle of the camera when the player starts a directional
		/// input and will update <see cref="GlobalMovementInput"/> every frame according to that fixed camera angle,
		/// even if the camera moves, rotates, or another camera becomes active while the player enters directional
		/// input.
		///
		/// This mode allows the player to keep their input direction even after a camera cut.
		///
		/// This mode is best suited for games that performs camera cuts between several static camera angles.
		/// </summary>
		StaticCamera,
		/// <summary>
		/// This mode behaves like StaticCamera, but the InputController will change this camera mode to
		/// <see cref="DynamicCamera"/> automatically when the player releases the directional input.
		///
		/// This mode is intended to be used when the game performs a camera cut to a dynamic camera. If this is is the
		/// case, change the camera mode to this mode every time a camera cut is performed.
		/// </summary>
		DynamicCameraCut,
	}

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	SuperconStateMachine ISuperconStateMachineOwner.StateMachine => this.StateMachine;
	Node ISuperconStateMachineOwner.AsNode() => this;

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.AppendIf(this.RestState == null, $"Mandatory field {nameof(this.RestState)} is not set.")
			.ToArray();

	public override void _Ready()
	{
		base._Ready();
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
		this.InputController?.Update();
		this.DebugDraw();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint())
		{
			this.SetPhysicsProcess(false);
			return;
		}
		base._PhysicsProcess(delta);
		this.UpdateContactTrackers(delta);
		this.CallDeferred(CharacterBody3D.MethodName.MoveAndSlide);
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	private void UpdateContactTrackers(double delta)
	{
		this.TimeOnFloor = this.IsOnFloor()
			? this.TimeOnFloor.AtLeast(0) + (float) delta
			: this.TimeOnFloor.AtMost(0) - (float) delta;
		this.TimeOnCeiling = this.IsOnCeiling()
			? this.TimeOnCeiling.AtLeast(0) + (float) delta
			: this.TimeOnCeiling.AtMost(0) - (float) delta;
		this.TimeOnWall = this.IsOnWall()
			? this.TimeOnWall.AtLeast(0) + (float) delta
			: this.TimeOnWall.AtMost(0) - (float) delta;
	}

	private void DebugDraw()
	{
		if (this.DebugDrawInput)
			Draw3D.AddText(nameof(this.InputController.RawDirectionalInput), this.InputController?.RawDirectionalInput ?? Variant.NULL);
		if (this.DebugDrawVelocity)
			Draw3D.DrawArrow(this.GlobalPosition, this.GlobalPosition + this.Velocity, Colors.Green);
		if (this.DebugDrawCollisions && this.GetLastSlideCollision() is KinematicCollision3D collision)
			Draw3D.DrawArrow(collision.GetPosition(), collision.GetPosition() + collision.GetNormal(), Colors.Red);
	}

	/// <summary>
	/// Applies the given force to the character's velocity, then limits the resulting velocity's magnitude along the
	/// direction of the force to the given maximum speed.
	/// </summary>
	/// <param name="force">The force to apply to the character's velocity.</param>
	/// <param name="maxSpeed">The maximum speed along the direction of the force, in meters per second.</param>
	public void ApplyForceAndLimitSpeed(Vector3 force, float maxSpeed)
	{
		if (force.IsZeroApprox())
			return;
		if (this.Velocity.IsZeroApprox())
		{
			this.Velocity = force.LimitLength(maxSpeed);
			return;
		}
		Vector3 parallelVelocity = this.Velocity.Project(force.Normalized());
		Vector3 orthogonalVelocity = this.Velocity - parallelVelocity;
		Vector3 newParallelVelocity = (parallelVelocity + force).LimitLength(maxSpeed);
		this.Velocity = orthogonalVelocity + newParallelVelocity;
	}
}
