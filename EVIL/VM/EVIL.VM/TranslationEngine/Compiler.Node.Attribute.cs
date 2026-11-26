namespace EVIL.VirtualMachine.TranslationEngine;

using System;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Miscellaneous;

public partial class Compiler
{
    protected override void Visit(AttributeNode attributeNode)
    {
        var attribute = new ChunkAttribute(attributeNode.Identifier.Name);

        foreach (var valueNode in attributeNode.Values)
        {
            attribute.Values.Add(
                ExtractConstantValueFrom(valueNode)
            );
        }

        foreach (var propertyKvp in attributeNode.Properties)
        {
            attribute.Properties.Add(
                propertyKvp.Key.Name,
                ExtractConstantValueFrom(propertyKvp.Value)
            );
        }

        Chunk.AddAttribute(attribute);

        if (_attributeProcessors.TryGetValue(attribute.Name, out var processors))
        {
            foreach (var processor in processors)
            {
                try
                {
                    processor.Invoke(attribute, Chunk);
                }
                catch (Exception e)
                {
                    throw new AttributeProcessorException(attribute, Chunk, e);
                }
            }
        }
    }
}