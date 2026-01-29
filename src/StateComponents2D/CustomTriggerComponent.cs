using Godot;

namespace Raele.Supercon2D.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_binary.png")]
public partial class CustomTriggerComponent : SuperconStateComponent2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node? Context;
	/// <summary>
	/// This value will be available in the expression's context as the 'context' variable.
	/// </summary>
	[Export] public Variant Param = new Variant();
	[Export(PropertyHint.Expression)] public string Expression
		{ get; set { field = value; this.Interpreter = null!; } }
		= "";
	[Export] public bool PhysicsProcess = false;

	[ExportCategory("🔀 Connect State Transitions")]
	[ExportToolButton("On Triggered")] public Callable ConnectTriggeredToolButton
		=> Callable.From(this.OnConnectTriggeredToolButtonPressed);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression Interpreter
	{
		get
		{
			if (field == null)
			{
				field = new();
				field.Parse(this.Expression, ["param"]);
			}
			return field;
		}
		set;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void TriggeredEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
			this.Context ??= this.Character;
	}

	protected override void _ActivityStarted(string mode, Variant argument)
	{
		base._ActivityStarted(mode, argument);
		this.SetProcess(!this.PhysicsProcess);
		this.SetPhysicsProcess(this.PhysicsProcess);
	}

	protected override void _ActivityProcess(double delta)
	{
		base._ActivityProcess(delta);
		if (this.TestExpression())
			this.EmitSignalTriggered();
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		if (this.TestExpression())
			this.EmitSignalTriggered();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private bool TestExpression()
	{
		Variant result = this.Interpreter.Execute([this.Param], this.Context);
		if (result.VariantType != Variant.Type.Bool)
		{
			GD.PushWarning(
				$"{this.GetPath()} - The expression '{this.Expression}' did not return a boolean value. " +
				"Make sure the expression is correct."
			);
			return false;
		}
		return result.AsBool();
	}

	private void OnConnectTriggeredToolButtonPressed() => this.ConnectStateTransition(SignalName.Triggered);
}
