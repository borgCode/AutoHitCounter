// 

using System.Diagnostics;

namespace AutoHitCounter.Interfaces
{
    public interface IMemoryService
    {
        bool IsAttached { get; }
        Process? TargetProcess { get; }
        nint BaseAddress { get; }
        int ModuleMemorySize { get; }
        
        byte[] ReadBytes(nint addr, int size);
        T Read<T>(nint addr) where T : unmanaged;

        void WriteBytes(nint addr, byte[] val);
        void Write<T>(nint addr, T value) where T : unmanaged;
        
        void AllocCodeCave();
        
        void StartAutoAttach(string processName);
        void StopAutoAttach();
    }
}