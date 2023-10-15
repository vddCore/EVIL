using System;
using System.Collections.Generic;
using System.Linq;

namespace Ceres.ExecutionEngine.Concurrency
{
    public delegate void FiberCrashHandler(FiberScheduler scheduler, Fiber fiber, Exception exception);

    public class FiberScheduler
    {
        private CeresVM _vm;

        private List<Fiber> _fibers;
        private List<int> _dueForRemoval;

        private bool _running;
        private FiberCrashHandler? _crashHandler;

        public IReadOnlyList<Fiber> Fibers => _fibers;
        public bool IsRunning => _running;
        
        public FiberScheduler(CeresVM vm)
        {
            _vm = vm;

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
                        
                        if (fiber.State == FiberState.Awaiting)
                        {
                            fiber.RemoveFinishedAwaitees();
                            fiber.Resume();
                        }
                        else if (fiber.State == FiberState.Fresh)
                        {
                            fiber.Resume();
                        }
                        else if (fiber.State == FiberState.Paused)
                        {
                            continue;
                        }
                        else
                        {
                            if (fiber.State == FiberState.Finished)
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

                            if (fiber.State != FiberState.Running)
                            {
                                continue;
                            }
                        }

                        try
                        {
                            fiber.Step();
                        }
                        catch (Exception e)
                        {
                            fiber.EnterCrashState();
                            
                            if (_crashHandler != null)
                            {
                                _crashHandler(this, fiber, e);
                            }
                        }
                    }

                    RemoveFinishedFibers();
                }
            }
        }

        public void Stop()
        {
            lock (_fibers)
            {
                _running = false;
            }
        }

        public Fiber CreateFiber(bool immunized)
        {
            var fiber = new Fiber(_vm);

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

        public void SetCrashHandler(FiberCrashHandler handler)
        {
            _crashHandler = handler;
        }

        public void RemoveCrashedFibers()
        {
            for (var i = 0; i < _fibers.Count; i++)
            {
                if (_fibers[i].State == FiberState.Crashed)
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