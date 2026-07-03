using System.Linq;
using Godot;

namespace Raele.Supercon.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_velocity_reset.png")]
public partial class VelocityResetComponent2D : SuperconStateComponent2D
{
	//==================================================================================================================
	// STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public AxisEnum Axis = AxisEnum.Both;
	[Export] public ModeEnum Mode = ModeEnum.Instant;
	[Export(PropertyHint.ExpEasing, "attenuation")] public float Easing = .5f;
	[Export(PropertyHint.None, "suffix:ms")] public int Duration = 200;
	[Export(PropertyHint.Range, "0.01,1")] public float LerpWeight = 0.05f;
	[Export] public Curve? Curve
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	private Vector2 InitialVelocity = Vector2.Zero;

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	public bool HorizontalAffected => this.Axis == AxisEnum.Both || this.Axis == AxisEnum.Horizontal;
	public bool VerticalAffected => this.Axis == AxisEnum.Both || this.Axis == AxisEnum.Vertical;

	//==================================================================================================================
	// SIGNALS
	//==================================================================================================================

	// [Signal] public delegate void EventHandler()

	//==================================================================================================================
	// INTERNAL TYPES
	//==================================================================================================================

	public enum AxisEnum : sbyte {
		Both = 0,
		Horizontal = 2,
		Vertical = 3,
	}

	public enum ModeEnum : sbyte {
		Instant = 0,
		Ease = 16,
		Lerp = 32,
		Curve = 48,
	}

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	public override string[] _GetConfigurationWarnings()
		=> new string[0]
			.Concat(
				this.Mode == ModeEnum.Curve && this.Curve == null
					? [$"Mode is set to {ModeEnum.Curve} but the {nameof(this.Curve)} property has not been assigned."]
					: []
			)
			.ToArray();
	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Mode):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.Easing):
			case nameof(this.Duration):
				property["usage"] = this.Mode == ModeEnum.Ease
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.None;
				break;
			case nameof(this.LerpWeight):
				property["usage"] = this.Mode == ModeEnum.Lerp
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.None;
				break;
			case nameof(this.Curve):
				property["usage"] = this.Mode == ModeEnum.Curve
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.None;
				break;
		}
	}

	protected override void _ActivityStarted(string mode, Variant argument)
	{
		base._ActivityStarted(mode, argument);
		this.InitialVelocity = this.Character?.Velocity ?? Vector2.Zero;
		this.SetPhysicsProcess(true);
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		if (this.ParentActivity?.ActiveTimeSpan.TotalMilliseconds > this.Duration - Mathf.Epsilon)
		{
			this.ZeroOutVelocity();
			return;
		}
		switch (this.Mode)
		{
			case ModeEnum.Instant:
				this.ProcessInstant();
				break;
			case ModeEnum.Ease:
				this.ProcessEase();
				break;
			case ModeEnum.Lerp:
				this.ProcessLerp();
				break;
			case ModeEnum.Curve:
				this.ProcessCurve();
				break;
		}
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	private void ProcessInstant()
		=> this.ZeroOutVelocity();

	private void ProcessEase()
	{
		if (this.ParentActivity == null)
			return;
		if (this.HorizontalAffected)
			this.Character?.VelocityX = this.InitialVelocity.X * Mathf.Ease(1 - (float) this.ParentActivity.ActiveTimeSpan.TotalMilliseconds / this.Duration, this.Easing);
		if (this.VerticalAffected)
			this.Character?.VelocityY = this.InitialVelocity.Y * Mathf.Ease(1 - (float) this.ParentActivity.ActiveTimeSpan.TotalMilliseconds / this.Duration, this.Easing);
	}

	private void ProcessLerp()
	{
		if (this.HorizontalAffected)
			this.Character?.VelocityX = Mathf.Lerp(this.Character.VelocityX, 0f, this.LerpWeight);
		if (this.VerticalAffected)
			this.Character?.VelocityY = Mathf.Lerp(this.Character.VelocityY, 0f, this.LerpWeight);
	}

	private void ProcessCurve()
	{
		if (this.ParentActivity == null || this.Curve == null)
			return;
		float progress = (float) this.ParentActivity.ActiveTimeSpan.TotalMilliseconds / this.Duration;
		float multiplier = this.Curve.SampleBaked(progress);
		if (this.HorizontalAffected)
			this.Character?.VelocityX = this.InitialVelocity.X * multiplier;
		if (this.VerticalAffected)
			this.Character?.VelocityY = this.InitialVelocity.Y * multiplier;
	}

	private void ZeroOutVelocity()
	{
		if (this.HorizontalAffected)
			this.Character?.VelocityX = 0f;
		if (this.VerticalAffected)
			this.Character?.VelocityY = 0f;
		this.SetPhysicsProcess(false);
	}
}
