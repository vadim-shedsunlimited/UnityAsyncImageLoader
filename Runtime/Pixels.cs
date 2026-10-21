internal interface IPixel<TChannel> where TChannel : struct {
    static int ChannelCount() => throw new System.NotImplementedException();
    static int ChannelByteCount() => throw new System.NotImplementedException();
}

internal struct R16Pixel : IPixel<short> {
    public static int ChannelCount() => 1;
    public static int ChannelByteCount() => sizeof(short);

    public short r;
}

internal struct RFloatPixel : IPixel<float> {
    public static int ChannelCount() => 1;
    public static int ChannelByteCount() => sizeof(float);

    public float r;
}

internal struct RGB24Pixel : IPixel<byte> {
    public static int ChannelCount() => 3;
    public static int ChannelByteCount() => sizeof(byte);

    public byte r, g, b;
}

internal struct RGB48Pixel : IPixel<short> {
    public static int ChannelCount() => 3;
    public static int ChannelByteCount() => sizeof(short);

    public short r, g, b;
}

internal struct RGBA32Pixel : IPixel<byte> {
    public static int ChannelCount() => 4;
    public static int ChannelByteCount() => sizeof(byte);

    public byte r, g, b, a;
}

internal struct RGBA64Pixel : IPixel<short> {
    public static int ChannelCount() => 4;
    public static int ChannelByteCount() => sizeof(short);

    public short r, g, b, a;
}

internal struct RGBAFloatPixel : IPixel<float> {
    public static int ChannelCount() => 4;
    public static int ChannelByteCount() => sizeof(float);

    public float r, g, b, a;
}