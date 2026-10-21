using Unity.Collections;
using UnityEngine;

public struct RawImageData {
    public NativeArray<byte> data;
    public TextureFormat format;
    public int width;
    public int height;


    public RawImageData(NativeArray<byte> data, TextureFormat format, int width, int height) {
        this.data = data;
        this.format = format;
        this.width = width;
        this.height = height;
    }
}