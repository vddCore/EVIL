using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class Chunk : IDisposable
    {
        private MemoryStream _code;
        private List<int> _labels;
        private List<ChunkAttribute> _attributes;

        public string? Name { get; set; }

        public StringPool StringPool { get; }
        public CodeGenerator CodeGenerator { get; }

        public int ParameterCount { get; private set; }
        public int LocalCount { get; private set; }
        
        public IReadOnlyList<int> Labels => _labels;
        public IReadOnlyList<ChunkAttribute> Attributes => _attributes;

        public byte[] Code => _code.GetBuffer();
        public bool IsAnonymous => Name == null;

        public Chunk()
        {
            _code = new MemoryStream(0);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            
            StringPool = new StringPool();
            CodeGenerator = new CodeGenerator(_code, _labels);
        }

        public Chunk(byte[] code)
        {
            _code = new MemoryStream(code, 0, code.Length, true, true);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            
            StringPool = new StringPool();
            CodeGenerator = new CodeGenerator(_code, _labels);
        }

        public Chunk(byte[] code, string[] stringConstants)
        {
            _code = new MemoryStream(code, 0, code.Length, true, true);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();

            StringPool = new StringPool(stringConstants);
            CodeGenerator = new CodeGenerator(_code, _labels);
        }

        public BinaryReader SpawnCodeReader()
        {
            return new BinaryReader(
                new MemoryStream(_code.ToArray()),
                Encoding.UTF8,
                false
            );
        }

        public int AllocateParameter()
            => ParameterCount++;

        public int AllocateLocal()
            => LocalCount++;

        public int CreateLabel()
            => CreateLabel(CodeGenerator.IP);
        
        public int CreateLabel(int ip)
        {
            _labels.Add(ip);
            return _labels.Count - 1;
        }

        public void AddAttribute(ChunkAttribute attribute) 
            => _attributes.Add(attribute);

        public void UpdateLabel(int id, int ip) 
            => _labels[id] = ip;

        public ChunkAttribute[] GetAttributes(string name)
            => _attributes.Where(x => x.Name == name).ToArray();
        
        public ChunkAttribute? GetAttribute(string name)
            => _attributes.FirstOrDefault(x => x.Name == name);

        public bool HasAttribute(string name)
            => GetAttribute(name) != null;
        
        public void Dispose()
        {
            _code.Dispose();
            _labels.Clear();
            
            CodeGenerator.Dispose();
        }
    }
}