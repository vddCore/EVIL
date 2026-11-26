namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

using EVIL.VirtualMachine.ExecutionEngine.Collections;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;
using EVIL.Core.Common.TypeSystem;

public sealed class Error
{
    private Table UserData { get; } = new();
        
    public int Length => UserData.Length;

    public string? Message
    {
        get
        {
            if (UserData["msg"].Type == DynamicValueType.String)
            {
                return UserData["msg"].String!;
            }

            return null;
        }

        set => UserData["msg"] = value ?? DynamicValue.Nil;
    }

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
        Message = message;
    }
        
    public Error(Table userData, string message)
    {
        UserData = userData;
        Message = message;
    }

    public bool Contains(DynamicValue key)
        => UserData.Contains(key);

    public bool IsDeeplyEqualTo(Error other)
        => UserData.IsDeeplyEqualTo(other.UserData);
}