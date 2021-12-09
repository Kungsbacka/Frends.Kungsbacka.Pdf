using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Frends.Kungsbacka.Pdf
{
    /// <summary>
    /// Thrown if the byte array does not contain valid image data
    /// </summary>
    public class InvalidImageDataException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner Exeception</param>
        public InvalidImageDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
