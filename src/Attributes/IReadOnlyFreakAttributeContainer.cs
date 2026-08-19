using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

public interface IReadOnlyFreakAttributeContainer
{
	//==================================================================================================================
	// ABSTRACTS
	//==================================================================================================================

	public bool HasAttribute(string name);
	public Variant GetAttributeValue(string name);
	public IFreakAttribute GetAttributeDefinition(string name);
	public IEnumerable<IFreakAttribute> GetAttributeDefinitions();

	//==================================================================================================================
	// CONCRETES
	//==================================================================================================================

	public IEnumerable<string> GetAttributeNames()
		=> this.GetAttributeDefinitions().Select(definition => definition.Name);
	public Godot.Collections.Dictionary<string, Variant> ToDictionary()
		=> this.GetAttributeNames()
			.ToDictionary(name => name, this.GetAttributeValue)
			.ToGodotDictionaryT();
}

public static class IReadOnlyAttributeContainerExtensions
{
	public static IReadOnlyFreakAttributeContainer AsReadOnlyAttributeContainer(this IReadOnlyFreakAttributeContainer source) => source;
}
