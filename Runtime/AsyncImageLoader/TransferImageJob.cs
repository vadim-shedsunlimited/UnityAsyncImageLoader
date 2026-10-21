using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

public partial class AsyncImageLoader {
    [BurstCompile(CompileSynchronously = true)]
    struct TransferImageToTextureJob : IJobParallelFor {
        public int bytesPerLine;
        public int bytesPerScanline;

        [NativeDisableUnsafePtrRestriction]
        public System.IntPtr bitsPtr;

        [WriteOnly] public NativeSlice<byte> textureData;

        public unsafe void Execute(int rowIndex) {
            UnsafeUtility.MemCpy(
              (byte*)textureData.GetUnsafePtr() + rowIndex * bytesPerLine,
              (bitsPtr + rowIndex * bytesPerScanline).ToPointer(),
              bytesPerLine
            );
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    struct TransferBGR24ImageToRGB24TextureJob : IJobParallelFor {
        public int bytesPerScanline;
        public int width;

        [NativeDisableUnsafePtrRestriction]
        public System.IntPtr bitsPtr;

        [WriteOnly] public NativeSlice<byte> textureData;

        public unsafe void Execute(int rowIndex) {
            // Get typed pointers for the destination and the source
            RGB24Pixel* dst = (RGB24Pixel*)textureData.GetUnsafePtr() + rowIndex * width;
            RGB24Pixel* src = (RGB24Pixel*)((byte*)bitsPtr + rowIndex * bytesPerScanline);

            for (int i = 0; i < width; i++) {
                // Swap Blue and Red channels
                dst[i].r = src[i].b;
                dst[i].g = src[i].g;
                dst[i].b = src[i].r;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    struct TransferBGRA32ImageToRGBA32TextureJob : IJobParallelFor {
        public int bytesPerScanline;
        public int width;

        [NativeDisableUnsafePtrRestriction]
        public System.IntPtr bitsPtr;

        [WriteOnly] public NativeSlice<byte> textureData;

        public unsafe void Execute(int rowIndex) {
            // Get typed pointers for the destination and the source
            RGBA32Pixel* dst = (RGBA32Pixel*)textureData.GetUnsafePtr() + rowIndex * width;
            RGBA32Pixel* src = (RGBA32Pixel*)((byte*)bitsPtr + rowIndex * bytesPerScanline);

            for (int i = 0; i < width; i++) {
                // Swap Blue and Red channels, copy Alpha and Green
                dst[i].r = src[i].b;
                dst[i].g = src[i].g;
                dst[i].b = src[i].r;
                dst[i].a = src[i].a;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    struct TransferRGBFloatImageToRGBAFloatTextureJob : IJobParallelFor {
        public int bytesPerScanline;
        public int width;

        [NativeDisableUnsafePtrRestriction]
        public System.IntPtr bitsPtr;

        [WriteOnly] public NativeSlice<byte> textureData;

        public unsafe void Execute(int rowIndex) {
            var rowData = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<RGBAFloatPixel>(
              (RGBAFloatPixel*)textureData.GetUnsafePtr() + rowIndex * width, sizeof(RGBAFloatPixel), width
            );
            var rowAlphaData = rowData.SliceWithStride<float>(3 * sizeof(float));

            UnsafeUtility.MemCpyStride(
              rowData.GetUnsafePtr(), rowData.Stride,
              (bitsPtr + rowIndex * bytesPerScanline).ToPointer(), 3 * sizeof(float),
              3 * sizeof(float), width
            );

            for (var i = 0; i < rowAlphaData.Length; i++) rowAlphaData[i] = 1f;
        }
    }
}
