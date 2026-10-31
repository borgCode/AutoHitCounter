using System;
using System.Collections.Generic;
using System.Linq;
using AutoHitCounter.Interfaces;

namespace AutoHitCounter.Memory
{
    public class HookManager(IMemoryService memoryService)
    {
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

        private void InstallHook(nint codeLoc, nint origin, byte[] originalBytes, byte opcode)
        {
            byte[] hookBytes = GetHookBytes(originalBytes.Length, codeLoc, origin, opcode);
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