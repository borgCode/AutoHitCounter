using System;
using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Memory
{
    public class HookManager(IMemoryService memoryService)
    {
        private const int AbsoluteJumpLength = 14;

        private readonly Dictionary<nint, HookData> _hookRegistry = new();

        private class HookData
        {
            public nint OriginAddr { get; set; }
            public nint CaveAddr { get; set; }
            public byte[] OriginalBytes { get; set; }
            public byte[] HookBytes { get; set; }
        }

        public void InstallHook(nint codeLoc, nint origin, byte[] originalBytes)
            => InstallHook(codeLoc, origin, originalBytes, 0xE9);

        public void InstallCallHook(nint codeLoc, nint origin, byte[] originalBytes)
            => InstallHook(codeLoc, origin, originalBytes, 0xE8);
        
        public void InstallAbsoluteHook(nint codeLoc, nint origin, byte[] originalBytes)
            => WriteHook(codeLoc, origin, originalBytes, GetAbsoluteHookBytes(originalBytes.Length, codeLoc));

        private void InstallHook(nint codeLoc, nint origin, byte[] originalBytes, byte opcode)
            => WriteHook(codeLoc, origin, originalBytes, GetHookBytes(originalBytes.Length, codeLoc, origin, opcode));

        private void WriteHook(nint codeLoc, nint origin, byte[] originalBytes, byte[] hookBytes)
        {
            memoryService.WriteBytes(origin, hookBytes);
            _hookRegistry[codeLoc] = new HookData
            {
                CaveAddr = codeLoc,
                OriginAddr = origin,
                OriginalBytes = originalBytes,
                HookBytes = hookBytes
            };
        }

        public bool IsHookInstalled(nint codeLoc)
        {
            if (!_hookRegistry.TryGetValue(codeLoc, out HookData hook)) return false;

            var current = memoryService.ReadBytes(hook.OriginAddr, hook.HookBytes.Length);
            return current.SequenceEqual(hook.HookBytes);
        }

        private byte[] GetHookBytes(int originalBytesLength, nint target, nint origin, byte opcode)
        {
            byte[] hookBytes = new byte[originalBytesLength];
            hookBytes[0] = opcode;

            int jumpOffset = (int)(target - (origin + 5));
            byte[] offsetBytes = BitConverter.GetBytes(jumpOffset);
            Array.Copy(offsetBytes, 0, hookBytes, 1, 4);

            for (int i = 5; i < hookBytes.Length; i++)
            {
                hookBytes[i] = 0x90;
            }

            return hookBytes;
        }
        
        private byte[] GetAbsoluteHookBytes(int originalBytesLength, nint target)
        {
            if (originalBytesLength < AbsoluteJumpLength)
                throw new ArgumentException($"Absolute hook needs at least {AbsoluteJumpLength} original bytes");

            byte[] hookBytes = new byte[originalBytesLength];
            hookBytes[0] = 0xFF;
            hookBytes[1] = 0x25;
            AsmHelper.WriteAbsoluteAddress(hookBytes, target, 6);

            for (int i = AbsoluteJumpLength; i < hookBytes.Length; i++)
            {
                hookBytes[i] = 0x90;
            }

            return hookBytes;
        }

        public void UninstallHook(nint key)
        {
            if (!_hookRegistry.TryGetValue(key, out HookData hookToUninstall)) return;

            memoryService.WriteBytes(hookToUninstall.OriginAddr, hookToUninstall.OriginalBytes);
            _hookRegistry.Remove(key);
        }

        public void ClearHooks()
        {
            _hookRegistry.Clear();
        }

        public void UninstallAllHooks()
        {
            foreach (var key in _hookRegistry.Keys.ToList())
            {
                UninstallHook(key);
            }
        }
    }
}