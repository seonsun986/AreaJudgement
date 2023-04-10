using UnityEngine;
using UnityEngine.UI;

public class TextureJudgement : MonoBehaviour
{
    public GameObject XPrefab;
    public Text outOrInText;
    private GameObject coordPrefab;
    private bool hasPrefab;
    public enum VertexType
    {
        InsideHazard,
        InsideOB,
        OutsideHazard,
        OutsideOB
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Judge();
        }
    }


    private void Judge()
    {
        VertexType vertexType = VertexType.InsideHazard;      // �ʱⰪ ����
        Texture2D alphaTexture = GetComponent<RawImage>().texture as Texture2D;
        Vector2 mousePos = Input.mousePosition;
        Color pixelColor = alphaTexture.GetPixel((int)mousePos.x, (int)mousePos.y);
        Vector3 mouseWorldPos = GetMouseWorldPos();
        if (pixelColor.a == 1f)
        {
            vertexType = GetNearestVertexType(mouseWorldPos,1);
        }
        else
        {
            vertexType = GetNearestVertexType(mouseWorldPos,0);
        }
        CreateXPrefab(vertexType, mousePos);
        Debug.Log(vertexType.ToString());
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y;

        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private VertexType GetNearestVertexType(Vector3 mousePos, int alpha)
    {
        float nearestObDistance = float.MaxValue;
        // ���� ����� Ob ã��
        foreach (var ob in VertextLoader.Instance.obPositions.Values)
        {
            foreach (Vector3 vertex in ob)
            {
                float distance = Vector3.Distance(mousePos, vertex);
                if (distance < nearestObDistance)
                {
                    nearestObDistance = distance;
                }
            }
        }


        float nearestHazardDistance = float.MaxValue;

        // ���� ����� Hazard ã��
        foreach (var hazard in VertextLoader.Instance.hazardPositions.Values)
        {
            foreach (Vector3 vertex in hazard)
            {
                float distance = Vector3.Distance(mousePos, vertex);
                if (distance < nearestHazardDistance)
                {
                    nearestHazardDistance = distance;
                }
            }
        }

        // ���� ����� Ob�� Hazard �� ����� ���� Ÿ�� ��ȯ
        if (nearestObDistance < nearestHazardDistance)
        {
            if(alpha == 1)
            {
                return VertexType.InsideOB;
            }
            else
            {
                return VertexType.OutsideOB;
            }
        }

        else
        {
            if (alpha == 1)
            {
                return VertexType.InsideHazard;
            }
            else
            {
                return VertexType.OutsideHazard;
            }
        }
    }

    private void CreateXPrefab(VertexType vertexType, Vector3 mousePos)
    {
        if(hasPrefab == true)
        {
            Destroy(coordPrefab);
        }
        coordPrefab = Instantiate(XPrefab);
        coordPrefab.transform.SetParent(GameObject.Find("Canvas").transform);
        coordPrefab.transform.position = mousePos;

        if (vertexType == VertexType.InsideOB || vertexType == VertexType.OutsideOB)
        {
            coordPrefab.GetComponent<Image>().color = Color.white;
            coordPrefab.transform.GetChild(0).GetComponent<Text>().color = Color.white;
            coordPrefab.transform.GetChild(0).GetComponent<Text>().text = "OB";

            if(vertexType == VertexType.InsideOB)
            {
                outOrInText.text = "�����Ͻ� ������ �����Դϴ�.";
            }
            else
            {
                outOrInText.text = "�����Ͻ� ������ �ٱ����Դϴ�.";

            }
        }

        else
        {
            coordPrefab.GetComponent<Image>().color = Color.red;
            coordPrefab.transform.GetChild(0).GetComponent<Text>().color = Color.red;
            coordPrefab.transform.GetChild(0).GetComponent<Text>().text = "Hazard";
            if(vertexType == VertexType.InsideHazard)
            {
                outOrInText.text = "�����Ͻ� ������ �����Դϴ�.";
            }
            else
            {
                outOrInText.text = "�����Ͻ� ������ �ٱ����Դϴ�.";
            }

        }

        hasPrefab = true;
    }

}
