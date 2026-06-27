// 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory.Patterns;

namespace AutoHitCounter.Memory;

public class AobScanner(IMemoryService memoryService)
{
    private const int HistogramSampleStep = 16;
    private const double AnchorLogPpmThreshold = 50;

    private readonly List<Request> _requests = new();

    private byte[] _module;
    private nint _moduleBase;

    private readonly byte[] _bitmap = new byte[65536 / 8];
    private readonly List<Request>[] _pairBuckets = new List<Request>[65536];
    private readonly List<Request>[] _singleBuckets = new List<Request>[256];
    private bool _hasSingleFallback;

    private readonly Dictionary<string, nint> _savedAddresses = new();
    private readonly long[] _pairHistogram = new long[65536];
    private long _histogramSamples;
    private bool _histogramBuilt;

    public void Queue(string name, Pattern pattern, Action<nint> setter)
        => _requests.Add(new Request(_requests.Count, name, pattern, setter));

    public void Run(string savedAddressPath)
    {
        LoadModule();
        LoadSavedAddresses(savedAddressPath);
        AssignAnchors();
#if DEBUG
        LogAnchors();
#endif

        var buf = _module;
        var bufLen = buf.Length;
        var end = bufLen - 1;
        ref var bufRef = ref buf[0];

        var found = new bool[_requests.Count];
        var matchCounts = new int[_requests.Count];
        var remaining = _requests.Count;

        for (var i = 0; i < end && remaining > 0; i++)
        {
            var b0 = Unsafe.Add(ref bufRef, i);
            var key = b0 | (Unsafe.Add(ref bufRef, i + 1) << 8);

            if ((_bitmap[key >> 3] & (1 << (key & 7))) != 0)
            {
                var bucket = _pairBuckets[key];
                if (bucket != null)
                {
                    foreach (var request in bucket)
                    {
                        if (found[request.Id]) continue;
                        var start = i - request.AnchorOffset;
                        if (start < 0) continue;
                        if (!Matches(ref bufRef, bufLen, start, request)) continue;
                        if (!AcceptOccurrence(request, matchCounts)) continue;

                        ResolveAndInvoke(request, start);
                        found[request.Id] = true;
                        remaining--;
                    }
                }
            }

            if (!_hasSingleFallback) continue;

            var singleBucket = _singleBuckets[b0];
            if (singleBucket == null) continue;

            foreach (var request in singleBucket)
            {
                if (found[request.Id]) continue;
                var start = i - request.AnchorOffset;
                if (start < 0) continue;
                if (!Matches(ref bufRef, bufLen, start, request)) continue;
                if (!AcceptOccurrence(request, matchCounts)) continue;

                ResolveAndInvoke(request, start);
                found[request.Id] = true;
                remaining--;
            }
        }

        for (var i = 0; i < _requests.Count; i++)
        {
            if (found[i]) continue;

            var request = _requests[i];
            if (_savedAddresses.TryGetValue(request.Name, out var savedAddress))
            {
                request.Setter(savedAddress);
                continue;
            }

            request.Setter(0);
        }

        WriteSavedAddresses(savedAddressPath);
    }

    private void LoadModule()
    {
        _moduleBase = memoryService.BaseAddress;
        _module = memoryService.ReadBytes(_moduleBase, memoryService.ModuleMemorySize);
    }

    private void LoadSavedAddresses(string path)
    {
        _savedAddresses.Clear();
        if (!File.Exists(path)) return;

        foreach (var line in File.ReadAllLines(path))
        {
            var parts = line.Split('=');
            if (parts.Length != 2) continue;
            if (long.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                _savedAddresses[parts[0]] = (nint)value;
        }
    }

    private void WriteSavedAddresses(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var writer = new StreamWriter(path);
        foreach (var kvp in _savedAddresses)
            writer.WriteLine($"{kvp.Key}={(long)kvp.Value:X}");
    }

    private static bool IsConcrete(string mask, int index, int length)
        => index < length && (index >= mask.Length || mask[index] != '?');

    private void BuildHistogram()
    {
        Array.Clear(_pairHistogram, 0, _pairHistogram.Length);

        var end = _module.Length - 1;
        long samples = 0;

        for (var i = 0; i < end; i += HistogramSampleStep)
        {
            var key = _module[i] | (_module[i + 1] << 8);
            _pairHistogram[key]++;
            samples++;
        }

        _histogramSamples = samples;
        _histogramBuilt = true;
    }

    private void AssignAnchors()
    {
        var needHistogram = _requests.Any(r => r.Pattern.AnchorOffset < 0);
#if DEBUG
        needHistogram = true;
#endif
        if (needHistogram) BuildHistogram();

        long[] singleMarginal = null;

        foreach (var request in _requests)
        {
            var bytes = request.Pattern.Bytes;
            var hardOffset = request.Pattern.AnchorOffset;

            if (hardOffset >= 0 && hardOffset + 1 < bytes.Length)
            {
                AssignPair(request, hardOffset);
                continue;
            }

            var mask = request.Pattern.Mask;
            var length = bytes.Length;
            var bestOffset = -1;
            var bestFrequency = long.MaxValue;

            for (var i = 0; i + 1 < length; i++)
            {
                if (!IsConcrete(mask, i, length) || !IsConcrete(mask, i + 1, length)) continue;

                var frequency = _pairHistogram[bytes[i] | (bytes[i + 1] << 8)];
                if (frequency >= bestFrequency) continue;

                bestFrequency = frequency;
                bestOffset = i;
            }

            if (bestOffset >= 0)
            {
                AssignPair(request, bestOffset);
                continue;
            }

            singleMarginal ??= BuildSingleByteMarginal();
            var bestByteOffset = request.NonWildcardIndices.Length > 0 ? request.NonWildcardIndices[0] : 0;
            var bestByteFrequency = long.MaxValue;

            foreach (var index in request.NonWildcardIndices)
            {
                var frequency = singleMarginal[bytes[index]];
                if (frequency >= bestByteFrequency) continue;

                bestByteFrequency = frequency;
                bestByteOffset = index;
            }

            request.IsSingle = true;
            request.AnchorOffset = bestByteOffset;
            request.AnchorFrequency = bestByteFrequency;
            _hasSingleFallback = true;
            (_singleBuckets[bytes[bestByteOffset]] ??= new List<Request>()).Add(request);
        }
    }

    private void AssignPair(Request request, int offset)
    {
        var bytes = request.Pattern.Bytes;
        var key = bytes[offset] | (bytes[offset + 1] << 8);

        request.AnchorOffset = offset;
        request.IsSingle = false;
        request.AnchorFrequency = _histogramBuilt ? _pairHistogram[key] : -1;

        _bitmap[key >> 3] |= (byte)(1 << (key & 7));
        (_pairBuckets[key] ??= new List<Request>()).Add(request);
    }

    private long[] BuildSingleByteMarginal()
    {
        var marginal = new long[256];
        for (var key = 0; key < _pairHistogram.Length; key++)
            marginal[key & 0xFF] += _pairHistogram[key];
        return marginal;
    }

    private static bool Matches(ref byte bufRef, int bufLen, int start, Request request)
    {
        var bytes = request.Pattern.Bytes;
        if (start + bytes.Length > bufLen) return false;

        foreach (var index in request.NonWildcardIndices)
        {
            if (Unsafe.Add(ref bufRef, start + index) != bytes[index]) return false;
        }

        return true;
    }

    private static bool AcceptOccurrence(Request request, int[] matchCounts)
        => matchCounts[request.Id]++ >= request.Pattern.OccurrenceIndex;

    private void ResolveAndInvoke(Request request, int startIndex)
    {
        var pattern = request.Pattern;
        var instructionAddress = _moduleBase + startIndex + pattern.InstructionOffset;

        var final = pattern.AddressingMode switch
        {
            AddressingMode.Absolute => instructionAddress,
            AddressingMode.Direct32 => (nint)(uint)ReadInt32(instructionAddress + pattern.OffsetLocation),
            _ => instructionAddress + ReadInt32(instructionAddress + pattern.OffsetLocation) + pattern.InstructionLength
        };

        _savedAddresses[request.Name] = final;
        request.Setter(final);
    }

    private int ReadInt32(nint address)
    {
        var index = (int)(address - _moduleBase);
        return Unsafe.ReadUnaligned<int>(ref _module[index]);
    }

#if DEBUG
    private void LogAnchors()
    {
        var scale = (double)HistogramSampleStep;
        var highFrequencyAnchors = _requests
            .Select(request =>
            {
                var ppm = _histogramSamples > 0
                    ? request.AnchorFrequency / (double)_histogramSamples * 1_000_000
                    : 0;
                var estimatedCount = (long)(request.AnchorFrequency * scale);
                return (Request: request, Ppm: ppm, EstimatedCount: estimatedCount);
            })
            .Where(anchor => anchor.Ppm >= AnchorLogPpmThreshold)
            .OrderByDescending(anchor => anchor.Ppm)
            .ToList();

        if (highFrequencyAnchors.Count == 0) return;

        Console.WriteLine($"[AobScanner] --- high-frequency anchors >= {AnchorLogPpmThreshold:F0} ppm ---");
        Console.WriteLine("[AobScanner]   freq(ppm)  count~   combo      off  name");

        foreach (var anchor in highFrequencyAnchors)
        {
            var request = anchor.Request;

            if (request.IsSingle)
            {
                var b = request.Pattern.Bytes[request.AnchorOffset];
                Console.WriteLine(
                    $"[AobScanner]   {anchor.Ppm,8:F1}  {anchor.EstimatedCount,8}  0x{b:X2}(1byte) {request.AnchorOffset,3}   {request.Name}  <-- SINGLE-BYTE FALLBACK");
                continue;
            }

            var b0 = request.Pattern.Bytes[request.AnchorOffset];
            var b1 = request.Pattern.Bytes[request.AnchorOffset + 1];
            Console.WriteLine(
                $"[AobScanner]   {anchor.Ppm,8:F1}  {anchor.EstimatedCount,8}  0x{b0:X2} 0x{b1:X2}  {request.AnchorOffset,3}   {request.Name}");
        }
    }
#endif

    private sealed class Request
    {
        public Request(int id, string name, Pattern pattern, Action<nint> setter)
        {
            Id = id;
            Name = name;
            Pattern = pattern;
            Setter = setter;
            NonWildcardIndices = BuildNonWildcardIndices(pattern);
        }

        public int Id { get; }
        public string Name { get; }
        public Pattern Pattern { get; }
        public Action<nint> Setter { get; }
        public int[] NonWildcardIndices { get; }
        public int AnchorOffset;
        public long AnchorFrequency = -1;
        public bool IsSingle;

        private static int[] BuildNonWildcardIndices(Pattern pattern)
        {
            var length = pattern.Bytes.Length;
            var indices = new List<int>(length);

            for (var i = 0; i < length; i++)
            {
                if (IsConcrete(pattern.Mask, i, length)) indices.Add(i);
            }

            return indices.ToArray();
        }
    }
}