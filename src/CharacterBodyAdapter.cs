// using Godot;
// using Godot.Collections;

// namespace Raele.Supercon;

// public partial class CharacterBodyAdapter : GodotObject
// {
// 	public CharacterBodyAdapter(CharacterBody2D characterBody)
// 		=> this.BackingCharacterBody = characterBody;
// 	public CharacterBodyAdapter(CharacterBody3D characterBody)
// 		=> this.BackingCharacterBody = characterBody;

// 	private Node BackingCharacterBody;

// 	public CharacterBody2D? AsCharacterBody2D()
// 		=> this.BackingCharacterBody as CharacterBody2D;
// 	public CharacterBody3D? AsCharacterBody3D()
// 		=> this.BackingCharacterBody as CharacterBody3D;

// 	public override Variant _Get(StringName property)
// 		=> this.BackingCharacterBody.Get(property);
// 	public override bool _Set(StringName property, Variant value)
// 	{
// 		this.BackingCharacterBody.Set(property, value);
// 		return true;
// 	}
// 	public override Array<Dictionary> _GetPropertyList()
// 		=> this.BackingCharacterBody.GetPropertyList();

// 	// public bool FloorBlockOnWall
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("floor_block_on_wall").AsBool();
// 	// 	set => this.BackingCharacterBody.Set("floor_block_on_wall", value);
// 	// }
// 	// public bool FloorConstantSpeed
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("floor_constant_speed").AsBool();
// 	// 	set => this.BackingCharacterBody.Set("floor_constant_speed", value);
// 	// }
// 	// public double FloorMaxAngle
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("floor_max_angle").AsDouble();
// 	// 	set => this.BackingCharacterBody.Set("floor_max_angle", value);
// 	// }
// 	// public double FloorSnapLength
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("floor_snap_length").AsDouble();
// 	// 	set => this.BackingCharacterBody.Set("floor_snap_length", value);
// 	// }
// 	// public bool FloorStopOnSlope
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("floor_stop_on_slope").AsBool();
// 	// 	set => this.BackingCharacterBody.Set("floor_stop_on_slope", value);
// 	// }
// 	// public long MaxSlides
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("max_slides").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("max_slides", value);
// 	// }
// 	// public long MotionModeRaw
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("motion_mode").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("motion_mode", value);
// 	// }
// 	// public CharacterBody2D.MotionModeEnum MotionMode2D
// 	// {
// 	// 	get => (CharacterBody2D.MotionModeEnum)
// 	// 		this.BackingCharacterBody.Get("motion_mode").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("motion_mode", (long) value);
// 	// }
// 	// public CharacterBody3D.MotionModeEnum MotionMode3D
// 	// {
// 	// 	get => (CharacterBody3D.MotionModeEnum)
// 	// 		this.BackingCharacterBody.Get("motion_mode").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("motion_mode", (long) value);
// 	// }
// 	// public long PlatformFloorLayers
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("platform_floor_layers").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("platform_floor_layers", value);
// 	// }
// 	// public long PlatformOnLeaveRaw
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("platform_on_leave").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("platform_on_leave", value);
// 	// }
// 	// public CharacterBody2D.PlatformOnLeaveEnum PlatformOnLeave2D
// 	// {
// 	// 	get => (CharacterBody2D.PlatformOnLeaveEnum)
// 	// 		this.BackingCharacterBody.Get("platform_on_leave").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("platform_on_leave", (long) value);
// 	// }
// 	// public CharacterBody3D.PlatformOnLeaveEnum PlatformOnLeave3D
// 	// {
// 	// 	get => (CharacterBody3D.PlatformOnLeaveEnum)
// 	// 		this.BackingCharacterBody.Get("platform_on_leave").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("platform_on_leave", (long) value);
// 	// }
// 	// public long PlatformWallLayers
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("platform_wall_layers").AsInt64();
// 	// 	set => this.BackingCharacterBody.Set("platform_wall_layers", value);
// 	// }
// 	// public double SafeMargin
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("safe_margin").AsDouble();
// 	// 	set => this.BackingCharacterBody.Set("safe_margin", value);
// 	// }
// 	// public bool SlideOnCeiling
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("slide_on_ceiling").AsBool();
// 	// 	set => this.BackingCharacterBody.Set("slide_on_ceiling", value);
// 	// }
// 	// public Vector3 UpDirection
// 	// {
// 	// 	get => this.ForceToVector3(this.BackingCharacterBody.Get("up_direction"));
// 	// 	set => this.BackingCharacterBody.Set("up_direction", value);
// 	// }
// 	// public Vector3 Velocity
// 	// {
// 	// 	get => this.ForceToVector3(this.BackingCharacterBody.Get("velocity"));
// 	// 	set => this.BackingCharacterBody.Set("velocity", value);
// 	// }
// 	// public double WallMinSlideAngle
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("wall_min_slide_angle").AsDouble();
// 	// 	set => this.BackingCharacterBody.Set("wall_min_slide_angle", value);
// 	// }
// 	// public bool InputPickable
// 	// {
// 	// 	get => this.BackingCharacterBody.Get("input_pickable").AsBool();
// 	// 	set => this.BackingCharacterBody.Set("input_pickable", value);
// 	// }

// 	// public void ApplyFloorSnap()
// 	// 	=> this.BackingCharacterBody.Call("apply_floor_snap");
// 	// public double GetFloorAngle()
// 	// 	=> this.AsCharacterBody2D()?.GetFloorAngle()
// 	// 		?? this.AsCharacterBody3D()!.GetFloorAngle();
// 	// public double GetFloorAngle(Vector2 upDirection)
// 	// 	=> this.AsCharacterBody2D()!.GetFloorAngle(upDirection);
// 	// public double GetFloorAngle(Vector3 upDirection)
// 	// 	=> this.AsCharacterBody3D()!.GetFloorAngle(upDirection);
// 	// public Vector3 GetFloorNormal()
// 	// 	=> this.ForceToVector3(this.BackingCharacterBody.Call("get_floor_normal"));
// 	// public Vector3 GetLastMotion()
// 	// 	=> this.ForceToVector3(this.BackingCharacterBody.Call("get_last_motion"));
// 	// public Vector3 GetPlatformVelocity()
// 	// 	=> this.ForceToVector3(this.BackingCharacterBody.Call("get_platform_velocity"));
// 	// public Vector3 GetPositionDelta()
// 	// 	=> this.ForceToVector3(this.BackingCharacterBody.Call("get_position_delta"));
// 	// public Vector3 GetRealVelocity()
// 	// 	=> this.ForceToVector3(this.BackingCharacterBody.Call("get_real_velocity"));
// 	// public double GetSlideCollisionCount()
// 	// 	=> this.BackingCharacterBody.Call("get_slide_collision_count").AsInt64();
// 	// public Vector3 GetWallNormal()
// 	// 	=> this.ForceToVector3(this.BackingCharacterBody.Call("get_wall_normal"));
// 	// public bool IsOnCeiling()
// 	// 	=> this.BackingCharacterBody.Call("is_on_ceiling").AsBool();
// 	// public bool IsOnCeilingOnly()
// 	// 	=> this.BackingCharacterBody.Call("is_on_ceiling_only").AsBool();
// 	// public bool IsOnFloor()
// 	// 	=> this.BackingCharacterBody.Call("is_on_floor").AsBool();
// 	// public bool IsOnFloorOnly()
// 	// 	=> this.BackingCharacterBody.Call("is_on_floor_only").AsBool();
// 	// public bool IsOnWall()
// 	// 	=> this.BackingCharacterBody.Call("is_on_wall").AsBool();
// 	// public bool IsOnWallOnly()
// 	// 	=> this.BackingCharacterBody.Call("is_on_wall_only").AsBool();
// 	// public bool MoveAndSlide()
// 	// 	=> this.BackingCharacterBody.Call("move_and_slide").AsBool();
// 	// public void AddCollisionExceptionWith(Node body)
// 	// 	=> this.BackingCharacterBody.Call("add_collision_exception_with", body);
// 	// // public PhysicsBody2D[] GetCollisionExceptions()
// 	// // 	=> this.BackingCharacterBody.Call("get_collision_exceptions").AsPhysicsBody2D();
// 	// public Vector3 GetGravity()
// 	// 	=> this.ForceToVector3(this.BackingCharacterBody.Call("get_gravity"));
// 	// // public KinematicCollision2D MoveAndCollide(Vector2 motion, bool test_only = false, double safe_margin = 0.08, bool recovery_as_collision = false)
// 	// // 	=> this.BackingCharacterBody.Call("move_and_collide").AsKinematicCollision2D();
// 	// public void RemoveCollisionExceptionWith(Node body)
// 	// 	=> this.BackingCharacterBody.Call("remove_collision_exception_with", body);
// 	// public bool TestMove(Transform2D from, Vector2 motion, KinematicCollision2D? collision = null, double safe_margin = 0.08, bool recovery_as_collision = false)
// 	// 	=> this.BackingCharacterBody.Call("test_move", from, motion, Variant.From(collision), safe_margin, recovery_as_collision).AsBool();

// 	// private Vector3 ForceToVector3(Variant value)
// 	// {
// 	// 	if (value.VariantType == Variant.Type.Vector3)
// 	// 		return value.AsVector3();
// 	// 	(float x, float y) = value.AsVector2();
// 	// 	return new Vector3(x, y, 0);
// 	// }
// }
