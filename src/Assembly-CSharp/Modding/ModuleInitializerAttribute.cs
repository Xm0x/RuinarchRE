// Unity 2020.3's mscorlib (.NET 4.x) predates ModuleInitializerAttribute
// (added in .NET 5). Declaring it here with the exact fully-qualified name lets
// the Roslyn compiler recognize [ModuleInitializer] and emit the call into the
// module's static constructor, which the Mono runtime runs when Assembly-CSharp
// is first loaded - the earliest possible managed entry point, and the one the
// mod loader uses to bootstrap without depending on Unity's build-time
// RuntimeInitializeOnLoads registry.
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	internal sealed class ModuleInitializerAttribute : Attribute
	{
	}
}
