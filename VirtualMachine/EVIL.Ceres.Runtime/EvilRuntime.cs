namespace EVIL.Ceres.Runtime;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.Ceres.Runtime.Modules;
using EVIL.Ceres.TranslationEngine;

public sealed class EvilRuntime(VirtualMachineBase vm)
{
    private readonly Compiler _compiler = new();

    private Table Global => vm.Global;

    public void RegisterBuiltInFunctions()
    {
        var scriptNames = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(x => x.StartsWith("EVIL.Ceres.Runtime.ScriptBuiltins"));

        foreach (var scriptName in scriptNames)
        {
            using var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(scriptName)!;

            using var streamReader = new StreamReader(stream);
            var fileName = $"builtin::{scriptName}";
                
            try
            {
                var text = streamReader.ReadToEnd();
                var root = _compiler.Compile(text, fileName);
                vm.MainFiber.Schedule(root);
            }
            catch (CompilerException e)
            {
                throw new EvilRuntimeException($"Failed to compile the built-in script '{fileName}'.", e);  
            }
        }
    }

    public List<RuntimeModule> RegisterBuiltInModules()
    {
        var modules = new List<RuntimeModule>
        {
            RegisterModule<ArrayModule>(out _),
            RegisterModule<ConvertModule>(out _),
            RegisterModule<CoreModule>(out _),
            RegisterModule<DebugModule>(out _),
            RegisterModule<EvilModule>(out _),
            RegisterModule<FsModule>(out _),
            RegisterModule<IoModule>(out _),
            RegisterModule<MathModule>(out _),
            RegisterModule<StringModule>(out _),
            RegisterModule<TableModule>(out _),
            RegisterModule<TimeModule>(out _)
        };

        return modules;
    }

    public T RegisterModule<T>(out DynamicValue table) where T : RuntimeModule, new()
    {
        var module = new T();
            
        table = module.AttachTo(Global);
        module.Registered(this);
            
        return module;
    }

    public RuntimeModule RegisterModule(Type type, out DynamicValue table)
    {
        var module = (RuntimeModule)Activator.CreateInstance(type)!;
        table = module.AttachTo(Global);

        return module;
    }

    public DynamicValue Register(string fullyQualifiedName, DynamicValue value, bool replaceIfExists = true)
        => Global.SetUsingPath<PropertyTable>(fullyQualifiedName, value, replaceIfExists);
}