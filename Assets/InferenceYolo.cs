using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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

public class InferenceYolo : MonoBehaviour
{
    public RawImage rawImage;

    private WebCamTexture _webCamTexture;
    private const int SelectCamera = 1; /* 追加 ２*/
    private Texture2D _tex;
    public bool isWave;

    void Start()
    {
        WebCamDevice[] webCamDevice = WebCamTexture.devices;
        _webCamTexture = new WebCamTexture(webCamDevice[SelectCamera].name, 416, 416); //カメラを変更
        _webCamTexture.Play();

        StartCoroutine(nameof(GetHand));
    }

    private IEnumerator GetHand()
    {
        // Webカメラ準備前は無処理
        while (_webCamTexture == null ||
               _webCamTexture.width <= 16 || _webCamTexture.height <= 16)
        {
            yield return null;
        }

        _tex = new Texture2D(_webCamTexture.width, _webCamTexture.height);

        while (true)
        {
            _tex.SetPixels(_webCamTexture.GetPixels());
            _tex.Apply();
            Texture2D resizedTex = Resize(_tex, 416, 416);
            Destroy(rawImage.texture);
            rawImage.texture = resizedTex;
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
            yield return new WaitForSeconds(1f);
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