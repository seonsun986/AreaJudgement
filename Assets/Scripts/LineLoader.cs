using System.Collections.Generic;
using UnityEngine;

public class LineLoader : MonoBehaviour
{
    public Transform hazardLines;
    public Transform obLines;
    private Dictionary<GameObject, List<GameObject>> planeLines = new Dictionary<GameObject, List<GameObject>>();
    private List<KeyValuePair<GameObject, GameObject>> checkedPairs = new List<KeyValuePair<GameObject, GameObject>>();
    private GameObject currentPlane;

    public void Connection(List<Vector3> hazardZonePositions, List<Vector3> obZonePositions, GameObject plane)
    {
        currentPlane = plane;
        ConnectVertices(hazardZonePositions, Color.red, "HazardLine", hazardLines);
        ConnectVertices(obZonePositions, Color.white, "OBLine", obLines);
        ConnectLastVertex(hazardZonePositions, obZonePositions);
    }


    #region 마지막 Vertex 제외 연결
    private void ConnectVertices(List<Vector3> positions, Color color, string name, Transform parent)
    {
        for (int i = 0; i < positions.Count - 1; i++)
        {
            CreateLine(positions[i], positions[i + 1], color, name, parent, planeLines);
        }
    }
    #endregion

    #region 마지막 Vertex 연결
    private void ConnectLastVertex(List<Vector3> hazardZonePositions, List<Vector3> obZonePositions)
    {
        int hazard = hazardZonePositions.Count;
        int ob = obZonePositions.Count;
        // 마지막 vertex 처리 
        if (hazard > 0 && ob > 0)
        {
            CreateLine(hazardZonePositions[0], obZonePositions[ob - 1], Color.red, "HazardLastLine", hazardLines, planeLines);
            CreateLine(obZonePositions[0], hazardZonePositions[hazard - 1], Color.white, "OBLastLine", obLines, planeLines);
        }
        // Hazard으로만 이뤄짐
        else if (hazard > 0 && ob == 0)
        {
            CreateLine(hazardZonePositions[hazard - 1], hazardZonePositions[0], Color.red, "HazardLastLine", hazardLines, planeLines);
        }
        // OB로만 이뤄짐
        else if (hazard == 0 && ob > 0)
        {
            CreateLine(obZonePositions[0], obZonePositions[ob - 1], Color.white, "OBLastLine", obLines, planeLines);
        }
    }
    #endregion

    #region 라인 생성
    private void CreateLine(Vector3 start, Vector3 end, Color color, string name, Transform parent, Dictionary<GameObject, List<GameObject>> planeLines)
    {
        if(!planeLines.ContainsKey(currentPlane))
        {
            planeLines.Add(currentPlane, new List<GameObject>());
        }
        GameObject lineObject = new GameObject(name);
        lineObject.transform.position = start;
        lineObject.transform.SetParent(parent);
        lineObject.AddComponent<LineRenderer>();
        LineRenderer lr = lineObject.GetComponent<LineRenderer>();
        lr.material.color = color;
        lr.startWidth = 2f;
        lr.endWidth = 2f;
        lr.SetPositions(new Vector3[] { start, end });
        planeLines[currentPlane].Add(lineObject);
    }
    #endregion


    #region 중첩 영역 판별 및 정리
    public void OverlapVertex(Dictionary<GameObject, List<Vector3>> planeVertices)
    {

        foreach (var planeVertex1 in planeVertices)
        {
            foreach (var planeVertex2 in planeVertices)
            {               
                // 같은 영역은 검사하지 않음.
                if (planeVertex1.Key == planeVertex2.Key)
                    continue;

                // 이미 검사한 영역끼리는 검사하지 않음.
                KeyValuePair<GameObject, GameObject> pair1 = new KeyValuePair<GameObject, GameObject>(planeVertex1.Key, planeVertex2.Key);
                KeyValuePair<GameObject, GameObject> pair2 = new KeyValuePair<GameObject, GameObject>(planeVertex2.Key, planeVertex1.Key);
                if (checkedPairs.Contains(pair1) || checkedPairs.Contains(pair2))
                    continue;

                List<Vector3> intersections = new List<Vector3>();
                List<Vector3> vertices1 = planeVertex1.Value;
                List<Vector3> vertices2 = planeVertex2.Value;
                Vector3 vertex1 = Vector3.zero;
                Vector3 vertex2 = Vector3.zero;
                int vertex1Index = 0;
                int vertex2Index = 0;

                for (int i = 0; i < vertices1.Count - 1; i++)
                {
                    for (int j = 0; j < vertices2.Count - 1; j++)
                    {
                        Vector3 intersection = Vector3.zero;
                        if(CrossCheckClass.CrossCheck(vertices1[i], vertices1[i + 1], vertices2[j], vertices2[j + 1],out intersection))
                        {
                            if (intersection != Vector3.zero)
                            {
                                intersections.Add(intersection);
                                vertex1 = vertices1[i];
                                vertex1Index = i;
                                vertex2 = vertices2[j];
                                vertex2Index = j;
                            }
                        }
                      
                    }
                }

                // 교차점이 2개 이상일 경우 영역이 겹치는 것으로 판단한다.
                if (intersections.Count > 1)
                {
                    foreach(var hazardPositions in VertextLoader.Instance.hazardPositions.Values)
                    {
                        GameObject hazardVertexKey = null;
                        GameObject obVertexKey = null;
                        if (hazardPositions.Contains(vertex1))
                        {
                            // hazard는 planeVertex1이다.
                            hazardVertexKey = planeVertex1.Key;
                            obVertexKey = planeVertex2.Key;
                            DestoryOverlap(intersections, hazardVertexKey, obVertexKey, vertex2Index);                                                     
                        }

                        else if(hazardPositions.Contains(vertex2))
                        {
                            // hazard는 planeVertex2이다.
                            hazardVertexKey = planeVertex2.Key;
                            obVertexKey = planeVertex1.Key;
                            DestoryOverlap(intersections, hazardVertexKey, obVertexKey, vertex1Index);
                        }
                    }
                }

                // 검사한 쌍을 리스트에 추가한다.
                checkedPairs.Add(pair1);

            }
        }
    }

    #endregion


    #region 중첩 Hazard Line, Vertex 제거
    private void DestoryOverlap(List<Vector3> intersections, GameObject hazardVertexKey, GameObject obVertexKey, int vertexIndex)
    {
        // 중첩된 영역의 line을 없앤다
        for (int j = 0; j < planeLines[hazardVertexKey].Count; j++)
        {
            List<GameObject> lines = planeLines[hazardVertexKey];
            if (IsPointOnRight(intersections[1], intersections[0], lines[j].transform.position))
            {
                Destroy(lines[j]);
                Destroy(planeLines[obVertexKey][vertexIndex]);
            }
        }

        // 중첩된 영역의 Hazard vertex를 없앤다.
        Transform hazardTransform = GameObject.Find(hazardVertexKey.name).transform;
        for (int k = 0; k < hazardTransform.childCount; k++)
        {
            if (IsPointOnRight(intersections[1], intersections[0], hazardTransform.GetChild(k).position))
            {
                Destroy(hazardTransform.GetChild(k).gameObject);
            }
        }
    }
    #endregion


    // 직선을 기준으로 왼쪽 오른쪽 구별을 하는 함수
    private bool IsPointOnRight(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 dir = (b - a).normalized;
        Vector3 normal = Vector3.Cross(dir, Vector3.foward);

        return Vector3.Dot(p - a, normal) > 0f;
    }
}


