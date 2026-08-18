using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class ComputedAttribute : Resource, IAttribute
{
	//==================================================================================================================
	// STATICS
	//==================================================================================================================

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public string Name
	{
		get;
		set { field = value; this.ResourceName = value; }
	} = "";
	[Export] public NodePath Context { get; set; } = ".";
	[Export] public Godot.Collections.Dictionary<string, Variant> Params
		{ get; set { field = value; this.Interpreter = null; } }
		= [];
	[Export(PropertyHint.Expression)] public string Expression
		{ get; set { field = value; this.Interpreter = null; } }
		= "";
	[Export] public string Comments
	{
		get;
		set
		{
			if ((field.Length > Consts.MAX_SINGLE_LINE_COMMENT_LENGTH) != (value.Length > Consts.MAX_SINGLE_LINE_COMMENT_LENGTH))
				this.CallDebouncedRealTime(.5d, GodotObject.MethodName.NotifyPropertyListChanged);
			field = value;
		}
	 } = "";

	 [ExportGroup("Type Checking")]
	 [Export] public Variant.Type ExpectedType = Variant.Type.Nil;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	/// <summary>
	/// Flag to prevent infinite loops when a ComputedVariable's ComputedValue depends (directly or indirectly) on
	/// itself.
	/// </summary>
	private bool ProcessingValue = false;
	private Expression? Interpreter;

	//==================================================================================================================
	// VIRTUALS & OVERRIDES
	//==================================================================================================================

	Variant.Type IAttribute.Type => this.ExpectedType;
	PropertyHint IAttribute.Hint => PropertyHint.None;
	string IAttribute.HintString => "";
	bool IAttribute.IsReadOnly => true;

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.ResourceLocalToScene):
				property["usage"] = (long) (PropertyUsageFlags.Default | PropertyUsageFlags.UpdateAllIfModified);
				break;
			case nameof(this.Context):
				property["usage"] = this.GetLocalScene() == null
					? (long) PropertyUsageFlags.None
					: (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.NodePathFromSceneRoot;
				break;
			case nameof(this.Comments):
				property["hint"] = this.Comments.Length > Consts.MAX_SINGLE_LINE_COMMENT_LENGTH
					? (long) PropertyHint.MultilineText
					: (long) PropertyHint.None;
				break;
		}
	}

	Variant IAttribute.RunGetter(IReadOnlyAttributeContainer container, Variant _)
	{
		if (this.ProcessingValue)
		{
			GD.PushError(
				$"Detected circular dependency when trying to compute the value of ComputedVariable '{this.Name}'. " +
				"This usually happens when a ComputedVariable's ComputedValue depends (directly or indirectly) on " +
				"itself."
			);
			return Variant.NULL;
		}
		this.ProcessingValue = true;
		try
		{
			Godot.Collections.Dictionary<string, Variant> attributes = container.ToDictionary();
			if (this.Interpreter == null || Engine.IsEditorHint())
			{
				this.Interpreter = new();
				this.Interpreter.Parse(this.Expression, [..attributes.Keys, ..this.Params.Keys]);
			}
			Variant value = this.Interpreter.Execute(
				[..attributes.Values, ..this.Params.Values],
				this.ResourceLocalToScene ? this.GetLocalScene().GetNode(this.Context) : null
			);
			if (this.ExpectedType != Variant.Type.Nil)
			{
				if (!value.VariantType.IsConvertibleTo(this.ExpectedType))
					GD.PushWarning(
						$"ComputedVariable '{this.Name}' expected to compute a value of type '{this.ExpectedType}', " +
						$"but got '{value.VariantType}' instead."
					);
				return value.As(this.ExpectedType);
			}
			return value;
		}
		finally
		{
			this.ProcessingValue = false;
		}
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================
}
