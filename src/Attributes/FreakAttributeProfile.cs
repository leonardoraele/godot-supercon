using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.Supercon;

[Tool][GlobalClass]
public partial class FreakAttributeProfile : Resource, IReadOnlyFreakAttributeContainer
{
	//------------------------------------------------------------------------------------------------------------------
	// STATICS
	//------------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	//------------------------------------------------------------------------------------------------------------------
	// EXPORTS
	//------------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.ResourceType, $"{nameof(StandardFreakAttribute)},{nameof(ComputedFreakAttribute)}")]
	public Resource[] CustomAttributes = [];

	//------------------------------------------------------------------------------------------------------------------
	// FIELDS
	//------------------------------------------------------------------------------------------------------------------

	//------------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	//------------------------------------------------------------------------------------------------------------------

	public Dictionary<string, IFreakAttribute> AggregatedDefinitions
	{
		get
		{
			if (field != null)
				return field;
			Dictionary<string, IFreakAttribute> result = this.CustomAttributes.OfType<IFreakAttribute>()
				.ToDictionary(def => def.Name);
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

	public IEnumerable<IFreakAttribute> GetAttributeDefinitions()
		=> this.AggregatedDefinitions.Values;

	public bool HasAttribute(string name)
		=> this.AggregatedDefinitions.ContainsKey(name);

	public Variant GetAttributeValue(string name)
		=> this.GetAttributeDefinition(name).RunGetter(this, new Variant());

	public IFreakAttribute GetAttributeDefinition(string name)
		=> this.AggregatedDefinitions.TryGetValue(name, out IFreakAttribute? definition)
			? definition
			: throw new KeyNotFoundException($"Failed to get definition of attribute \"{name}\" in attribute profile \"{this.ResourcePath}\". Cause: Attribute does not exist in the profile.");
}
