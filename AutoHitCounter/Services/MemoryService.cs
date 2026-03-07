// 

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Timers;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;

namespace AutoHitCounter.Services
{
    public class MemoryService : IMemoryService
    {
        public bool IsAttached { get; private set; }
        public Process? TargetProcess { get; private set; }
        public IntPtr ProcessHandle { get; private set; } = IntPtr.Zero;
        public nint BaseAddress { get; private set; }
        public int ModuleMemorySize { get; private set; }

        private const int ProcessVmRead = 0x0010;
        private const int ProcessVmWrite = 0x0020;
        private const int ProcessVmOperation = 0x0008;
        private const int ProcessQueryInformation = 0x0400;
        private const int AttachCheckInterval = 2000;

        private const uint CodeCaveSize = 0x5000;
        private const int CodeCaveSearchStart = 0x40000000;
        private const int CodeCaveSearchEnd = 0x30000;
        private const int CodeCaveSearchStep = 0x10000;

        private Timer _autoAttachTimer;
        private readonly object _attachLock = new();

        public byte[] ReadBytes(nint addr, int size)
        {
            var array = new byte[size];
            var lpNumberOfBytesRead = 1;
            Kernel32.ReadProcessMemory(ProcessHandle, addr, array, size, ref lpNumberOfBytesRead);
            return array;
        }

        public T Read<T>(nint addr) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            var bytes = ReadBytes(addr, size);
            return MemoryMarshal.Read<T>(bytes);
        }

        public nint FollowPointers64(nint baseAddress, int[] offsets, bool readFinalPtr)
        {
            nint ptr = Read<nint>(baseAddress);

            for (int i = 0; i < offsets.Length - 1; i++)
            {
                ptr = Read<nint>((IntPtr)ptr + offsets[i]);
            }

            IntPtr finalAddress = (IntPtr)ptr + offsets[offsets.Length - 1];

            if (readFinalPtr)
                return Read<nint>(finalAddress);

            return finalAddress;
        }

        public nint FollowPointers32(nint baseAddress, int[] offsets, bool readFinalPtr)
        {
            uint ptr = Read<uint>(baseAddress);

            for (int i = 0; i < offsets.Length - 1; i++)
            {
                ptr = Read<uint>((nint)ptr + offsets[i]);
            }

            nint finalAddress = (nint)ptr + offsets[offsets.Length - 1];

            if (readFinalPtr)
                return (nint)Read<uint>(finalAddress);

            return finalAddress;
        }

        public void Write<T>(nint addr, T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            var bytes = new byte[size];
            MemoryMarshal.Write(bytes, ref value);
            WriteBytes(addr, bytes);
        }

        public void WriteBytes(nint addr, byte[] val)
        {
            Kernel32.WriteProcessMemory(ProcessHandle, addr, val, val.Length, 0);
        }

        public IntPtr AllocCustomCodeMem()
        {
            nint searchRangeStart = BaseAddress - CodeCaveSearchStart;
            nint searchRangeEnd = BaseAddress - CodeCaveSearchEnd;

            for (nint addr = searchRangeEnd; addr > searchRangeStart; addr -= CodeCaveSearchStep)
            {
                var allocatedMemory = Kernel32.VirtualAllocEx(ProcessHandle, addr, CodeCaveSize);

                if (allocatedMemory != IntPtr.Zero)
                {
                    return allocatedMemory;
                }
            }

            return IntPtr.Zero;
        }

        public IntPtr GetProcAddress(string moduleName, string procName)
        {
            IntPtr moduleHandle = Kernel32.GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
                return IntPtr.Zero;

            return Kernel32.GetProcAddress(moduleHandle, procName);
        }

        public void StartAutoAttach(string processName)
        {
            _autoAttachTimer?.Stop();
            _autoAttachTimer?.Dispose();

            _autoAttachTimer = new Timer(AttachCheckInterval);
            _autoAttachTimer.Elapsed += (sender, e) => TryAttachToProcess(processName);
            _autoAttachTimer.Start();

            TryAttachToProcess(processName);
        }

        public void StopAutoAttach()
        {
            _autoAttachTimer?.Stop();
            _autoAttachTimer?.Dispose();
            _autoAttachTimer = null;
        }

        private void TryAttachToProcess(string processName)
        {
            lock (_attachLock)
            {
                if (ProcessHandle != IntPtr.Zero)
                {
                    if (TargetProcess == null || TargetProcess.HasExited)
                    {
                        Kernel32.CloseHandle(ProcessHandle);
                        ProcessHandle = IntPtr.Zero;
                        TargetProcess = null;
                        IsAttached = false;
                    }

                    return;
                }

                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0 && !processes[0].HasExited)
                {
                    TargetProcess = processes[0];
                    ProcessHandle = Kernel32.OpenProcess(
                        ProcessVmRead | ProcessVmWrite | ProcessVmOperation | ProcessQueryInformation,
                        false,
                        TargetProcess.Id);

                    if (ProcessHandle == IntPtr.Zero)
                    {
                        TargetProcess = null;
                        IsAttached = false;
                    }
                    else
                    {
                        if (TargetProcess.MainModule != null)
                        {
                            BaseAddress = TargetProcess.MainModule.BaseAddress;
                            ModuleMemorySize = TargetProcess.MainModule.ModuleMemorySize;
                        }

                        IsAttached = true;
                    }
                }
            }
        }
    }
}