// 

using System;
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
        public nint FollowPointers(nint baseAddress, int[] offsets, bool readFinalPtr, bool derefBase = true);

        void WriteBytes(nint addr, byte[] val);
        void Write<T>(nint addr, T value) where T : unmanaged;
        
        IntPtr AllocCustomCodeMem();
        
        void StartAutoAttach(string processName);
        void StopAutoAttach();
    }
}