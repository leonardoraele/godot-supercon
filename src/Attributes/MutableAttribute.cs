using System;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class MutableAttribute : Resource, IAttribute
{
	//------------------------------------------------------------------------------------------------------------------
	// STATICS
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// EXPORTS
	//------------------------------------------------------------------------------------------------------------------

	[Export] public string Name
	{
		get;
		set { field = value; this.ResourceName = value; }
	} = "";
	/// <summary>
	/// If nil, the variable can assume any type.
	/// </summary>
	[Export] public Variant.Type Type { get; set; } = Variant.Type.Nil;
	[Export] public Variant DefaultValue = new Variant();

	[ExportGroup("Type Hint")]
	[Export] public PropertyHint Hint { get; set; } = PropertyHint.None;
	[Export] public string HintString
		{
			get;
			set {
				field = value;
				this.CallDebouncedRealTime(2f, GodotObject.MethodName.NotifyPropertyListChanged);
			}
		}
		= "";

	[ExportGroup("Use Setter Expression", "Setter")]
	[Export(PropertyHint.GroupEnable)] public bool SetterEnabled = false;
	[Export] public NodePath SetterContext = "";
	[Export] public Godot.Collections.Dictionary<string, Variant> SetterParams
		{ get; set { field = value; this.Interpreter = null!; }}
		= [];
	[Export(PropertyHint.Expression)] public string SetterExpression
		{ get; set { field = value; this.Interpreter = null!; }}
		= "";

	[ExportGroup("Comments")]
	[Export(PropertyHint.MultilineText)] public string Comments
	{
		get;
		set
		{
			if ((field.Length > Consts.MAX_SINGLE_LINE_COMMENT_LENGTH) != (value.Length > Consts.MAX_SINGLE_LINE_COMMENT_LENGTH))
				this.CallDebouncedRealTime(.5d, GodotObject.MethodName.NotifyPropertyListChanged);
			field = value;
		}
	 } = "";

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	public Expression? Interpreter;

	//------------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	//------------------------------------------------------------------------------------------------------------------

	bool IAttribute.IsReadOnly => false;

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Type):
			case nameof(this.Hint):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.DefaultValue):
				property["type"] = (long) this.Type;
				property["hint"] = (long) this.Hint;
				property["hint_string"] = this.HintString;
				break;
			case nameof(this.SetterContext):
				property["usage"] = this.ResourceLocalToScene
					? (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.NodePathFromSceneRoot
					: (long) PropertyUsageFlags.None;
				break;
			case nameof(this.Comments):
				property["hint"] = this.Comments.Length > Consts.MAX_SINGLE_LINE_COMMENT_LENGTH
					? (long) PropertyHint.MultilineText
					: (long) PropertyHint.None;
				break;
			default:
				if (property["name"].AsStringName() == Resource.PropertyName.ResourceLocalToScene)
					property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
		}
	}

	//------------------------------------------------------------------------------------------------------------------
	// METHODS
	//------------------------------------------------------------------------------------------------------------------

	// public Variant ApplyConstraints(IConflexStatsSource source,	Variant value)
	// 	=> this.Constraints.Aggregate(value, (current, constraint) => constraint.Apply(source, this, current));

	Variant IAttribute.RunGetter(IReadOnlyAttributeContainer source, Variant storedValue)
		=> storedValue.VariantType == Variant.Type.Nil ? this.DefaultValue
			: storedValue.VariantType == this.Type ? storedValue
			: storedValue.VariantType.IsConvertibleTo(this.Type) ? storedValue.As(this.Type)
			: this.Type.DefaultValue;

	Variant IAttribute.RunSetter(IReadOnlyAttributeContainer container, Variant value)
	{
		if (this.MustConvert(value.VariantType) && !this.TryConvert(ref value))
			throw new ArgumentException(
				string.Join(" ", [
						"Failed to validate value against GAS variable.",
						"Cause: Invalid input type.",
						$"Variable: \"{this.Name}\" (Type {this.Type}).",
						$"Input value: {Json.Stringify(value)} (Type {value.VariantType})."
					])
				);

		if (this.SetterEnabled)
		{
			value = this.ExecuteSetterExpression(container, value);
			if (this.MustConvert(value.VariantType) && !this.TryConvert(ref value))
				throw new ArgumentException(
					string.Join(" ", [
						"Failed to validate value against GAS variable.",
						"Cause: Invalid input type.",
						$"Variable: \"{this.Name}\" (Type {this.Type}).",
						$"Input value: {Json.Stringify(value)} (Type {value.VariantType})."
					])
				);
		}

		return value;
	}

	private bool MustConvert(Variant.Type inputType)
		=> this.Type != Variant.Type.Nil && inputType != this.Type;

	private bool TryConvert(ref Variant value)
	{
		bool convertible = value.VariantType.IsConvertibleTo(this.Type);
		if (!convertible)
		{
			GD.PushWarning(
				string.Join(" ", [
					"Failed to validate value against GAS variable.",
					"Cause: Invalid input type.",
					$"Variable: \"{this.Name}\" (Type {this.Type}).",
					$"Input value: {Json.Stringify(value)} (Type {value.VariantType})."
				])
			);
			return false;
		}
		value = value.As(this.Type);
		return convertible;
	}

	private Variant ExecuteSetterExpression(IReadOnlyAttributeContainer container, Variant input)
	{
		Godot.Collections.Dictionary<string, Variant> attributes = container.ToDictionary();

		if (this.Interpreter == null || Engine.IsEditorHint())
		{
			this.Interpreter = new();
			this.Interpreter.Parse(this.SetterExpression, [..attributes.Keys, ..this.SetterParams.Keys, "value"]);
		}

		Variant result = this.Interpreter.Execute(
			[..attributes.Values, ..this.SetterParams.Values, input],
			this.ResourceLocalToScene ? this.GetLocalScene().GetNode(this.SetterContext) : null
		);

		if (this.Interpreter?.HasExecuteFailed() == true)
		{
			GD.PushError(
				string.Join(" ", [
					"Failed to execute GAS variable constraint expression.",
					$"Error: \"{this.Interpreter.GetErrorText()}\".",
					$"Variable: \"{this.Name}\" (Type {this.Type}).",
					$"Setter Expression: \"{this.SetterExpression}\".",
					$"Input Value: {Json.Stringify(input)} (Type {input.VariantType}).",
					$"Current Stats: {Json.Stringify(container.ToDictionary())}.",
				])
			);
		}

		return result;
	}
}
