// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Mediapipe.Unity
{
    public class WebCamSource : ImageSource
    {
        private readonly int _preferableDefaultWidth = 1280;

        private const string _TAG = nameof(WebCamSource);

        private readonly ResolutionStruct[] _defaultAvailableResolutions;

        public WebCamSource(int preferableDefaultWidth, ResolutionStruct[] defaultAvailableResolutions)
        {
            _preferableDefaultWidth = preferableDefaultWidth;
            _defaultAvailableResolutions = defaultAvailableResolutions;
        }

        private static readonly object _PermissionLock = new object();
        private static bool _IsPermitted = false;

        private WebCamTexture _webCamTexture;
        private WebCamTexture webCamTexture
        {
            get => _webCamTexture;
            set
            {
                if (_webCamTexture != null)
                {
                    _webCamTexture.Stop();
                }
                _webCamTexture = value;
            }
        }

        //public override int textureWidth => !isPrepared ? 0 : webCamTexture.width;
        //public override int textureHeight => !isPrepared ? 0 : webCamTexture.height;

        public override int textureWidth => MinRezValue;
        public override int textureHeight => MaxRezValue;

        //public override bool isVerticallyFlipped => isPrepared && webCamTexture.videoVerticallyMirrored;
        public override bool isVerticallyFlipped => false;
        //public override bool isFrontFacing => isPrepared && (webCamDevice is WebCamDevice valueOfWebCamDevice) && valueOfWebCamDevice.isFrontFacing;
        public override bool isFrontFacing => true;
        //public override RotationAngle rotation => !isPrepared ? RotationAngle.Rotation0 : (RotationAngle)webCamTexture.videoRotationAngle;
        public override RotationAngle rotation => 0;

        private WebCamDevice? _webCamDevice;
        private WebCamDevice? webCamDevice
        {
            get => _webCamDevice;
            set
            {
                if (_webCamDevice is WebCamDevice valueOfWebCamDevice)
                {
                    if (value is WebCamDevice valueOfValue && valueOfValue.name == valueOfWebCamDevice.name)
                    {
                        // not changed
                        return;
                    }
                }
                else if (value == null)
                {
                    // not changed
                    return;
                }
                _webCamDevice = value;
                resolution = GetDefaultResolution();
            }
        }
        public override string sourceName => (webCamDevice is WebCamDevice valueOfWebCamDevice) ? valueOfWebCamDevice.name : null;

        private WebCamDevice[] _availableSources;
        private WebCamDevice[] availableSources
        {
            get
            {
                if (_availableSources == null)
                {
                    _availableSources = WebCamTexture.devices;
                }

                return _availableSources;
            }
            set => _availableSources = value;
        }

        public override string[] sourceCandidateNames => availableSources?.Select(device => device.name).ToArray();

#pragma warning disable IDE0025
        public override ResolutionStruct[] availableResolutions
        {
            get
            {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (webCamDevice is WebCamDevice valueOfWebCamDevice) {
          return valueOfWebCamDevice.availableResolutions.Select(resolution => new ResolutionStruct(resolution)).ToArray();
        }
#endif
                return webCamDevice == null ? null : _defaultAvailableResolutions;
            }
        }
#pragma warning restore IDE0025

        //public override bool isPrepared => webCamTexture != null;
        //public override bool isPlaying => webCamTexture != null && webCamTexture.isPlaying;
        public override bool isPrepared => true;
        public override bool isPlaying => true;

        ARCameraManager arCameraManager;
        ARCameraBackground arCameraBackground;
        [SerializeField]
        private int maxCustomResolution = 700;
        int MaxRezValue;
        int MinRezValue;
        float inputFrameScale;
        private IEnumerator Initialize()
        {
            //yield return GetPermission();
            inputFrameScale = GetInputScaleValue((int)Math.Max(UnityEngine.Screen.width, UnityEngine.Screen.height));

            MaxRezValue = (int)(Math.Max(UnityEngine.Screen.width, UnityEngine.Screen.height) * inputFrameScale);
            MinRezValue = (int)(Math.Min(UnityEngine.Screen.width, UnityEngine.Screen.height) * inputFrameScale);

            //if (!_IsPermitted)
            //{
            //    yield break;
            //}

            if (/*webCamDevice != null || */!Camera.main.TryGetComponent<ARCameraManager>(out var arCameraManager) || !Camera.main.TryGetComponent<ARCameraBackground>(out var arCameraBackground))
            {
                yield break;
            }

            this.arCameraManager = arCameraManager;
            this.arCameraBackground = arCameraBackground;
            //availableSources = WebCamTexture.devices;

            //if (availableSources != null && availableSources.Length > 0)
            //{
            //    webCamDevice = availableSources[0];
            //}
        }
        private float GetInputScaleValue(int maxScreenValue)
        {
            float result;
            result = (float)maxCustomResolution / (float)maxScreenValue;
            result = Mathf.Round(result * 100f) / 100f;
            return result;
        }

        private IEnumerator GetPermission()
        {
            lock (_PermissionLock)
            {
                if (_IsPermitted)
                {
                    yield break;
                }

#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    Permission.RequestUserPermission(Permission.Camera);
                    yield return new WaitForSeconds(0.1f);
                }
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) {
          yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }
#endif

#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    Debug.LogWarning("Not permitted to use Camera");
                    yield break;
                }
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) {
          Debug.LogWarning("Not permitted to use WebCam");
          yield break;
        }
#endif
                _IsPermitted = true;

                yield return new WaitForEndOfFrame();
            }
        }

        public override void SelectSource(int sourceId)
        {
            if (sourceId < 0 || sourceId >= availableSources.Length)
            {
                throw new ArgumentException($"Invalid source ID: {sourceId}");
            }

            webCamDevice = availableSources[sourceId];
        }

        public override IEnumerator Play()
        {
            yield return Initialize();
            //if (!_IsPermitted)
            //{
            //    throw new InvalidOperationException("Not permitted to access cameras");
            //}

            //InitializeWebCamTexture();
            //webCamTexture.Play();
            //yield return WaitForWebCamTexture();
        }

        public override IEnumerator Resume()
        {
            yield break;
            if (!isPrepared)
            {
                throw new InvalidOperationException("WebCamTexture is not prepared yet");
            }
            if (!webCamTexture.isPlaying)
            {
                webCamTexture.Play();
            }
            yield return WaitForWebCamTexture();
        }

        public override void Pause()
        {
            //if (isPlaying)
            //{
            //    webCamTexture.Pause();
            //}
        }

        public override void Stop()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }
            webCamTexture = null;
        }

        private RenderTexture resizedTexture;
        private Texture2D convertedTexture;
        //public override Texture GetCurrentTexture() => webCamTexture;
        public override Texture GetCurrentTexture()
        {
            if (!arCameraBackground) return null;
            int textureWidth = this.textureWidth;
            int textureHeight = this.textureHeight;
            //Debug.Log($"{textureWidth}X{textureHeight}");
            // Reuse existing textures if they are not null
            if (resizedTexture == null || resizedTexture.width != textureWidth || resizedTexture.height != textureHeight)
            {
                resizedTexture = new RenderTexture(textureWidth, textureHeight, 1, RenderTextureFormat.ARGB32);
            }

            if (convertedTexture == null || convertedTexture.width != textureWidth || convertedTexture.height != textureHeight)
            {
                convertedTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            }

            // Clear the RenderTexture and draw the background again
            Graphics.Blit(null, resizedTexture, arCameraBackground.material);


            // Read the pixels into the reused Texture2D
            //RenderTexture.active = resizedTexture;
            //convertedTexture.ReadPixels(new UnityEngine.Rect(0, 0, textureWidth, textureHeight), 0, 0);
            //convertedTexture.Apply();
            //RenderTexture.active = null;

            return resizedTexture;
        }

        private ResolutionStruct GetDefaultResolution()
        {
            var resolutions = availableResolutions;
            return resolutions == null || resolutions.Length == 0 ? new ResolutionStruct() : resolutions.OrderBy(resolution => resolution, new ResolutionStructComparer(_preferableDefaultWidth)).First();
        }

        private void InitializeWebCamTexture()
        {
            return;
            Stop();
            if (webCamDevice is WebCamDevice valueOfWebCamDevice)
            {
                webCamTexture = new WebCamTexture(valueOfWebCamDevice.name, resolution.width, resolution.height, (int)resolution.frameRate);
                return;
            }
            throw new InvalidOperationException("Cannot initialize WebCamTexture because WebCamDevice is not selected");
        }

        private IEnumerator WaitForWebCamTexture()
        {
            const int timeoutFrame = 2000;
            var count = 0;
            Debug.Log("Waiting for WebCamTexture to start");
            yield return new WaitUntil(() => count++ > timeoutFrame || webCamTexture.width > 16);

            if (webCamTexture.width <= 16)
            {
                throw new TimeoutException("Failed to start WebCam");
            }
        }

        private class ResolutionStructComparer : IComparer<ResolutionStruct>
        {
            private readonly int _preferableDefaultWidth;

            public ResolutionStructComparer(int preferableDefaultWidth)
            {
                _preferableDefaultWidth = preferableDefaultWidth;
            }

            public int Compare(ResolutionStruct a, ResolutionStruct b)
            {
                var aDiff = Mathf.Abs(a.width - _preferableDefaultWidth);
                var bDiff = Mathf.Abs(b.width - _preferableDefaultWidth);
                if (aDiff != bDiff)
                {
                    return aDiff - bDiff;
                }
                if (a.height != b.height)
                {
                    // prefer smaller height
                    return a.height - b.height;
                }
                // prefer smaller frame rate
                return (int)(a.frameRate - b.frameRate);
            }
        }
    }
}
