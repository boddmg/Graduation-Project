//------------------------------------------------------------------------------
// <copyright file="KinectColorViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for KinectColorViewer.xaml
    /// </summary>
    public partial class KinectColorViewer : KinectViewer
    {
        private ColorImageFormat lastImageFormat = ColorImageFormat.Undefined;
        private byte[] rawPixelData;
        private byte[] pixelData;
        private WriteableBitmap outputImage;

        public KinectColorViewer()
        {
            InitializeComponent();
        }

        protected override void OnKinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null == args)
            {
                throw new ArgumentNullException("args");
            }

            if (null != args.OldValue)
            {
                args.OldValue.ColorFrameReady -= this.ColorImageReady;

                if (!this.RetainImageOnSensorChange)
                {
                    kinectColorImage.Source = null;
                    this.lastImageFormat = ColorImageFormat.Undefined;
                }
            }

            if ((null != args.NewValue) && (KinectStatus.Connected == args.NewValue.Status))
            {
                ResetFrameRateCounters();

                if (ColorImageFormat.RawYuvResolution640x480Fps15 == args.NewValue.ColorStream.Format)
                {
                    throw new NotImplementedException("RawYuv conversion is not yet implemented.");
                }
                else
                {
                    args.NewValue.ColorFrameReady += this.ColorImageReady;
                }
            }
        }

        private void ConvertBayerToRgb32(int width, int height)
        {
            // Demosaic using a basic nearest-neighbor algorithm, operating on groups of four pixels.
            for (int y = 0; y < height; y += 2)
            {
                for (int x = 0; x < width; x += 2)
                {
                    int firstRowOffset = (y * width) + x;
                    int secondRowOffset = firstRowOffset + width;

                    // Cache the Bayer component values.
                    byte red = rawPixelData[firstRowOffset + 1];
                    byte green1 = rawPixelData[firstRowOffset];
                    byte green2 = rawPixelData[secondRowOffset + 1];
                    byte blue = rawPixelData[secondRowOffset];

                    // Adjust offsets for RGB.
                    firstRowOffset *= 4;
                    secondRowOffset *= 4;

                    // Top left
                    pixelData[firstRowOffset]     = blue;
                    pixelData[firstRowOffset + 1] = green1;
                    pixelData[firstRowOffset + 2] = red;

                    // Top right
                    pixelData[firstRowOffset + 4] = blue;
                    pixelData[firstRowOffset + 5] = green1;
                    pixelData[firstRowOffset + 6] = red;

                    // Bottom left
                    pixelData[secondRowOffset]     = blue;
                    pixelData[secondRowOffset + 1] = green2;
                    pixelData[secondRowOffset + 2] = red;

                    // Bottom right
                    pixelData[secondRowOffset + 4] = blue;
                    pixelData[secondRowOffset + 5] = green2;
                    pixelData[secondRowOffset + 6] = red;
                }
            }
        }

        private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame != null)
                {
                    // We need to detect if the format has changed.
                    bool haveNewFormat = this.lastImageFormat != imageFrame.Format;
                    bool convertToRgb = false;
                    int bytesPerPixel = imageFrame.BytesPerPixel;

                    if (imageFrame.Format == ColorImageFormat.RawBayerResolution640x480Fps30 ||
                        imageFrame.Format == ColorImageFormat.RawBayerResolution1280x960Fps12)
                    {
                        convertToRgb = true;
                        bytesPerPixel = 4;
                    }

                    if (haveNewFormat)
                    {
                        if (convertToRgb)
                        {
                            this.rawPixelData = new byte[imageFrame.PixelDataLength];
                            this.pixelData = new byte[bytesPerPixel * imageFrame.Width * imageFrame.Height];
                        }
                        else
                        {
                            this.pixelData = new byte[imageFrame.PixelDataLength];
                        }
                    }

                    if (convertToRgb)
                    {
                        imageFrame.CopyPixelDataTo(this.rawPixelData);
                        ConvertBayerToRgb32(imageFrame.Width, imageFrame.Height);
                    }
                    else
                    {
                        imageFrame.CopyPixelDataTo(this.pixelData);
                    }

                    // A WriteableBitmap is a WPF construct that enables resetting the Bits of the image.
                    // This is more efficient than creating a new Bitmap every frame.
                    if (haveNewFormat)
                    {
                        PixelFormat format = PixelFormats.Bgr32;
                        if (imageFrame.Format == ColorImageFormat.InfraredResolution640x480Fps30)
                        {
                            format = PixelFormats.Gray16;
                        }

                        kinectColorImage.Visibility = Visibility.Visible;
                        this.outputImage = new WriteableBitmap(
                            imageFrame.Width,
                            imageFrame.Height,
                            96,  // DpiX
                            96,  // DpiY
                            format,
                            null);

                        this.kinectColorImage.Source = this.outputImage;
                    }

                    this.outputImage.WritePixels(
                        new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                        this.pixelData,
                        imageFrame.Width * bytesPerPixel,
                        0);

                    this.lastImageFormat = imageFrame.Format;

                    UpdateFrameRate();
                }
            }
        }
    }
}
