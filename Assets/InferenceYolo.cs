using System;
using System.Collections;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

public class InferenceYolo : MonoBehaviour
{
    public RawImage rawImage;

    private WebCamTexture _webCamTexture;
    private const int SelectCamera = 1; /* 追加 ２*/
    private Texture2D _tex;
    [SerializeField] private NNModel modelSource;
    private IWorker _worker;
    private Boolean _detected;
    private IEnumerator _executor;

    void Start()
    {
        WebCamDevice[] webCamDevice = WebCamTexture.devices;
        _webCamTexture = new WebCamTexture(webCamDevice[SelectCamera].name, 640, 640); //カメラを変更
        _webCamTexture.Play();

        var model = ModelLoader.Load(modelSource);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, model);
    }

    void Update()
    {
        // Webカメラ準備前は無処理
        if (_webCamTexture == null ||
            _webCamTexture.width <= 16 || _webCamTexture.height <= 16) return;

        if (_detected)
        {
            Tensor input = GetInputTensor();
            _executor = _worker.StartManualSchedule(input);
            _detected = false;
            input.Dispose();
        }
        else
        {
            int i = 0;
            bool hasMoreWork = true;
            do
            {
                hasMoreWork = _executor.MoveNext();

            } while (++i < 30 && hasMoreWork);

            if (!hasMoreWork)
            {
                Tensor output = _worker.CopyOutput();
                Debug.Log(output.shape);
                output.Dispose();
            }
        }
    }

    Tensor GetInputTensor()
    {
        if (_tex == null)
        {
            _tex = new Texture2D(_webCamTexture.width, _webCamTexture.height);
        }
    
        _tex.SetPixels(_webCamTexture.GetPixels());
        _tex.Apply();
        Texture2D resizedTex = Resize(_tex, 416, 416);
        rawImage.texture = resizedTex;
        Debug.Log(resizedTex);
        return new Tensor(resizedTex,3);
    }

    private Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }
}