using System;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[Serializable]
public class Hand
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class Hands
{
    public Hand[] hands;
}

public class DetectHands : MonoBehaviour
{
    public ARCameraManager cameraManager;

    private Texture2D _tex;
    public bool isWave;

    void Start()
    {
        StartCoroutine(nameof(GetHand));
    }
    
    
    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }
    
    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        XRCpuImage image;
        if (!cameraManager.TryAcquireLatestCpuImage(out image))
            return;

        XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams
        (
            image,
            TextureFormat.RGBA32
            );
      
        if (!(image.width <= 16 || image.height <= 16))
        {
            if (_tex == null)
            {
                _tex = new Texture2D(conversionParams.outputDimensions.x,
                    conversionParams.outputDimensions.y,
                    conversionParams.outputFormat, false);
            }
            
            var buffer = _tex.GetRawTextureData<byte>();
            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
            _tex.Apply();

            buffer.Dispose();
        }
        image.Dispose();
    }

    private IEnumerator GetHand()
    {
        while (true)
        {
            if (_tex == null) yield return null;
            
            Texture2D resizedTex = Resize(_tex, 416, 416);
            byte[] bytes = resizedTex.EncodeToJPG(50);

            // Create a Web Form
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes);

            UnityWebRequest www = UnityWebRequest.Post("https://korone-nxvvg3u2jq-an.a.run.app", form);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Hands hands = JsonUtility.FromJson<Hands>(www.downloadHandler.text);
                Debug.Log(hands.hands.Length);
                isWave = hands.hands.Length > 0;
            }
            else
            {
                Debug.LogError(www.error);
            }

            www.Dispose();
            yield return new WaitForSeconds(0f);
        }
    }

    private Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        Destroy(rt);
        return result;
    }
}