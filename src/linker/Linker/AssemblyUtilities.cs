using Mono.Cecil;

namespace Mono.Linker
{

	public static class AssemblyUtilities
	{

		public static bool IsCrossgened (this ModuleDefinition module)
		{
			return (module.Attributes & ModuleAttributes.ILOnly) == 0 &&
				(module.Attributes & ModuleAttributes.ILLibrary) != 0;
		}

		public static EmbeddedResource FindEmbeddedResource (this AssemblyDefinition assembly, string name)
		{
			foreach (var resource in assembly.MainModule.Resources) {
				if (resource is EmbeddedResource embeddedResource && embeddedResource.Name == name)
					return embeddedResource;
			}
			return null;
		}
	}
}
