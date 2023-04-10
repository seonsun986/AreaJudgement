using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TextureLoader : MonoBehaviour
{
    public GameObject rawImage;

    public void SetCameraPosition(Dictionary<GameObject, List<Vector3>> planeVertices)
    {
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        // planeVertices 딕셔너리에 포함된 모든 plane의 좌표를 모두 포함하도록 bounding box 계산
        foreach (List<Vector3> vertexList in planeVertices.Values)
        {
            foreach (Vector3 vertex in vertexList)
            {
                minBounds = Vector3.Min(minBounds, vertex);
                maxBounds = Vector3.Max(maxBounds, vertex);
            }
        }

        Vector3 boundsCenter = (maxBounds + minBounds) / 2f;
        float boundsSize = (maxBounds - minBounds).magnitude;     // bounding box 대각선 길이
        float cameraDistance = (boundsSize / 2f) / Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2f);

        // 카메라 위치 조정
        Camera.main.transform.position = boundsCenter - cameraDistance * Camera.main.transform.forward;
    }

    public void CreateTexture(Dictionary<GameObject, List<Vector3>> planeVertices, int width, int height)
    {        
        Bounds bounds = new Bounds();
        foreach (List<Vector3> vertexList in planeVertices.Values)
        {
            foreach (Vector3 vertex in vertexList)
            {
                bounds.Encapsulate(vertex);
            }
        }

        // RenderTexture 생성
        RenderTexture renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        Camera.main.targetTexture = renderTexture;
        Camera.main.Render();
        Camera.main.targetTexture = null;

        // Texture2D 생성
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        // Texture2D의 픽셀값을 변경하여 alpha값 조절
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // 픽셀 좌표를 계산
            int x = i % width;
            int y = i / width;
            Vector3 screenPos = new Vector3(x, y, Camera.main.transform.position.y);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos = new Vector3(worldPos.x, 0, worldPos.z);
            
            // vertexBounds에 포함되면 alpha값을 1, 아니면 0으로 설정
            bool isInside = false;
            foreach (List<Vector3> vertexList in planeVertices.Values)
            {
                if (isInsidePolygon(vertexList, worldPos))
                {
                    isInside = true;
                    break;
                }
            }

            pixels[i].a = isInside ? 1f : 0f;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // 추출된 알파값 텍스쳐를 저장할 경로
        string fileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png";
#if UNITY_EDITOR
        string filePath = Application.dataPath + "/Textures/" + fileName;


#else
        string filePath = Application.streamingAssetsPath + "/Textures/" + fileName;
#endif

        // 추출된 알파값 텍스쳐를 압축하여 PNG 파일로 저장
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);

        // 프로세스 생성
        Process process = new Process();
        process.StartInfo.FileName = filePath;

        // 파일 열기
        process.Start();

        rawImage.GetComponent<RawImage>().texture = texture;
        rawImage.SetActive(true);
        VertextLoader.Instance.outOrInText.text = "텍스쳐를 추출하였습니다.\n원하는 곳을 클릭하여\n영역판정이 가능합니다.";
    }


    bool CrossCheck(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        return CrossCheckClass.CrossCheck(a, b, c, d);
    }

    bool isInsidePolygon(List<Vector3> polygonPos, Vector3 dotPos)
    {
        Vector3 outsidePos = dotPos;
        outsidePos.x += 1024;

        int count = polygonPos.Count;
        int crossCount = 0;

        for (int i = 0; i < count - 1; i++)
        {
            if (CrossCheck(polygonPos[i], polygonPos[i + 1], dotPos, outsidePos))
            {
                crossCount++;
            }
        }

        if (CrossCheck(polygonPos[0], polygonPos[count - 1], dotPos, outsidePos))
            crossCount++;

        return (crossCount % 2) == 1;
    }

}



