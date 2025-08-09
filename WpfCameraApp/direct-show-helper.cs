using DirectShowLib;
using System.Runtime.InteropServices;

namespace WpfCameraApp
{
    public static class DirectShowHelper
    {
        public static List<CameraResolution> GetCameraResolutions(int cameraIndex)
        {
            var resolutions = new List<CameraResolution>();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            if (devices.Length <= cameraIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(cameraIndex),
                    $"Camera index {cameraIndex} is out of range. Only {devices.Length} cameras detected.");
            }

            try
            {
                // Create the filter graph
                IFilterGraph2 filterGraph = new FilterGraph() as IFilterGraph2;
                IBaseFilter capFilter = null;
                IPin pin = null;

                try
                {
                    // Add video input device
                    Guid iid = typeof(IBaseFilter).GUID;
                    object source = null;
                    devices[cameraIndex].Mon.BindToObject(null, null, ref iid, out source);
                    capFilter = source as IBaseFilter;
                    filterGraph.AddFilter(capFilter, "Video Capture");

                    // Get the stream config interface
                    pin = DsFindPin.ByCategory(capFilter, PinCategory.Capture, 0);
                    IAMStreamConfig streamConfig = pin as IAMStreamConfig;

                    // Get number of capabilities
                    int count, size;
                    streamConfig.GetNumberOfCapabilities(out count, out size);

                    // Query each capability
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr ptr = Marshal.AllocCoTaskMem(size);
                        try
                        {
                            AMMediaType mediaType;
                            streamConfig.GetStreamCaps(i, out mediaType, ptr);

                            if (mediaType.formatType == FormatType.VideoInfo)
                            {
                                VideoInfoHeader videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));

                                var resolution = new CameraResolution
                                {
                                    Width = videoInfo.BmiHeader.Width,
                                    Height = videoInfo.BmiHeader.Height
                                };

                                // Avoid duplicates
                                if (!resolutions.Contains(resolution))
                                {
                                    resolutions.Add(resolution);
                                }
                            }
                            DsUtils.FreeAMMediaType(mediaType);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(ptr);
                        }
                    }

                    // Sort resolutions by width then height
                    resolutions.Sort((a, b) =>
                    {
                        int widthCompare = a.Width.CompareTo(b.Width);
                        return widthCompare != 0 ? widthCompare : a.Height.CompareTo(b.Height);
                    });
                }
                finally
                {
                    // Release COM objects
                    if (pin != null) Marshal.ReleaseComObject(pin);
                    if (capFilter != null) Marshal.ReleaseComObject(capFilter);
                    if (filterGraph != null) Marshal.ReleaseComObject(filterGraph);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get camera resolutions. See inner exception for details.", ex);
            }

            if (resolutions.Count == 0)
            {
                throw new ApplicationException("No supported resolutions found for the camera.");
            }

            return resolutions;
        }
    }

    public class CameraResolution : IEquatable<CameraResolution>
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString() => $"{Width} × {Height}";

        public bool Equals(CameraResolution other)
        {
            if (other is null) return false;
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj) => Equals(obj as CameraResolution);

        public override int GetHashCode() => HashCode.Combine(Width, Height);
    }
}