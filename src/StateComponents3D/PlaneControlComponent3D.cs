using Godot;
using Raele.GodotUtils.Debug;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

/// <summary>
/// Allows the player to control the character using directional input to move it along a plane. e.g. To move the
/// character over the floor or climbing a wall.
///
/// You can set different speed values for forward, lateral, and backward movement.
///
/// This component does not not update the character's rotation direction. For that, use the FacingComponent.
/// </summary>
[Tool][GlobalClass]
public partial class PlaneControlComponent3D : SuperconStateComponent3D
{
	//==================================================================================================================
		#region STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EXPORTS
	//==================================================================================================================

	[Export] public PlaneOptionsEnum MovementPlane = PlaneOptionsEnum.Floor;
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxSpeed
		{ get; set { if (value != field) field = /*this.MaxForwardSpeed =*/ value.Clamped(0f, value); } }
		= 5f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Acceleration
		{ get; set => field = value.Clamped(0f, value); }
		= 10f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Deceleration
		{ get; set { if (value != field) field = /*this.DragDeceleration =*/ value.Clamped(0f, value); } }
		= 15f;
	[Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float AngularVelocity
	{
		get;
		set
		{
			if (value == field)
				return;
			// this.TurnMinAngularVelocity = this.TurnMinAngularVelocity.Clamped(0f, value);
			/*this.TurnMaxAngularVelocity =*/ field = value;
		}
	}
		= Mathf.Pi * 2;

	[ExportGroup("Rotate Character", "Rotation")]
	[Export(PropertyHint.GroupEnable)] public bool RotationEnabled = false;
	[Export] public Vector3 RotationPlaneNormalAlignment = Vector3.Up;
	[ExportSubgroup("Face Movement Direction", "Rotation")]
	[Export(PropertyHint.GroupEnable)] public bool RotationForwardEnabled = false;
	[Export] public Vector3 RotationForwardDirectionAlignment = Vector3.Forward;

	// [ExportGroup("Break MaxSpeed")]
	// [Export(PropertyHint.GroupEnable)] public bool MaxSpeedOptionsEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxForwardSpeed
	// 	{ get; set { if (value != field) field = this.MaxSpeed = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxLateralSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxBackwardSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxVerticalSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;

	// [ExportGroup("Break Deceleration")]
	// /// <summary>
	// /// Determines the deceleration behavior when the character's speed exceeds the maximum speed.
	// ///
	// /// If this is higher than <see cref="Deceleration"/>, the character will decelerate faster when above max speed.
	// /// </summary>
	// [Export(PropertyHint.GroupEnable)] public bool DecelerationOptionsEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float DragDeceleration
	// 	{ get; set { if (value != field) field = this.Deceleration = value.Clamped(0f, value); } }
	// 	= 15f;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float DecelerationAboveMaxSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;

	// [ExportGroup("Break Angular Velocity", "Turn")]
	// [Export(PropertyHint.GroupEnable)] public bool TurnEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// // TODO Implement dynamic range min/max using _ValidateProperty() instead of setter
	// [Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float TurnMinAngularVelocity
	// 	{ get; set { field = value.Clamped(0f, this.TurnMaxAngularVelocity); } }
	// 	= Mathf.Pi / 2;
	// // TODO Implement dynamic range min/max using _ValidateProperty() instead of setter
	// [Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float TurnMaxAngularVelocity
	// 	{ get; set { if (value != field) this.AngularVelocity = field = value.Clamped(this.TurnMinAngularVelocity, float.PositiveInfinity); } }
	// 	= Mathf.Pi * 2;
	// [Export(PropertyHint.None, "m/s²")] public float TurnLowerMovementSpeed
	// 	{ get; set { field = value.Clamped(0f, this.TurnUpperMovementSpeed); } }
	// 	= 0f;
	// [Export(PropertyHint.None, "m/s²")] public float TurnUpperMovementSpeed
	// 	{ get; set { field = value.Clamped(this.TurnLowerMovementSpeed, float.PositiveInfinity); } }
	// 	= 5f;
	// // [Export] public bool TurnExtrapolateBeyondMaxSpeed = false;
	// [Export(PropertyHint.ExpEasing, "attenuation")] public float TurnCurve = 1f;

	// [ExportGroup("Brake On Sharp Turns")]
	// [Export(PropertyHint.GroupEnable)] public bool SharpTurnOptionsEnabled = false;
	// [Export(PropertyHint.Range, "0,180,5,radians_as_degrees,suffix:°")] public float SoftBrakeTurnAngle = Mathf.Pi / 3;
	// [Export(PropertyHint.ExpEasing, "attenuation")] public float SoftBrakeVelocityLoss = 1f;
	// [Export(PropertyHint.Range, "0,180,5,radians_as_degrees,suffix:°")] public float HardBrakeTurnAngle = Mathf.Pi / 3;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float HardBrakeDeceleration
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 50f;

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region FIELDS
	//==================================================================================================================

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region COMPUTED PROPERTIES
	//==================================================================================================================

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EVENTS & SIGNALS
	//==================================================================================================================

	[Signal] public delegate void SurfaceExitEventHandler();

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region INTERNAL TYPES
	//==================================================================================================================

	public enum PlaneOptionsEnum : sbyte {
		Floor = 16,
		Wall = 20,
		Ceiling = 24,
		PlaneXZ = 64,
		PlaneXY = 65,
		PlaneYZ = 66,
		NegativePlaneXZ = 78,
		NegativePlaneXY = 79,
		NegativePlaneYZ = 80,
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region OVERRIDES & VIRTUALS
	//==================================================================================================================

	// public override string[] _GetConfigurationWarnings()
	// 	=> (base._GetConfigurationWarnings() ?? [])
	// 		.AppendIf(false "This node is not configured correctly. Did you forget to assign a required field?")
	// 		.ToArray();

	// public override void _ValidateProperty(Godot.Collections.Dictionary property)
	// {
	// 	base._ValidateProperty(property);
	// 	switch (property["name"].AsString())
	// 	{
	// 		case nameof(this.MaxSpeed):
	// 			if (this.MaxSpeedOptionsEnabled)
	// 				property["usage"] = (long) PropertyUsageFlags.None;
	// 			break;
	// 		case nameof(this.Deceleration):
	// 			if (this.DecelerationOptionsEnabled)
	// 				property["usage"] = (long) PropertyUsageFlags.None;
	// 			break;
	// 		case nameof(this.AngularVelocity):
	// 			if (this.AngularVelocityOptionsEnabled)
	// 				property["usage"] = (long) PropertyUsageFlags.None;
	// 			break;
	// 	}
	// }

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		if (this.Character == null)
			return;
		if (this.ResolveGlobalMovementPlane() is not Plane plane)
		{
			if (this.TestSurfaceExit())
				this.EmitSignalSurfaceExit();
			return;
		}
		if (this.Character.GetViewport().GetCamera3D() is not Camera3D camera)
			return;
		Vector3 projectedVelocity = this.Character.Velocity.Project(plane);
		float currentSpeed = projectedVelocity.Length();
		bool isMoving = currentSpeed > Mathf.Epsilon;
		Vector3 currentDirection = isMoving
			? projectedVelocity.Normalized()
			: this.Character.GlobalBasis.Forward;
		Vector2 normalInput = this.Character.InputController?.NormalDirectionalInput ?? Vector2.Zero;
		float inputStrength = normalInput.Length();
		bool hasInput = inputStrength > Mathf.Epsilon;
		Vector3 inputDirection = hasInput
			? camera.GlobalBasis.RotateToward(plane.Normal * -1)
				* new Vector3(normalInput.X, normalInput.Y * -1, 0)
				* plane.Normal.Dot(camera.GlobalBasis.Back).Sign()
			: Vector3.Zero;
		Vector3 newGlobalDirection = isMoving && hasInput
				? currentDirection.RotateToward(inputDirection, this.AngularVelocity * delta)
			: isMoving ? currentDirection
			: hasInput ? inputDirection
			: this.Character.Basis.Forward;
		// Vector3 newLocalDirection = this.Character.GlobalBasis * newGlobalDirection;
		float maxSpeed = /*this.MaxSpeedOptionsEnabled
			? new Vector3(
					newLocalDirection.X * this.MaxLateralSpeed,
					0,
					newLocalDirection.Z * (
						newLocalDirection.Z < 0
							? this.MaxForwardSpeed
							: this.MaxBackwardSpeed
					)
				)
				.Length()
			:*/ this.MaxSpeed;
		float targetSpeed = maxSpeed * inputStrength;
		float acceleration = targetSpeed > currentSpeed - Mathf.Epsilon
			? this.Acceleration
			: this.Deceleration;
		float newSpeed = currentSpeed.MoveToward(targetSpeed, acceleration * (float) delta);
		this.Character.Velocity = newGlobalDirection * newSpeed
			+ this.Character.Velocity.Project(plane.Normal);
		if (!this.RotationEnabled)
			return;
		Vector3 alignBack = this.RotationPlaneNormalAlignment.IsZeroApprox()
			? this.Character.Basis.Back
			: this.RotationPlaneNormalAlignment.Normalized();
		Vector3 alignUp = this.Character.GetGravity() is Vector3 gravity && gravity.IsZeroApprox()
			? this.Character.Basis.Up
			: gravity.Normalized() * -1;
		Basis alignment = !alignBack.IsParallelTo(alignUp)
			? Basis.LookingAt(-alignBack, alignUp)
			: Basis.Identity;
		this.Character.GlobalBasis = alignment * (
			this.RotationForwardEnabled
				? Basis.LookingAt(newGlobalDirection, plane.Normal)
				: Basis.LookingAt(plane.GetCenter() - this.Character.GlobalPosition, alignUp)
		);
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region METHODS
	//==================================================================================================================

	public Plane? ResolveGlobalMovementPlane()
		=> this.MovementPlane switch
		{
			PlaneOptionsEnum.Floor => this.Character?.IsOnFloor() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					? new Plane(collision.GetNormal(), collision.GetPosition())
					: null,
			PlaneOptionsEnum.Wall => this.Character?.IsOnWall() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					? new Plane(collision.GetNormal(), collision.GetPosition())
					: null,
			PlaneOptionsEnum.Ceiling => this.Character?.IsOnCeiling() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					? new Plane(collision.GetNormal(), collision.GetPosition())
					: null,
			PlaneOptionsEnum.PlaneXZ => Plane.PlaneXZ with { D = this.Character?.GlobalPosition.Length() ?? 0},
			PlaneOptionsEnum.PlaneXY => Plane.PlaneXY with { D = this.Character?.GlobalPosition.Length() ?? 0},
			PlaneOptionsEnum.PlaneYZ => Plane.PlaneYZ with { D = this.Character?.GlobalPosition.Length() ?? 0},
			PlaneOptionsEnum.NegativePlaneXZ => new Plane(Vector3.Down, this.Character?.GlobalPosition ?? Vector3.Zero),
			PlaneOptionsEnum.NegativePlaneXY => new Plane(Vector3.Forward, this.Character?.GlobalPosition ?? Vector3.Zero),
			PlaneOptionsEnum.NegativePlaneYZ => new Plane(Vector3.Left, this.Character?.GlobalPosition ?? Vector3.Zero),
			_ => null,
		};

	private bool TestSurfaceExit()
		// Note: We assume that if this method is being called, then the character is not on the surface anymore. This
		// method only tests if the character has exit the surface this frame.
		=> this.Character != null && this.MovementPlane switch
		{
			PlaneOptionsEnum.Floor => this.Character.TimeOnFloor > this.Character.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			PlaneOptionsEnum.Wall => this.Character.TimeOnWall > this.Character.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			PlaneOptionsEnum.Ceiling => this.Character.TimeOnCeiling > this.Character.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			_ => false,
		};

	//==================================================================================================================
		#endregion
	//==================================================================================================================
}
