public static class BitHelper
{
    // Stores the given value in storage at the given index
    public static int Set(int storage, int value, int index)
    {
        int shiftCount = index * 5,
            mask = 0x1f << shiftCount;

        return (storage & ~mask) | (value << shiftCount);
    }

    // Retrieves the value stored in storage at the given index
    public static int Get(int storage, int index)
    {
        int shiftCount = index * 5,
            mask = 0x1f << shiftCount;

        return (storage & mask) >> shiftCount;
    }
}
