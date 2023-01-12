using System.IO;
using Chroma;
using Chroma.Diagnostics;
using Chroma.Diagnostics.Logging;
using Chroma.Graphics;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar.Parsing;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Lexical;

namespace ScriptingEngineExample
{
    public class GameCore : Game
    {
        private Log _log = LogManager.GetForCurrentAssembly();
        
        private Lexer _lexer;
        private Parser _parser;
        private Compiler _compiler;

        private Executable _exe;
        private EVM _evm;
        private Table _globalTable;

        private RenderContext _rcref;
        
        private Chunk _updateCallback;
        private Chunk _drawCallback;
        private Chunk _initCallback;

        public GameCore()
            : base(new(false, false))
        {
            Graphics.LimitFramerate = true;
            Graphics.VerticalSyncMode = VerticalSyncMode.None;

            Window.Mode.SetWindowed(320, 240, true);
            
            InitializeGlobalTable();
            InitializeScriptingEngine();

            if (_initCallback != null)
            {
                _evm.InvokeCallback(_initCallback);
            }
        }

        protected override void Update(float delta)
        {
            if (_updateCallback != null)
            {
                _evm.InvokeCallback(_updateCallback, new DynamicValue(delta));
            }
        }

        protected override void Draw(RenderContext context)
        {
            if (_rcref == null)
                _rcref = context;

            if (_drawCallback != null)
            {
                _evm.InvokeCallback(_drawCallback);
            }
        }

        private void InitializeGlobalTable()
        {
            _globalTable = new Table();
            var chromaTable = new Table();
            chromaTable.SetByString("log", new DynamicValue((evm, args) =>
            {
                if (args.Length >= 2)
                {
                    switch ((int)args[0].Number)
                    {
                        case 0:
                            _log.Info(args[1].AsString());
                            break;
                        
                        case 1:
                            _log.Warning(args[1].AsString());
                            break;
                        
                        case 2:
                            _log.Error(args[1].AsString());
                            break;
                        
                        case 3:
                            _log.Debug(args[1].AsString());
                            break;
                    }
                }
                
                return DynamicValue.Zero;  
            }));
            
            chromaTable.SetByString("win_set_title", new DynamicValue((evm, args) =>
            {
                if (args.Length >= 1)
                {
                    Window.Title = args[0].AsString();
                }
                
                return DynamicValue.Zero;
            }));
            
            chromaTable.SetByString("get_fps", new DynamicValue((evm, args) =>
            {
                return new(PerformanceCounter.FPS);
            }));
            
            chromaTable.SetByString("fillrect", new DynamicValue((evm, args) =>
            {
                if (args.Length >= 4)
                {
                    _rcref.Rectangle(
                        ShapeMode.Fill,
                        (float)args[0].Number,
                        (float)args[1].Number,
                        (float)args[2].Number,
                        (float)args[3].Number,
                        Color.White
                    );
                }
                return DynamicValue.Zero;
            }));
            
            _globalTable.SetByString("chroma", new(chromaTable));
        }

        private void InitializeScriptingEngine()
        {
            _lexer = new Lexer();
            
            using (var streamReader = new StreamReader("main.vil"))
            {
                _lexer.LoadSource(streamReader.ReadToEnd());
            }

            _parser = new Parser(_lexer, true);
            _compiler = new Compiler();
            _exe = _compiler.Compile(_parser.Parse());

            _evm = new EVM(_exe, _globalTable);

            _updateCallback = _evm.FindExposedChunk("update");
            _drawCallback = _evm.FindExposedChunk("draw");
            _initCallback = _evm.FindExposedChunk("init");
        }
    }
}