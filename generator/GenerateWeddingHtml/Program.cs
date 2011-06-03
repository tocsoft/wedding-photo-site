using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using gma.Drawing.ImageInfo;
using System.Drawing.Imaging;

namespace GenerateWeddingHtml
{
    class Program
    {
        const string pictureThumbHtml = @"<img data-src='images/thumbs/{0}/{1}' id='{5}' title='{2}' data-attribution='{3}'  data-attribution-url='{4}' href='images/mains/{0}/{1}'/>";
        const string albumHtml = @"<li class='album'>
					<span class='st_link'><span class='st_arrow_down'>{0}</span></span>
					<div class='st_wrapper st_thumbs_wrapper'>
						<div class='st_thumbs'>
                            {1}
						</div>
					</div>
				</li>";

        

        static void Main(string[] args)
        {
            string dir = @"..\..\..\..\albums";
            string thumbs = @"..\..\..\..\Site\images\thumbs";
            string mains = @"..\..\..\..\Site\images\mains";

            StringBuilder sb = new StringBuilder();

            var albumbs = Directory.GetDirectories(dir);
            foreach (var a_path in albumbs)
            {
                var albumDirName = Path.GetFileName(a_path);
                var albumName = albumDirName.Split('.').Last().Trim();

                
                StringBuilder thumbshtml = new StringBuilder();

                foreach (var img_path in Directory.GetFiles(a_path))
                {
                    var imageName = Path.GetFileName(img_path);
                   
                    Info inf = new Info(img_path);
                    var artist_details = (inf.Artists ?? new string[]{});
                    var artist = artist_details.FirstOrDefault() ?? "";
                    var albumFoldername = albumName.Replace(' ', '_');
                    var aUrl = artist_details.Select(x => x.Trim()).Where(x => x.StartsWith("http://") || x.StartsWith("https://") || x.StartsWith("mailto:")).FirstOrDefault() ?? "";
                    var imgId = albumFoldername + "_" + Path.GetFileNameWithoutExtension(img_path);

                    thumbshtml.AppendLine(string.Format(pictureThumbHtml, albumFoldername, imageName, inf.Title ?? imageName, artist, aUrl, imgId));
                   

                    using (Image img = inf.Image)
                    {

                        using (var smallImg = Resize(img, new Size(180, 120), ResizeMode.Normal, Color.Black, true))
                        {
                            var dirInfo = new DirectoryInfo(thumbs);
                            var thumbImagePathDir = Path.Combine(dirInfo.FullName, albumFoldername);


                            var thumbImagePath = Path.Combine(thumbImagePathDir, Path.GetFileNameWithoutExtension(imageName) + ".jpg");


                            if (!Directory.Exists(thumbImagePathDir))
                                Directory.CreateDirectory(thumbImagePathDir);


                            SaveJpeg(thumbImagePath, smallImg, 75);

                        }
                        using (var largeImage = Resize(img, new Size(600, 800), ResizeMode.Normal, Color.Black, true))
                        {
                            var dirInfo = new DirectoryInfo(mains);
                            var largeImagePathDir = Path.Combine(dirInfo.FullName, albumFoldername);


                            var largeImagePath = Path.Combine(largeImagePathDir, Path.GetFileNameWithoutExtension(imageName)+".jpg");


                            if (!Directory.Exists(largeImagePathDir))
                                Directory.CreateDirectory(largeImagePathDir);

                            
                            SaveJpeg(largeImagePath, largeImage, 90);
                        }
                    }

                }

                sb.AppendLine(string.Format(albumHtml, albumName, thumbshtml));
            }

            var html = File.ReadAllText("index.html").Replace("<!--INSERT ALBUM HTML HERE-->", sb.ToString());
            File.WriteAllText(@"..\..\..\..\Site\index.html", html);
        }

        public static void SaveJpeg(string path, Image img, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");


            // Encoder parameter for image quality 
            EncoderParameter qualityParam =
                new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            // Jpeg image codec 
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        /// <summary> 
        /// Returns the image codec with the given mime type 
        /// </summary> 
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        } 
        ///
        /// Modes available for the resize method of images.
        ///
        public enum ResizeMode
        {
            ///
            /// This will resize images to the resolution nearest to the target resolution. Images can become smaller when using this option
            ///
            Normal = 1,
            ///
            /// This will stretch an image so it always is the exact dimensions of the target resolution
            ///
            Stretch = 2,
            ///
            /// This will size an image to the exact dimensions of the target resolution, keeping ratio in mind and cropping parts that can't
            /// fit in the picture.
            ///
            Crop = 3,
            ///
            /// This will size an image to the exact dimensions of the target resolution, keeping ratio in mind and filling up the image
            /// with black bars when some parts remain empty.
            ///
            Fill = 4
        }

        ///
        /// Method resizes the image so it fits the wanted resolution best.
        ///
        ///The image to be resized
        ///The resolution which the image should be when the method is ready.
        ///The mode for resizing the image.
        public static System.Drawing.Image Resize(System.Drawing.Image image, Size targetResolution, ResizeMode resizeMode, System.Drawing.Color? background, bool rotate)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            int targetWidth = targetResolution.Width;
            int targetHeight = targetResolution.Height;
            var fillBackground = background.HasValue? background.Value : Color.Transparent;
            
            // Supplied image is landscape, while the target resolution is portait OR
            // supplied image is in portait, while the target resolution is in landscape.
            // switch target resolution to match the image.
            if ((sourceWidth > sourceHeight && targetWidth < targetHeight) || (sourceWidth < sourceHeight && targetWidth > targetHeight))
            {
                targetWidth = targetResolution.Height;
                targetHeight = targetResolution.Width;
            }
            
            float ratio = 0;
            float ratioWidth = ((float)targetWidth / (float)sourceWidth);
            float ratioHeight = ((float)targetHeight / (float)sourceHeight);
            if (ratioHeight < ratioWidth)
                ratio = ratioHeight;
            else
                ratio = ratioWidth;
            Bitmap newImage = null;
            switch (resizeMode)
            {
                case ResizeMode.Normal:
                default:
                    {
                        int destWidth = (int)(sourceWidth * ratio);
                        int destHeight = (int)(sourceHeight * ratio);
                        newImage = new Bitmap(destWidth, destHeight);
                        using (Graphics graphics = Graphics.FromImage((System.Drawing.Image)newImage))
                        {
                            graphics.Clear(fillBackground);
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, destWidth, destHeight);
                        }
                        break;
                    }
                case ResizeMode.Crop:
                    {
                        if (ratioHeight > ratioWidth)
                            ratio = ratioHeight;
                        else
                            ratio = ratioWidth;
                        int destWidth = (int)(sourceWidth * ratio);
                        int destHeight = (int)(sourceHeight * ratio);
                        newImage = new Bitmap(targetWidth, targetHeight);
                        int startX = 0;
                        int startY = 0;
                        if (destWidth > targetWidth)
                            startX = 0 - ((destWidth - targetWidth) / 2);
                        if (destHeight > targetHeight)
                            startY = 0 - ((destHeight - targetHeight) / 2);
                        using (Graphics graphics = Graphics.FromImage((System.Drawing.Image)newImage))
                        {
                            graphics.Clear(fillBackground);
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, startX, startY, destWidth, destHeight);
                        }
                        break;
                    }
                case ResizeMode.Stretch:
                    {
                        newImage = new Bitmap(targetWidth, targetHeight);
                        using (Graphics graphics = Graphics.FromImage((System.Drawing.Image)newImage))
                        {
                            graphics.Clear(fillBackground);
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, targetWidth, targetHeight);
                        }
                        break;
                    }
                case ResizeMode.Fill:
                    {
                        newImage = new Bitmap(targetWidth, targetHeight);
                        int destWidth = (int)(sourceWidth * ratio);
                        int destHeight = (int)(sourceHeight * ratio);
                        int startX = 0;
                        int startY = 0;
                        if (destWidth < targetWidth)
                            startX = 0 + ((targetWidth - destWidth) / 2);
                        if (destHeight < targetHeight)
                            startY = 0 + ((targetHeight - destHeight) / 2);
                        newImage = new Bitmap(targetWidth, targetHeight);
                        using (Graphics graphics = Graphics.FromImage((System.Drawing.Image)newImage))
                        {
                            graphics.Clear(fillBackground);
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, startX, startY, destWidth, destHeight);
                        }
                        break;
                    }
            }
            return (System.Drawing.Image)newImage;
        }
    }
}
