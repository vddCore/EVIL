using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine.Diagnostics
{
    public class Error
    {
        private Table UserData { get; } = new();
        
        public int Length => UserData.Length;
        
        public DynamicValue this[DynamicValue key]
        {
            get => UserData[key];
            set => UserData[key] = value;
        }

        public Error()
        {
        }

        public Error(Table userData)
        {
            UserData = userData;
        }
        
        public Error(string message)
        {
            UserData["msg"] = message;
        }
        
        public Error(Table userData, string message)
        {
            UserData = userData;
            UserData["msg"] = message;
        }

        public bool Contains(DynamicValue key)
            => UserData.Contains(key);

        public bool IsDeeplyEqualTo(Error other)
            => UserData.IsDeeplyEqualTo(other.UserData);
    }
}