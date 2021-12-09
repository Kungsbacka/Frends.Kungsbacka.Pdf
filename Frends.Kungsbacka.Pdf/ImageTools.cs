using System;
using System.Drawing;
using System.Linq;
using System.IO;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor;
using System.Drawing.Imaging;

namespace Frends.Kungsbacka.Pdf
{
    internal static class ImageTools
    {
        /// <summary>
        /// Rotates an image if Exif data is found and orientation indícates
        /// a rotation and/or flip is needed to display the image correctly.
        /// 
        /// The only common image formats that I found that uses Exif are
        /// Jpeg and Tiff. This method does not try to distinguish between
        /// the two and always return a Jpeg if a rotation is performed.
        /// </summary>
        /// <param name="imageBytes">Image data</param>
        /// <returns>Image data as Jpeg if the image has been rotated, otherwise the image data is returned unchanged.</returns>
        public static byte[] RotateImage(byte[] imageBytes)
        {
            RotateFlipType rotateFlip;
            try
            {
                rotateFlip = CalculateTransformation(imageBytes); // ImageProcessingException "File format could not be determined"
            }
            catch (ImageProcessingException ex)
            {
                throw NewInvalidImageDataException(ex);
            }
            if (rotateFlip == RotateFlipType.RotateNoneFlipNone)
            {
                return imageBytes;
            }
            using var sourceStream = new MemoryStream(imageBytes);
            Image image = null;
            try
            {
                image = Image.FromStream(sourceStream); // ArgumentException "The stream does not have a valid image format"
            }
            catch (ArgumentException ex)
            {
                throw NewInvalidImageDataException(ex);
            }
            image.RotateFlip(rotateFlip);
            using var destStream = new MemoryStream();
            try
            {
                image.Save(destStream, ImageFormat.Jpeg); // ExternalException "The image was saved with the wrong image format"
            }
            catch(System.Runtime.InteropServices.ExternalException ex)
            {
                throw NewInvalidImageDataException(ex);
            }
            return destStream.ToArray();
        }

        // Based on this answer on Stack Overflow: https://stackoverflow.com/a/48347653
        private static RotateFlipType CalculateTransformation(byte[] imageBytes)
        {
            int orientation = GetOrientation(imageBytes);
            RotateFlipType rotateFlip = RotateFlipType.RotateNoneFlipNone;
            switch (orientation)
            {
                case 3:
                case 4: rotateFlip = RotateFlipType.Rotate180FlipNone; break;
                case 5:
                case 6: rotateFlip = RotateFlipType.Rotate90FlipNone; break;
                case 7:
                case 8: rotateFlip = RotateFlipType.Rotate270FlipNone; break;
            }
            if (orientation == 2 || orientation == 4 || orientation == 5 || orientation == 7)
            {
                rotateFlip |= RotateFlipType.RotateNoneFlipX;
            }
            return rotateFlip;
        }

        // https://exiftool.org/TagNames/EXIF.html
        // 0 = No orientation information found
        // 1 = Horizontal (normal)
        // 2 = Mirror horizontal
        // 3 = Rotate 180
        // 4 = Mirror vertical
        // 5 = Mirror horizontal and rotate 270 CW
        // 6 = Rotate 90 CW
        // 7 = Mirror horizontal and rotate 90 CW
        // 8 = Rotate 270 CW
        private static int GetOrientation(byte[] imageBytes)
        {
            using var ms = new MemoryStream(imageBytes);
            //try
            {
                var md = ImageMetadataReader.ReadMetadata(ms);
                var exif = md.OfType<ExifIfd0Directory>().FirstOrDefault();
                if (exif != null)
                {
                    return exif.GetInt16(ExifDirectoryBase.TagOrientation);
                }

            }
            //catch (ImageProcessingException)
            //{
            //    throw new ArgumentException("Supplied image data cannot be converted to an image.");
            //}
            return 0;
        }

        private static InvalidImageDataException NewInvalidImageDataException(Exception innerException)
        {
            return new InvalidImageDataException("Byte array does not represent a valid image", innerException);
        }
    }
}
