using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class TransitionComponent : SuperconStateComponent
{
	//==================================================================================================================
	// STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public SuperconState? NextState;
	[Export] public StringName Condition = "";
	/// <summary>
	/// If true, the condition attribute will automatically be reset to false after the transition is
	/// triggered. This is useful for one-time triggers, such as a "jump" action.
	/// </summary>
	[Export] public bool ConditionIsTrigger = false;
	[Export(PropertyHint.Expression)] public string Expression = "";

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================



	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================



	//==================================================================================================================
	// SIGNALS
	//==================================================================================================================

	// [Signal] public delegate void EventHandler()

	//==================================================================================================================
	// INTERNAL TYPES
	//==================================================================================================================

	// public enum Type {
	// 	Value1,
	// }

	//==================================================================================================================
	// VIRTUALS & OVERRIDES
	//==================================================================================================================

	// public override string[] _GetConfigurationWarnings()
	// 	=> (base._GetConfigurationWarnings() ?? [])
	// 		.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Condition):
				string[] options = this.GetFirstAncestorOrDefault<FreakController3D>()
					?.CustomParameters
					.GetParameters()
					.Where(attr => attr.Type == Variant.Type.Bool)
					.Select(attr => attr.Name)
					.ToArray()
					?? [];
				property["hint"] = (long) PropertyHint.Enum;
				property["hint_string"] = string.Join(",", options);
				property["usage"] = (long) PropertyUsageFlags.Default
					| (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.ConditionIsTrigger):
				property["usage"] = !string.IsNullOrEmpty(this.Condition)
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.None;
				break;
		}
	}

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// }

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	//==================================================================================================================
	// METHODS
	//==================================================================================================================


}
