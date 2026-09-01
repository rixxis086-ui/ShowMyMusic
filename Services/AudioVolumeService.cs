using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ShowMyMusic.Services
{
    public class AudioVolumeService : IDisposable
    {
        public event EventHandler<int>? VolumeChanged;
        private IAudioEndpointVolume? _endpointVolume;
        private AudioEndpointVolumeCallback? _callback;
        private int _currentVolume = 50;

        public int CurrentVolume => _currentVolume;

        public void Initialize()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
                if (device != null)
                {
                    Guid iidIAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                    device.Activate(ref iidIAudioEndpointVolume, CLSCTX.ALL, IntPtr.Zero, out var endpointObj);
                    _endpointVolume = endpointObj as IAudioEndpointVolume;

                    if (_endpointVolume != null)
                    {
                        _endpointVolume.GetMasterVolumeLevelScalar(out float level);
                        _currentVolume = (int)Math.Round(level * 100);

                        _callback = new AudioEndpointVolumeCallback(this);
                        _endpointVolume.RegisterControlChangeNotify(_callback);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AudioVolumeService init error: {ex.Message}");
            }
        }

        internal void OnVolumeNotify(float newVolume)
        {
            int volPercent = (int)Math.Round(newVolume * 100);
            if (_currentVolume != volPercent)
            {
                _currentVolume = volPercent;
                VolumeChanged?.Invoke(this, volPercent);
            }
        }

        public void Dispose()
        {
            try
            {
                if (_endpointVolume != null && _callback != null)
                {
                    _endpointVolume.UnregisterControlChangeNotify(_callback);
                }
            }
            catch { }
        }

        #region COM Interfaces
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator { }

        private enum EDataFlow { eRender, eCapture, eAll }
        private enum ERole { eConsole, eMultimedia, eCommunications }

        [Flags]
        private enum CLSCTX { ALL = 23 }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IntPtr ppDevices);
            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
            int GetDevice(string pwstrId, out IMMDevice ppDevice);
            int RegisterEndpointNotificationCallback(IntPtr pClient);
            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
            int GetId(out string ppstrId);
            int GetState(out int pdwState);
        }

        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioEndpointVolume
        {
            [PreserveSig]
            int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
            [PreserveSig]
            int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
            int GetChannelCount(out int pnChannelCount);
            int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
            int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
            int GetMasterVolumeLevel(out float pfLevelDB);
            [PreserveSig]
            int GetMasterVolumeLevelScalar(out float pfLevel);
            int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
            int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
            int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
            int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
            int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
            int VolumeStepUp(ref Guid pguidEventContext);
            int VolumeStepDown(ref Guid pguidEventContext);
            int QueryHardwareSupport(out uint pdwHardwareSupportMask);
            int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        [Guid("657804FA-D6AD-4496-8560-E5D56D341390"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioEndpointVolumeCallback
        {
            [PreserveSig]
            int OnNotify(IntPtr pNotify);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AUDIO_VOLUME_NOTIFICATION_DATA
        {
            public Guid guidEventContext;
            public bool bMuted;
            public float fMasterVolume;
            public uint nChannels;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public float[] afChannelVolumes;
        }

        private class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
        {
            private readonly AudioVolumeService _service;
            public AudioEndpointVolumeCallback(AudioVolumeService service) => _service = service;

            public int OnNotify(IntPtr pNotify)
            {
                try
                {
                    var data = Marshal.PtrToStructure<AUDIO_VOLUME_NOTIFICATION_DATA>(pNotify);
                    _service.OnVolumeNotify(data.fMasterVolume);
                }
                catch { }
                return 0;
            }
        }
        #endregion
    }
}
