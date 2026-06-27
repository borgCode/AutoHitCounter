// 

namespace AutoHitCounter.Memory.Patterns;

public class Pattern(
    byte[] bytes,
    string mask,
    int instructionOffset,
    AddressingMode addressingMode,
    int offsetLocation = 0,
    int instructionLength = 0,
    int anchorOffset = -1,
    int occurrenceIndex = 0)
{
    public byte[] Bytes { get; } = bytes;
    public string Mask { get; } = mask;
    public int InstructionOffset { get; } = instructionOffset;
    public AddressingMode AddressingMode { get; } = addressingMode;
    public int OffsetLocation { get; } = offsetLocation;
    public int InstructionLength { get; } = instructionLength;
    public int AnchorOffset { get; } = anchorOffset;
    public int OccurrenceIndex { get; } = occurrenceIndex;
}