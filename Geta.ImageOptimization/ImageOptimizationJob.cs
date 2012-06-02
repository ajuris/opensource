using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using EPiServer;
using EPiServer.BaseLibrary.Scheduling;
using EPiServer.Data;
using EPiServer.PlugIn;
using EPiServer.Web.Hosting;
using Geta.ImageOptimization.Configuration;
using Geta.ImageOptimization.Implementations;
using Geta.ImageOptimization.Interfaces;
using Geta.ImageOptimization.Messaging;
using Geta.ImageOptimization.Models;

namespace Geta.ImageOptimization
{
    [ScheduledPlugIn(DisplayName = "Geta Image Optimization")]
    public class ImageOptimizationJob : JobBase
    {
        private bool _stop;
        private readonly IImageOptimization _imageOptimization;
        private readonly IImageLogRepository _imageLogRepository;

        public ImageOptimizationJob() : this(new Implementations.ImageOptimization(), new ImageLogRepository())
        {
            IsStoppable = true;
        }

        public ImageOptimizationJob(IImageOptimization imageOptimization, IImageLogRepository imageLogRepository)
        {
            this._imageOptimization = imageOptimization;
            this._imageLogRepository = imageLogRepository;
        }

        public override string Execute()
        {
            int count = 0;
            int totalBytesBefore = 0;
            int totalBytesAfter = 0;
            string siteUrl = UriSupport.SiteUrl.ToString();

            var allProviders = VirtualPathHandler.Instance.VirtualPathProviders.Where(p => p.Key.GetType() == typeof(VirtualPathVersioningProvider));

            try
            {
                if (!string.IsNullOrEmpty(ImageOptimizationSettings.Settings.SiteUrl))
                {
                    siteUrl = ImageOptimizationSettings.Settings.SiteUrl;
                }

                if (!string.IsNullOrEmpty(ImageOptimizationSettings.Settings.VirtualNames))
                {
                    allProviders = FilteredVirtualPathProviders(allProviders);
                }
            }
            catch
            {
                // no configuration could be loaded
            }

            foreach (KeyValuePair<VirtualPathProvider, ProviderSettings> vpp in allProviders)
            {
                if (_stop)
                {
                    return string.Format("Job stopped after optimizing {0} images.", count);
                }

                var rootFolder = VirtualPathHandler.Instance.GetDirectory(vpp.Value.Parameters["virtualPath"], true) as UnifiedDirectory;

                var images = new HashSet<string>();

                GetImages(images, rootFolder);

                // remove previously optimized/checked images
                images = new HashSet<string>(images.Where(virtualPath => this._imageLogRepository.GetLogEntry(VirtualPathUtility.RemoveTrailingSlash(siteUrl) + virtualPath) == null));

                foreach (string virtualPath in images)
                {
                    if (_stop)
                    {
                        return string.Format("Job completed after optimizing: {0} images. Before: {1} KB, after: {2} KB.", count, totalBytesBefore / 1024, totalBytesAfter / 1024);
                    }

                    var imageOptimizationRequest = new ImageOptimizationRequest
                                                       {
                                                           ImageUrl = VirtualPathUtility.RemoveTrailingSlash(siteUrl) + virtualPath
                                                       };

                    ImageOptimizationResponse imageOptimizationResponse = this._imageOptimization.ProcessImage(imageOptimizationRequest);

                    Identity logEntryId = this.AddLogEntry(imageOptimizationResponse, virtualPath);

                    if (imageOptimizationResponse.Successful)
                    {
                        totalBytesBefore += imageOptimizationResponse.OriginalImageSize;

                        if (imageOptimizationResponse.OptimizedImageSize > 0)
                        {
                            totalBytesAfter += imageOptimizationResponse.OptimizedImageSize;
                        }
                        else
                        {
                            totalBytesAfter += imageOptimizationResponse.OriginalImageSize;
                        }

                        var file = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath) as UnifiedFile;

                        if (file == null)
                        {
                            continue;
                        }

                        IVersioningFile versioningFile;
                        byte[] fileContent = imageOptimizationResponse.OptimizedImage;

                        if (file.TryAsVersioningFile(out versioningFile))
                        {
                            if (!versioningFile.IsCheckedOut)
                            {
                                versioningFile.CheckOut();
                            }

                            using (Stream stream = file.Open(FileMode.Create, FileAccess.Write))
                            {
                                stream.Write(fileContent, 0, fileContent.Length);
                            }

                            versioningFile.CheckIn(string.Format("Optimized image with Smush.it. From: {0} KB to: {1} KB. Saved: {2}%", imageOptimizationResponse.OriginalImageSize/1024, imageOptimizationResponse.OptimizedImageSize/1024, imageOptimizationResponse.PercentSaved));

                            this.UpdateLogEntryToOptimized(logEntryId);
                        }

                        count++;
                    }
                }
            }

            return string.Format("Job completed after optimizing: {0} images. Before: {1} KB, after: {2} KB.", count, totalBytesBefore/1024, totalBytesAfter/1024);
        }

        private static IEnumerable<KeyValuePair<VirtualPathProvider, ProviderSettings>> FilteredVirtualPathProviders(IEnumerable<KeyValuePair<VirtualPathProvider, ProviderSettings>> allProviders)
        {
            string[] virtualNames = ImageOptimizationSettings.Settings.VirtualNames.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var provider in allProviders)
            {
                if (virtualNames.Contains(provider.Value.Parameters["virtualName"]))
                {
                    yield return provider;
                }
            }
        }

        private void UpdateLogEntryToOptimized(Identity logEntryId)
        {
            ImageLogEntry logEntry = this._imageLogRepository.GetLogEntry(logEntryId);

            logEntry.IsOptimized = true;

            this._imageLogRepository.Save(logEntry);
        }

        private Identity AddLogEntry(ImageOptimizationResponse imageOptimizationResponse, string virtualPath)
        {
            ImageLogEntry logEntry = this._imageLogRepository.GetLogEntry(imageOptimizationResponse.OriginalImageUrl) ?? new ImageLogEntry();

            logEntry.VirtualPath = virtualPath;
            logEntry.OriginalSize = imageOptimizationResponse.OriginalImageSize;
            logEntry.OptimizedSize = imageOptimizationResponse.OptimizedImageSize;
            logEntry.PercentSaved = imageOptimizationResponse.PercentSaved;
            logEntry.ImageUrl = imageOptimizationResponse.OriginalImageUrl;

            return this._imageLogRepository.Save(logEntry);
        }

        private bool IsImage(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
            {
                return false;
            }

            string fixedFileName = fileExtension.ToLowerInvariant();

            return fixedFileName.EndsWith(".gif") || fileExtension.EndsWith(".png") || fileExtension.EndsWith(".jpg") || fileExtension.EndsWith(".jpeg");
        }

        private void GetImages(HashSet<string> images, UnifiedDirectory directory)
        {
            UnifiedFile[] files = directory.GetFiles();

            foreach (UnifiedFile file in files)
            {
                if (IsImage(file.Extension))
                {
                    images.Add(file.VirtualPath);
                }
            }

            UnifiedDirectory[] subDirectories = directory.GetDirectories();

            foreach (UnifiedDirectory subDirectory in subDirectories)
            {
                GetImages(images, subDirectory);
            }
        }

        public override void Stop()
        {
            _stop = true;
        }
    }
}