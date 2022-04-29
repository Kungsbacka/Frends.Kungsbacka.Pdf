using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace Frends.Kungsbacka.Pdf
{
    internal static class ImageTools
    {
        /// <summary>
        /// Rotates an image if Exif data is found and orientation indícates
        /// a rotation and/or flip is needed to display the image correctly.
        /// 
        /// If input does not represent an image supported by ImageSharp,
        /// null is returned.
        /// </summary>
        /// <param name="imageBytes">Image data</param>
        /// <returns>Image data as Jpeg if the image has been rotated, 
        /// otherwise the image data is returned unchanged.</returns>
        public static byte[] RotateImage(byte[] imageBytes)
        {
            if (Image.DetectFormat(imageBytes) == null)
            {
                return null;
            }
            using var image = Image.Load(imageBytes, out var imageFormat);
            // AutoOrient relies on the orientation tag in the Exif metadata.
            // If no metadata is found, if the metadata doesn't contain any
            // information about orientation or if the current orientation
            // is already correct, the image will not be mutated.
            image.Mutate(x => x.AutoOrient());
            using var result = new MemoryStream();
            image.Save(result, imageFormat);
            return result.ToArray();
        }
    }
}
