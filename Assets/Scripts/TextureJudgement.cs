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
        VertexType vertexType = VertexType.InsideHazard;      // 초기값 설정
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
        // 가장 가까운 Ob 찾기
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

        // 가장 가까운 Hazard 찾기
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

        // 가장 가까운 Ob와 Hazard 중 가까운 쪽의 타입 반환
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
                outOrInText.text = "선택하신 영역은 안쪽입니다.";
            }
            else
            {
                outOrInText.text = "선택하신 영역은 바깥쪽입니다.";

            }
        }

        else
        {
            coordPrefab.GetComponent<Image>().color = Color.red;
            coordPrefab.transform.GetChild(0).GetComponent<Text>().color = Color.red;
            coordPrefab.transform.GetChild(0).GetComponent<Text>().text = "Hazard";
            if(vertexType == VertexType.InsideHazard)
            {
                outOrInText.text = "선택하신 영역은 안쪽입니다.";
            }
            else
            {
                outOrInText.text = "선택하신 영역은 바깥쪽입니다.";
            }

        }

        hasPrefab = true;
    }

}
