using Godot;
using Raele.GodotUtils.Debug;
using Raele.GodotUtils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Raele.Supercon;

[Tool][GlobalClass][Icon($"res://{Consts.IconsDir}/character_body_neutral.png")]
public partial class FreakController3D : Node3D, ISuperconStateMachineOwner
{
	//==================================================================================================================
	// STATICS
	//==================================================================================================================

	public const string CUSTOM_ATTRIBUTE_PREFIX = "parameters/";

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public SuperconState? RestState
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }

	[Export] public SuperconInputController InputController = new();

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

	[Export] public FreakParameterProfile Parameters = new();

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

	public SuperconStateMachine StateMachine { get; } = new();

	/// <summary>
	/// Number of seconds the character has been on the floor. If negative, then it's the number of seconds
	/// since the character has left the floor.
	/// </summary>
	public double TimeOnFloorSec = double.NegativeInfinity;

	/// <summary>
	/// Number of seconds the character has been on the ceiling. If negative, then it's the number of seconds
	/// since the character has left the ceiling.
	/// </summary>
	public double TimeOnCeilingSec = double.NegativeInfinity;

	/// <summary>
	/// Number of seconds the character has been on a wall. If negative, then it's the number of seconds since
	/// the character has left the wall.
	/// </summary>
	public double TimeOnWallSec = double.NegativeInfinity;

	public FreakParameterContainer ParameterContainer
		=> field ??= new() { Prototype = this.Parameters };

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	public CharacterBody3D Character => this.GetParent<CharacterBody3D>();

	public Vector3 Velocity
	{
		get => this.Character.Velocity;
		set => this.Character.Velocity = value;
	}

	/// <summary>
	/// This is the character's velocity relative to their basis of rotation.
	/// </summary>
	public Vector3 LocalVelocity {
		get => this.ToLocal(this.Velocity);
		set => this.Velocity = this.ToGlobal(value);
	}

	public float LateralSpeed {
		get => this.LocalVelocity.X;
		set => this.LocalVelocity = this.LocalVelocity with { X = value };
	}

	public float VerticalSpeed {
		get => this.LocalVelocity.Y;
		set => this.LocalVelocity = this.LocalVelocity with { Y = value };
	}

	public float FrontalSpeed {
		get => this.LocalVelocity.Z * -1;
		set => this.LocalVelocity = this.LocalVelocity with { Z = value * -1 };
	}

	public bool IsOnFloor => this.Character.IsOnFloor();
	public bool IsOnCeiling => this.Character.IsOnCeiling();
	public bool IsOnWall => this.Character.IsOnWall();

	public double TimeAwayFromFloorSec => this.TimeOnFloorSec * -1;
	public double TimeAwayFromCeilingSec => this.TimeOnCeilingSec * -1;
	public double TimeAwayFromWallSec => this.TimeOnWallSec * -1;

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
		/// <see cref="GlobalMovementInput"/> will be updated every frame according to the currently active
		/// camera, even if the camera moves, rotates, or another camera becomes active, the input direction
		/// will be updated every frame based to the new camera parameters.
		///
		/// For example, if the player is pressing input Forward while the camera rotates, the input will
		/// point to the forward direction of the camera at every frame as it rotates, meaning the character
		/// will rotate along with the camera
		///
		/// Likewise, if a camera cut happens, the input will point toward the forward direction of the new
		/// active camera, meaning the character will turn to the new camera direction even though the player
		/// has not changed the input.
		///
		/// This mode is best suited for games where the camera moves during gameplay, specially if the player
		/// is able to control the camera.
		///
		/// For games that feature dynamic cameras and also performs camera cuts, see
		/// <see cref="DynamicCameraCut"/>.
		/// </summary>
		DynamicCamera,

		/// <summary>
		/// In this mode, the InputController will remember the angle of the camera when the player starts a
		/// directional input and will update <see cref="GlobalMovementInput"/> every frame according to that
		/// fixed camera angle, even if the camera moves, rotates, or another camera becomes active while the
		/// player enters directional input.
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
		/// This mode is intended to be used when the game performs a camera cut to a dynamic camera. If this
		/// is the case, change the camera mode to this mode every time a camera cut is performed.
		/// </summary>
		DynamicCameraCut,
	}

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.AppendIf(this.GetParent() is not CharacterBody3D, $"The {nameof(FreakController3D)} node must be a direct child of a {nameof(CharacterBody3D)} node.")
			.AppendIf(this.RestState == null, $"Mandatory field {nameof(this.RestState)} is not set.")
			.ToArray();

	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
		=> (base._GetPropertyList() ?? [])
			.Concat(
				this.Parameters?.GetParameters()
					.Select(attr => new Godot.Collections.Dictionary()
					{
						["name"] = CUSTOM_ATTRIBUTE_PREFIX + attr.Name,
						["type"] = (long) attr.Type,
						["hint"] = (long) attr.Hint,
						["hint_string"] = attr.HintString,
						["usage"] = (long) PropertyUsageFlags.Default,
					})
					?? []
			)
			.ToGodotArrayT();

	public override Variant _Get(StringName property)
	{
		if (property.ToString().StartsWith(CUSTOM_ATTRIBUTE_PREFIX))
		{
			string attributeName = property.ToString().Substring(CUSTOM_ATTRIBUTE_PREFIX.Length);
			return this.ParameterContainer.GetParameterValue(attributeName);
		}
		return new Variant();
	}

	public override bool _Set(StringName property, Variant value)
	{
		if (property.ToString().StartsWith(CUSTOM_ATTRIBUTE_PREFIX))
		{
			string attributeName = property.ToString().Substring(CUSTOM_ATTRIBUTE_PREFIX.Length);
			this.ParameterContainer.SetParameterValue(attributeName, value);
			return true;
		}
		return false;
	}

	public override Variant _PropertyGetRevert(StringName property)
	{
		if (property.ToString().StartsWith(CUSTOM_ATTRIBUTE_PREFIX) && this.Parameters != null)
		{
			string attributeName = property.ToString().Substring(CUSTOM_ATTRIBUTE_PREFIX.Length);
			return this.Parameters.GetParameterValue(attributeName);
		}
		return base._PropertyGetRevert(property);
	}

	public override bool _PropertyCanRevert(StringName property)
	{
		if (property.ToString().StartsWith(CUSTOM_ATTRIBUTE_PREFIX))
			return !this.Get(property).Equals(this.PropertyGetRevert(property));
		return false;
	}

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
			return;
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
		this.Character.CallDeferred(CharacterBody3D.MethodName.MoveAndSlide);
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	private void UpdateContactTrackers(double delta)
	{
		this.TimeOnFloorSec = this.IsOnFloor
			? this.TimeOnFloorSec.AtLeast(0) + delta
			: this.TimeOnFloorSec.AtMost(0) - delta;
		this.TimeOnCeilingSec = this.IsOnCeiling
			? this.TimeOnCeilingSec.AtLeast(0) + delta
			: this.TimeOnCeilingSec.AtMost(0) - delta;
		this.TimeOnWallSec = this.IsOnWall
			? this.TimeOnWallSec.AtLeast(0) + delta
			: this.TimeOnWallSec.AtMost(0) - delta;
	}

	private void DebugDraw()
	{
		if (this.DebugDrawInput)
			Draw3D.AddText(nameof(this.InputController.RawDirectionalInput), this.InputController?.RawDirectionalInput ?? Variant.NULL);
		if (this.DebugDrawVelocity)
			Draw3D.DrawArrow(this.Character.GlobalPosition, this.Character.GlobalPosition + this.Velocity, Colors.Green);
		if (this.DebugDrawCollisions && this.Character.GetLastSlideCollision() is KinematicCollision3D collision)
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
