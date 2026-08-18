using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class FreakCustomDataInterface : Resource, IReadOnlyAttributeContainer
{
	//------------------------------------------------------------------------------------------------------------------
	// STATICS
	//------------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	//------------------------------------------------------------------------------------------------------------------
	// EXPORTS
	//------------------------------------------------------------------------------------------------------------------

	[Export] public string Name
		{
			get;
			set
			{
				field = value;
				this.ResourceName = string.IsNullOrWhiteSpace(value) ? "" : $"{value.Trim()} Profile";
			}
		}
		= "";
	[Export] public FreakCustomDataInterface?[] Inherits = [];
	[Export(PropertyHint.ResourceType, $"{nameof(MutableAttribute)},{nameof(ComputedAttribute)}")]
	public Resource[]? Attributes;

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	//------------------------------------------------------------------------------------------------------------------

	public Dictionary<string, IAttribute> AggregatedDefinitions
	{
		get
		{
			if (field != null)
				return field;
			Dictionary<string, IAttribute> result = this.Inherits.OfType<FreakCustomDataInterface>()
				.SelectMany(profile => profile.AggregatedDefinitions)
				.Concat((this.Attributes ?? []).OfType<IAttribute>().ToDictionary(def => def.Name))
				.ToDictionary();
			if (!Engine.IsEditorHint())
				field = result;
			return result;
		}
	}

	//------------------------------------------------------------------------------------------------------------------
	// SIGNALS
	//------------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	//------------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// METHODS
	//------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Tests whether this profile equals to the <paramref name="other"/> profile or inherits it. i.e. if this profile
	/// is a subtype of the <paramref name="other"/> profile, and therefore can be used in a place that expects it.
	/// </summary>
	public bool IsA(FreakCustomDataInterface other)
		=> other == this || this.Inherits.WhereNotNull().Any(parent => parent.IsA(other));

	/// <summary>
	/// Tests whether the given <paramref name="container"/> implements all attributes defined in this profile, with the
	/// correct types.
	/// </summary>
	public bool TestIsImplementedBy(IReadOnlyAttributeContainer container)
		=> this.GetAttributeDefinitions()
			.All(definition =>
				container.HasAttribute(definition.Name)
				&& container.GetAttributeDefinition(definition.Name).Type == definition.Type
			);

	public IEnumerable<IAttribute> GetAttributeDefinitions()
		=> this.AggregatedDefinitions.Values;

	public bool HasAttribute(string name)
		=> this.AggregatedDefinitions.ContainsKey(name);

	public Variant GetAttributeValue(string name)
		=> this.GetAttributeDefinition(name).RunGetter(this, new Variant());

	public IAttribute GetAttributeDefinition(string name)
		=> this.AggregatedDefinitions.TryGetValue(name, out IAttribute? definition)
			? definition
			: throw new KeyNotFoundException($"Failed to get definition of attribute \"{name}\" in attribute profile \"{this.ResourcePath}\". Cause: Attribute does not exist in the profile.");
}
