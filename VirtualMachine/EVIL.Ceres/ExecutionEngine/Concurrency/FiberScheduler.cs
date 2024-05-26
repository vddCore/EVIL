using System.Collections.Generic;
using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

namespace EVIL.Ceres.ExecutionEngine.Concurrency
{

    public class FiberScheduler
    {
        private CeresVM _vm;
        private FiberCrashHandler _defaultCrashHandler;

        private List<Fiber> _fibers;
        private List<int> _dueForRemoval;

        private bool _running;

        public IReadOnlyList<Fiber> Fibers => _fibers;
        public bool IsRunning => _running;
        
        public FiberScheduler(CeresVM vm, FiberCrashHandler defaultCrashHandler)
        {
            _vm = vm;
            _defaultCrashHandler = defaultCrashHandler;
            
            _fibers = new();
            _dueForRemoval = new();
        }

        public void Run()
        {
            _running = true;

            while (_running)
            {
                lock (_fibers)
                {
                    for (var i = 0; i < _fibers.Count; i++)
                    {
                        var fiber = _fibers[i];
                        
                        if (fiber._state == FiberState.Awaiting)
                        {
                            fiber.RemoveFinishedAwaitees();
                            fiber.Resume();
                        }
                        else if (fiber._state == FiberState.Fresh)
                        {
                            fiber.Resume();
                        }
                        else if (fiber._state == FiberState.Paused)
                        {
                            continue;
                        }
                        else
                        {
                            if (fiber._state == FiberState.Finished || fiber._state == FiberState.Crashed)
                            {
                                if (!fiber.ImmuneToCollection)
                                {
                                    _dueForRemoval.Add(i);
                                }
                            }
                            else
                            {
                                fiber.Resume();
                            }

                            if (fiber._state != FiberState.Running)
                            {
                                continue;
                            }
                        }
                        
                        fiber.Step();
                    }

                    RemoveFinishedFibers();
                }
            }
        }

        public void SetDefaultCrashHandler(FiberCrashHandler crashHandler)
        {
            _defaultCrashHandler = crashHandler;
        }

        public void Stop()
        {
            lock (_fibers)
            {
                _running = false;
            }
        }

        public Fiber CreateFiber(
            bool immunized,
            FiberCrashHandler? crashHandler = null,
            Dictionary<string, ClosureContext>? closureContexts = null)
        {
            var fiber = new Fiber(
                _vm, 
                crashHandler ?? _defaultCrashHandler, 
                closureContexts
            );

            if (immunized)
            {
                fiber.Immunize();
            }

            lock (_fibers)
            {
                _fibers.Add(fiber);
            }

            return fiber;
        }

        public void RemoveCrashedFibers()
        {
            for (var i = 0; i < _fibers.Count; i++)
            {
                if (_fibers[i]._state == FiberState.Crashed)
                {
                    _dueForRemoval.Add(i);
                }
            }
        }

        private void RemoveFinishedFibers()
        {
            if (!_dueForRemoval.Any())
                return;

            _dueForRemoval.Sort();
            _dueForRemoval.Reverse();

            for (var i = 0; i < _dueForRemoval.Count; i++)
            {
                _fibers.RemoveAt(_dueForRemoval[i]);
            }

            _dueForRemoval.Clear();
        }
    }
}