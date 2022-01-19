using System.Reflection;
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Internal;

internal class AssemblyResolver : IAssemblyResolver
{
    private readonly Assembly _assembly;

    public AssemblyResolver(
        Assembly assembly
    )
    {
        _assembly = assembly;
    }

    public Assembly GetResolvedAssembly()
    {
        return _assembly;
    }
}

internal class DynamicallyCompiledAssemblyResolver : IAssemblyResolver, IDisposable
{
    private readonly ICompilerFactory _compilerFactory;
    private Assembly? _compiledAssembly;
    private CollectibleAssemblyLoadContext? _currentContext;

    public DynamicallyCompiledAssemblyResolver(
        ICompilerFactory compilerFactory
    )
    {
        _compilerFactory = compilerFactory;
    }

    public Assembly GetResolvedAssembly()
    {
        // We reuse an already compiled assembly since we only 
        // compile once per start
        if (_compiledAssembly is not null)
            return _compiledAssembly;

        var compiler = _compilerFactory.New();
        var (loadContext, compiledAssembly) = compiler.Compile();
        _currentContext = loadContext;
        _compiledAssembly = compiledAssembly;
        return compiledAssembly;
    }

    public void Dispose()
    {
        if (_currentContext is null) return;
        _currentContext.Unload();
        // Finally do cleanup and release memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}