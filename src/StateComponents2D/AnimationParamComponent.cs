using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.Supercon2D.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_animation_param.png")]
public partial class AnimationParamComponent : SuperconStateComponent2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimationTree? AnimationTree;
	[Export(PropertyHint.EnumSuggestion)] public string ParameterName = "";
	[Export] public Node? TargetNode;
	[Export(PropertyHint.EnumSuggestion)] public string Property = "";

	[ExportGroup("Value Mapping")]
	[Export] public bool Absolute = false;
	[Export] public bool RadToDeg = false;
	[ExportSubgroup("Wrap", "Wrap")]
	[Export(PropertyHint.GroupEnable)] public bool WrapEnabled = false;
	[Export] public float WrapMin = 0;
	[Export] public float WrapMax = 1;
	[ExportSubgroup("Remap", "Remap")]
	[Export(PropertyHint.GroupEnable)] public bool RemapEnabled = false;
	[Export] public float RemapFromStart = 0;
	[Export] public float RemapFromEnd = 1;
	[Export] public float RemapToStart = 0;
	[Export] public float RemapToEnd = 1;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------


	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// public enum Enum : sbyte {
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
		{
			this.AnimationTree ??= this.Character?.GetChildren().OfType<AnimationTree>().FirstOrDefault();
			this.TargetNode ??= this.Character;
		}
	}

	protected override void _ActivityProcess(double delta)
	{
		base._ActivityProcess(delta);

		if (this.AnimationTree == null || string.IsNullOrWhiteSpace(this.ParameterName))
		{
			return;
		}

		this.AnimationTree.SetIndexed(this.ParameterName, this.GetPropertyValue());
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.Concat(true ? ["Some warning"] : [])
	// 		.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.AnimationTree):
			case nameof(this.TargetNode):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.ParameterName):
				property["hint_string"] = this.AnimationTree?.GetPropertyList()
					.Where(prop => (prop["usage"].AsInt64() & (long) PropertyUsageFlags.Editor) != 0)
					.Select(prop => prop["name"].AsString())
					.Where(prop => prop.StartsWith("parameters/"))
					.ToArray()
					.Join(",")
					?? "";
				break;
			case nameof(this.Property):
				List<string> properties = this.TargetNode?.GetPropertyList()
					.Select(prop => prop["name"].AsString())
					.Where(prop => !prop.StartsWith("_"))
					.ToList()
					?? [];
				properties.Sort();
				property["hint_string"] = string.Join(",", properties);
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private Variant GetPropertyValue()
		=> this.ProcessValue(this.GetPropertyValueRaw());

	private Variant GetPropertyValueRaw() => this.TargetNode?.GetIndexed(this.Property) ?? 0.0f;
	private Variant ProcessValue(Variant value)
	{
		if (this.Absolute)
		{
			value = Math.Abs(value.AsSingle());
		}
		if (this.RadToDeg)
		{
			value = Mathf.RadToDeg(value.AsSingle());
		}
		if (this.WrapEnabled)
		{
			value = Mathf.Wrap(value.AsSingle(), this.WrapMin, this.WrapMax);
		}
		if (this.RemapEnabled)
		{
			value = Mathf.Remap(value.AsSingle(), this.RemapFromStart, this.RemapFromEnd, this.RemapToStart, this.RemapToEnd);
		}
		return value;
	}
}
