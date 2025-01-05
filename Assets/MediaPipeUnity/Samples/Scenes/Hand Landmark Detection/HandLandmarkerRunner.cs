// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.VisualScripting;
using Unity.Collections;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
    {
        [SerializeField] private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController;

        private Experimental.TextureFramePool _textureFramePool;

        public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

        public ARCameraBackground arCameraBackground;
        public ARCameraManager arCameraManager;
        public UnityEngine.UI.Image DebugOutput;

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        ImageSource imageSource;
        protected override IEnumerator Run()
        {
            Debug.Log($"Delegate = {config.Delegate}");
            Debug.Log($"Running Mode = {config.RunningMode}");
            Debug.Log($"NumHands = {config.NumHands}");
            Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
            Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetHandLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
            taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
            var imageSource = ImageSourceProvider.ImageSource;
            this.imageSource = imageSource;
            //Debug.Log("ImageSource Set");
            yield return imageSource.Play();

            //if (!imageSource.isPrepared)
            //{
            //    Debug.LogError("Failed to start ImageSource, exiting...");
            //    yield break;
            //}

            // Use RGBA32 as the input format.
            // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
            _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

            // NOTE: The screen will be resized later, keeping the aspect ratio.
            //Debug.Log("Initialising Screen");

            screen.Initialize(imageSource);

            SetupAnnotationController(_handLandmarkerResultAnnotationController, imageSource);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            //var flipHorizontally = false;
            //var flipVertically = false;
            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var result = HandLandmarkerResult.Alloc(options.numHands);

            // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
            var canUseGpuImage = options.baseOptions.delegateCase == Tasks.Core.BaseOptions.Delegate.GPU &&
              SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
              GpuManager.GpuResources != null;
            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                //Debug.Log("Starting");
                //yield return new WaitForSeconds(.03f);
                //if (isPaused)
                //{
                //    Debug.Log("Paused");
                //    yield return new WaitWhile(() => isPaused);
                //}
                //screen.UpdateTexture(imageSource);

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    Debug.Log("_textureFramePool WaitForEndOfFrame");
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                //Debug.Log("<color=blue>checking AR Camera Background</color>");

                //if (arCameraBackground.material == null)
                //{
                //    continue;
                //}

                //Debug.Log("<color=pink>Detected AR Camera Background</color>");
                //Debug.Log("Preparing image");

                // Build the input Image
                Image image;
                if (canUseGpuImage)
                {
                    yield return new WaitForEndOfFrame();
                    textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                    image = textureFrame.BuildGpuImage(glContext);
                }
                else
                {
                    req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);

                    yield return waitUntilReqDone;

                    if (req.hasError)
                    {
                        Debug.LogError($"Failed to read texture from the image source, exiting...");
                        break;
                    }
                    image = textureFrame.BuildCPUImage();
                    textureFrame.Release();
                }
                //Debug.Log("image prepared");

                switch (taskApi.runningMode)
                {
                    case Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(default);
                        }
                        break;
                    case Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(default);
                        }
                        break;
                    case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }
                //Debug.Log("Task done");

            }
        }
        IEnumerator DebugVisual()
        {
            Texture texture = imageSource.GetCurrentTexture();
            if (texture != null)
            {
                DebugOutput.sprite = TextureToSpriteConversion(arCameraBackground.material.mainTexture);
            }
            yield return new WaitForSeconds(1f);
            StartCoroutine(DebugVisual());
        }
        Sprite TextureToSpriteConversion(Texture texture)
        {
            // Convert Texture (e.g., Texture2D or RenderTexture) to Sprite
            if (texture is Texture2D)
            {
                Texture2D texture2D = (Texture2D)texture;
                return Sprite.Create(texture2D, new UnityEngine.Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            }
            else if (texture is RenderTexture)
            {
                RenderTexture renderTexture = (RenderTexture)texture;
                // Read RenderTexture into Texture2D
                RenderTexture.active = renderTexture;
                Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                texture2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture2D.Apply();
                RenderTexture.active = null;

                // Convert to Sprite
                return Sprite.Create(texture2D, new UnityEngine.Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            }

            return null;
        }
        private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
        {
            _handLandmarkerResultAnnotationController.DrawLater(result);
        }
        Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture)
        {
            // Create a new Texture2D with the same dimensions as the RenderTexture
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

            // Set the RenderTexture as the active texture
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // Read the pixels from the RenderTexture into the Texture2D
            Graphics.CopyTexture(renderTexture, texture);
            //texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

            // Apply changes to the Texture2D
            texture.Apply();

            // Reset the active RenderTexture to the original
            RenderTexture.active = currentActiveRT;

            return texture;
        }
    }
}
