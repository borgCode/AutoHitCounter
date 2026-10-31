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
        public nint ProcessHandle { get; private set; }
        public nint BaseAddress { get; private set; }
        public int ModuleMemorySize { get; private set; }
        public bool IsWow64 { get; private set; }

        private const int ProcessVmRead = 0x0010;
        private const int ProcessVmWrite = 0x0020;
        private const int ProcessVmOperation = 0x0008;
        private const int ProcessQueryInformation = 0x0400;
        private const int ProcessAccessRights =
            ProcessVmRead | ProcessVmWrite | ProcessVmOperation | ProcessQueryInformation;
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
                ptr = Read<nint>(ptr + offsets[i]);
            }

            nint finalAddress = ptr + offsets[offsets.Length - 1];

            if (readFinalPtr)
            {
                return Read<nint>(finalAddress);
            }

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
            {
                return (nint)Read<uint>(finalAddress);
            }

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

        public nint AllocCustomCodeMem()
        {
            nint searchRangeStart = BaseAddress - CodeCaveSearchStart;
            nint searchRangeEnd = BaseAddress - CodeCaveSearchEnd;

            for (nint addr = searchRangeEnd; addr > searchRangeStart; addr -= CodeCaveSearchStep)
            {
                nint allocatedMemory = Kernel32.VirtualAllocEx(ProcessHandle, addr, CodeCaveSize);

                if (allocatedMemory != 0)
                {
                    return allocatedMemory;
                }
            }

            return 0;
        }

        public nint FindAllocationStartingWith(byte[] signature)
        {
            var mbiSize = (uint)Marshal.SizeOf<Kernel32.MemoryBasicInformation>();
            long address = 0;

            while (Kernel32.VirtualQueryEx(ProcessHandle, (nint)address, out var mbi, mbiSize) != 0)
            {
                if (mbi.State == Kernel32.MemCommit && mbi.BaseAddress == mbi.AllocationBase && IsReadable(mbi.Protect)
                    && ReadBytes(mbi.BaseAddress, signature.Length).AsSpan().SequenceEqual(signature))
                    return mbi.BaseAddress;

                var next = (long)mbi.BaseAddress + (long)mbi.RegionSize;
                if (next <= address) break;
                address = next;
            }

            return 0;
        }

        public bool IsExecutable(nint address)
        {
            var mbiSize = (uint)Marshal.SizeOf<Kernel32.MemoryBasicInformation>();
            return Kernel32.VirtualQueryEx(ProcessHandle, address, out var mbi, mbiSize) != 0
                   && mbi.State == Kernel32.MemCommit
                   && (mbi.Protect & Kernel32.PageExecuteAny) != 0;
        }

        private static bool IsReadable(uint protect) =>
            (protect & Kernel32.PageGuard) == 0 && (protect & 0xFF) is not (0 or Kernel32.PageNoAccess);

        public nint GetProcAddress(string moduleName, string procName)
        {
            nint moduleHandle = Kernel32.GetModuleHandle(moduleName);
            if (moduleHandle == 0)
            {
                return 0;
            }

            return Kernel32.GetProcAddress(moduleHandle, procName);
        }

        public void StartAutoAttach(string[] processNames)
        {
            StopAutoAttach();

            _autoAttachTimer = new Timer(AttachCheckInterval);
            _autoAttachTimer.Elapsed += (sender, e) => RefreshAttachment(processNames);
            _autoAttachTimer.Start();

            RefreshAttachment(processNames);
        }

        public void StopAutoAttach()
        {
            _autoAttachTimer?.Stop();
            _autoAttachTimer?.Dispose();
            _autoAttachTimer = null;

            lock (_attachLock)
            {
                if (ProcessHandle != 0)
                {
                    Detach();
                }
            }
        }

        private void RefreshAttachment(string[] processNames)
        {
            lock (_attachLock)
            {
                if (ProcessHandle != 0)
                {
                    if (TargetProcess == null || TargetProcess.HasExited)
                    {
                        Detach();
                    }

                    return;
                }

                var processes = FindProcesses(processNames);
                try
                {
                    if (processes.Length == 0 || processes[0].HasExited)
                    {
                        return;
                    }

                    var process = processes[0];
                    nint handle = Kernel32.OpenProcess(ProcessAccessRights, bInheritHandle: false, process.Id);
                    if (handle == 0)
                    {
                        return;
                    }

                    TargetProcess = process;
                    ProcessHandle = handle;
                    IsWow64 = Kernel32.IsWow64Process(handle, out var isWow64) && isWow64;

                    if (process.MainModule is { } mainModule)
                    {
                        BaseAddress = mainModule.BaseAddress;
                        ModuleMemorySize = mainModule.ModuleMemorySize;
                    }

                    IsAttached = true;
                }
                finally
                {
                    foreach (var candidate in processes)
                    {
                        if (candidate != TargetProcess)
                        {
                            candidate.Dispose();
                        }
                    }
                }
            }
        }

        private static Process[] FindProcesses(string[] processNames)
        {
            foreach (var processName in processNames)
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    return processes;
                }
            }

            return [];
        }

        private void Detach()
        {
            Kernel32.CloseHandle(ProcessHandle);
            ProcessHandle = 0;
            TargetProcess?.Dispose();
            TargetProcess = null;
            IsAttached = false;
        }
    }
}
