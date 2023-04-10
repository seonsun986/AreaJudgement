using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VertextLoader : MonoBehaviour
{
    public static VertextLoader Instance;
    public GameObject vertexFactory;
    // string : 땅번호, List Vector3 : 버텍스 위치
    public Dictionary<string, List<Vector3>> obPositions = new Dictionary<string, List<Vector3>>();
    public Dictionary<string, List<Vector3>> hazardPositions = new Dictionary<string, List<Vector3>>();

    private Dictionary<GameObject, List<Vector3>> planeVertices = new Dictionary<GameObject, List<Vector3>>();
    private GameObject plane;
    private TextAsset textFile; // 텍스트 파일

    public LineLoader lineLoader;
    public TextureLoader textureLoader;
    public List<GameObject> buttons;
    public Text outOrInText;

    private void Awake()
    {
        Instance = this;
    }

    public void ChooseTextFile(TextAsset textAsset)
    {
        textFile = textAsset;
        for(int i = 0; i<buttons.Count; i++)
        {
            buttons[i].SetActive(false);
        }
        outOrInText.gameObject.SetActive(true);
        StartCoroutine(LoadVertices());
    }

    #region 파일 파싱 및 vertex 생성
    private void ParseTextFile(string text)
    {
        string[] lines = text.Split('\n');

        string currentZone = "";
        List<Vector3> hazardZonePositions = null;
        List<Vector3> obZonePositions = null;
        List<Vector3> vertexPositions = null;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (line.StartsWith("Plane Number"))
            {
                currentZone = line;
                if (!hazardPositions.ContainsKey(currentZone))
                {
                    if (hazardPositions.Count > 0)
                    {
                        lineLoader.Connection(hazardZonePositions, obZonePositions, plane);
                        vertexPositions.AddRange(hazardZonePositions);
                        vertexPositions.AddRange(obZonePositions);
                        planeVertices[plane] = vertexPositions;
                    }

                    plane = new GameObject("Plane" + hazardPositions.Count);
                    vertexPositions = new List<Vector3>();
                    hazardZonePositions = new List<Vector3>();
                    obZonePositions = new List<Vector3>();
                    hazardPositions[currentZone] = hazardZonePositions;
                    obPositions[currentZone] = obZonePositions;
                }
                else
                {
                    hazardZonePositions = hazardPositions[currentZone];
                    obZonePositions = obPositions[currentZone];
                }
            }
            else if (line.StartsWith("Hazard"))
            {
                hazardZonePositions.Add(ParsePosition(line));
                CreateVertex(ParsePosition(line), Color.red, "Hazardvertex" + i, plane.transform);

            }
            else if (line.StartsWith("OB"))
            {
                obZonePositions.Add(ParsePosition(line));
                CreateVertex(ParsePosition(line), Color.white, "OBvertex" + i, plane.transform);
            }
        }

        // 마지막으로 연결
        if (hazardZonePositions != null && obZonePositions != null)
        {
            lineLoader.Connection(hazardZonePositions, obZonePositions, plane);
            vertexPositions.AddRange(hazardZonePositions);
            vertexPositions.AddRange(obZonePositions);
            planeVertices[plane] = vertexPositions;
            lineLoader.OverlapVertex(planeVertices);
        }
    }
    #endregion

    private Vector3 ParsePosition(string line)
    {
        string[] parts = line.Split(new char[] { ',', '\t' });
        float x, y, z;
        float.TryParse(parts[1].Replace("(", "").Replace(",", ""), out x);
        float.TryParse(parts[2].Replace(",", ""), out y);
        float.TryParse(parts[3].Replace(")", "").Replace(",", ""), out z);
        return new Vector3(x, y, z);
    }

    private void CreateVertex(Vector3 position, Color color, string name, Transform parent)
    {
        GameObject vertex = Instantiate(vertexFactory);
        vertex.name = name;
        vertex.transform.position = position;
        vertex.transform.SetParent(parent);
        MeshRenderer mr = vertex.GetComponent<MeshRenderer>();
        mr.material.color = color;
    }

    private IEnumerator LoadVertices()
    {
        if (textFile != null)
        {
            ParseTextFile(textFile.text);
            textureLoader.SetCameraPosition(planeVertices);
            yield return new WaitForSeconds(0.1f);
            textureLoader.CreateTexture(planeVertices, 512, 512);
        }
    }
}