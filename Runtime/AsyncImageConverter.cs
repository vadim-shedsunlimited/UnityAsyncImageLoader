using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

internal static class NativeArrayExtensions {
    public static NativeSlice<T> SliceConvert<T>(this NativeArray<byte> array) where T : struct {
        return array.Slice().SliceConvert<T>();
    }
}

public static partial class AsyncImageConverter {
    internal struct ARGB32Pixel : IPixel<byte> {
        public static int ChannelCount() => 4;
        public static int ChannelByteCount() => sizeof(byte);
        public byte a, r, g, b; // ARGB order in memory
    }

    public static JobHandle Convert(RawImageData input, RawImageData output, int batchCount = 64, JobHandle dependency = default) {
        if (input.width != output.width || input.height != output.height) {
            throw new System.ArgumentException("Input and output dimensions must match.");
        }

        switch (input.format) {
            case TextureFormat.RGB24 when output.format == TextureFormat.RGBA32:
                return new ConvertRGB24ToRGBA32Job {
                    src = input.data.SliceConvert<RGB24Pixel>(),
                    dst = output.data.SliceConvert<RGBA32Pixel>(),
                    width = input.width
                }.Schedule(input.height, batchCount, dependency);
            case TextureFormat.RGBA32 when output.format == TextureFormat.RGB24:
                return new ConvertRGBA32ToRGB24Job {
                    src = input.data.SliceConvert<RGBA32Pixel>(),
                    dst = output.data.SliceConvert<RGB24Pixel>(),
                    width = input.width
                }.Schedule(input.height, batchCount, dependency);

            case TextureFormat.ARGB32 when output.format == TextureFormat.RGB24:
                return new ConvertARGB32ToRGB24Job {
                    src = input.data.SliceConvert<ARGB32Pixel>(),
                    dst = output.data.SliceConvert<RGB24Pixel>(),
                    width = input.width
                }.Schedule(input.height, batchCount, dependency);
            case TextureFormat.ARGB32 when output.format == TextureFormat.RGBA32:
                return new ConvertARGB32ToRGBA32Job {
                    src = input.data.SliceConvert<ARGB32Pixel>(),
                    dst = output.data.SliceConvert<RGBA32Pixel>(),
                    width = input.width
                }.Schedule(input.height, batchCount, dependency);
            case TextureFormat.RGB48 when output.format == TextureFormat.RGB24:
                return new ConvertRGB48ToRGB24Job {
                    src = input.data.SliceConvert<RGB48Pixel>(),
                    dst = output.data.SliceConvert<RGB24Pixel>(),
                    width = input.width
                }.Schedule(input.height, batchCount, dependency);
            case TextureFormat.RGB48 when output.format == TextureFormat.RGBA32:
                return new ConvertRGB48ToRGBA32Job {
                    src = input.data.SliceConvert<RGB48Pixel>(),
                    dst = output.data.SliceConvert<RGBA32Pixel>(),
                    width = input.width
                }.Schedule(input.height, batchCount, dependency);
            default:
                throw new System.NotSupportedException($"Image conversion from {input.format} to {output.format} is not currently supported.");
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct ConvertRGB24ToRGBA32Job : IJobParallelFor {
        [ReadOnly] public NativeSlice<RGB24Pixel> src;
        [NativeDisableParallelForRestriction, WriteOnly]
        public NativeSlice<RGBA32Pixel> dst;
        public int width;

        public void Execute(int rowIndex) {
            int rowStart = rowIndex * width;

            for (int x = 0; x < width; ++x) {
                int pixelIndex = rowStart + x;
                var p = src[pixelIndex];
                dst[pixelIndex] = new RGBA32Pixel {
                    r = p.r,
                    g = p.g,
                    b = p.b,
                    a = 255 // Set alpha to fully opaque when converting from RGB to RGBA
                };
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct ConvertRGBA32ToRGB24Job : IJobParallelFor {
        [ReadOnly] public NativeSlice<RGBA32Pixel> src;
        [NativeDisableParallelForRestriction, WriteOnly]
        public NativeSlice<RGB24Pixel> dst;
        public int width;

        public void Execute(int rowIndex) {
            int rowStart = rowIndex * width;
            for (int x = 0; x < width; ++x) {
                int pixelIndex = rowStart + x;
                var p = src[pixelIndex];
                dst[pixelIndex] = new RGB24Pixel {
                    r = p.r,
                    g = p.g,
                    b = p.b
                };
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct ConvertARGB32ToRGB24Job : IJobParallelFor {
        [ReadOnly] public NativeSlice<ARGB32Pixel> src;
        [NativeDisableParallelForRestriction, WriteOnly]
        public NativeSlice<RGB24Pixel> dst;
        public int width;

        public void Execute(int rowIndex) {
            int rowStart = rowIndex * width;
            for (int x = 0; x < width; ++x) {
                int pixelIndex = rowStart + x;
                var p = src[pixelIndex];
                dst[pixelIndex] = new RGB24Pixel {
                    r = p.r,
                    g = p.g,
                    b = p.b
                };
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct ConvertARGB32ToRGBA32Job : IJobParallelFor {
        [ReadOnly] public NativeSlice<ARGB32Pixel> src;
        [NativeDisableParallelForRestriction, WriteOnly]
        public NativeSlice<RGBA32Pixel> dst;
        public int width;
        public void Execute(int rowIndex) {
            int rowStart = rowIndex * width;
            for (int x = 0; x < width; ++x) {
                int pixelIndex = rowStart + x;
                var p = src[pixelIndex];
                dst[pixelIndex] = new RGBA32Pixel {
                    r = p.r,
                    g = p.g,
                    b = p.b,
                    a = p.a
                };
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct ConvertRGB48ToRGBA32Job : IJobParallelFor {
        [ReadOnly] public NativeSlice<RGB48Pixel> src;
        [NativeDisableParallelForRestriction, WriteOnly]
        public NativeSlice<RGBA32Pixel> dst;
        public int width;
        public void Execute(int rowIndex) {
            int rowStart = rowIndex * width;
            for (int x = 0; x < width; ++x) {
                int pixelIndex = rowStart + x;
                var p = src[pixelIndex];
                dst[pixelIndex] = new RGBA32Pixel {
                    r = (byte)(p.r >> 8), // Convert 16-bit to 8-bit by taking the high byte
                    g = (byte)(p.g >> 8),
                    b = (byte)(p.b >> 8),
                    a = 255 // Set alpha to fully opaque when converting from RGB to RGBA
                };
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct ConvertRGB48ToRGB24Job : IJobParallelFor {
        [ReadOnly] public NativeSlice<RGB48Pixel> src;
        [NativeDisableParallelForRestriction, WriteOnly]
        public NativeSlice<RGB24Pixel> dst;
        public int width;

        public void Execute(int rowIndex) {
            int rowStart = rowIndex * width;
            for (int x = 0; x < width; ++x) {
                int pixelIndex = rowStart + x;
                var p = src[pixelIndex];
                dst[pixelIndex] = new RGB24Pixel {
                    r = (byte)(p.r >> 8), // Convert 16-bit to 8-bit by taking the high byte
                    g = (byte)(p.g >> 8),
                    b = (byte)(p.b >> 8)
                };
            }
        }
    }
}