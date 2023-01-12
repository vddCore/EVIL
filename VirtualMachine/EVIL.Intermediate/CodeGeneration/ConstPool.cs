using EVIL.Intermediate.CodeGeneration.Collections;

namespace EVIL.Intermediate.CodeGeneration
{
    public class ConstPool
    {
        private int _nextConstantId;
        
        private TwoWayDictionary<string, int> _stringConstants = new();
        private TwoWayDictionary<double, int> _numberConstants = new();

        public int Count => _nextConstantId;

        public int FetchOrAddConstant(string constant)
        {
            if (_stringConstants.TryGetByKey(constant, out var id))
                return id;
            
            _stringConstants.Add(constant, _nextConstantId);
            return _nextConstantId++;
        }

        public int FetchOrAddConstant(double constant)
        {
            if (_numberConstants.TryGetByKey(constant, out var id))
                return id;

            _numberConstants.Add(constant, _nextConstantId);
            return _nextConstantId++;
        }

        public string GetStringConstant(int id)
        {
            if (!_stringConstants.Reverse.ContainsKey(id))
                return null;

            return _stringConstants.GetByValue(id);
        }

        public double? GetNumberConstant(int id)
        {
            if (!_numberConstants.Reverse.ContainsKey(id))
                return null;

            return _numberConstants.GetByValue(id);
        }

        public int? GetStringConstantId(string constant)
        {
            if (!_stringConstants.TryGetByKey(constant, out var id))
                return null;

            return id;
        }

        public int? GetDoubleConstantId(double constant)
        {
            if (!_numberConstants.TryGetByKey(constant, out var id))
                return null;

            return id;
        }

        public void Clear()
        {
            _stringConstants.Clear();
            _numberConstants.Clear();
            
            _nextConstantId = 0;
        }
    }
}