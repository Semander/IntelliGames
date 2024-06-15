using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    [HideInInspector]
    public bool hasGameFinished;

    [SerializeField]
    private TMP_Text titleText;
    [SerializeField]
    private GameObject winText;
    [SerializeField]
    private SpriteRenderer clickHighlight;

    [SerializeField]
    private SpriteRenderer boardPrefab, bgCellPrefab;

    [SerializeField]
    private Node nodePrefab;
    private List<Node> nodes;
    public Dictionary<Vector2Int, Node> nodeGrid;

    public List<Color> nodeColors;
    private Node startNode;
    private LevelData currentLevelData;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        winText.SetActive(false);
        titleText.gameObject.SetActive(true);
        titleText.text = GameManager.Instance.StageName + " - " + GameManager.Instance.CurrentLevel.ToString();
        currentLevelData = GameManager.Instance.GetLevel();
        SpawnBoard();
        SpawnNodes();
    }

    private void SpawnBoard()
    {
        int currentLevelSize = GameManager.Instance.CurrentStage + 4;

        var board = Instantiate(boardPrefab, new Vector3(currentLevelSize / 2f, currentLevelSize / 2f, 0f), Quaternion.identity);
        board.size = new Vector2(currentLevelSize + 0.08f, currentLevelSize + 0.08f);

        for (int i = 0; i < currentLevelSize; i++)
        {
            for (int j = 0; j < currentLevelSize; j++)
            {
                Instantiate(bgCellPrefab, new Vector3(i + 0.5f, j + 0.5f, 0f), Quaternion.identity);
            }
        }

        Camera.main.orthographicSize = currentLevelSize + 2f;
        Camera.main.transform.position = new Vector3(currentLevelSize / 2f, currentLevelSize / 2f, -10f);

        clickHighlight.size = new Vector2(currentLevelSize / 4f, currentLevelSize / 4f);
        clickHighlight.transform.position = Vector3.zero;
        clickHighlight.gameObject.SetActive(false);
    }

    private void SpawnNodes()
    {
        nodes = new List<Node>();
        nodeGrid = new Dictionary<Vector2Int, Node>();

        int currentLevelSize = GameManager.Instance.CurrentStage + 4;
        for (int i = 0; i < currentLevelSize; i++)
        {
            for (int j = 0; j < currentLevelSize; j++)
            {
                Vector3 spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f);
                Node spawnedNode = Instantiate(nodePrefab, spawnPos, Quaternion.identity);
                spawnedNode.Init();

                int colorIdForSpawnedNode = GetColorId(i, j);
                if (colorIdForSpawnedNode != -1)
                {
                    spawnedNode.SetColorForPoint(colorIdForSpawnedNode);
                }

                nodes.Add(spawnedNode);
                nodeGrid.Add(new Vector2Int(i, j), spawnedNode);
                spawnedNode.gameObject.name = i.ToString() + j.ToString();
                spawnedNode.Pos2D = new Vector2Int(i, j);
            }
        }

        List<Vector2Int> offsetPos = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var item in nodeGrid)
        {
            foreach (var offset in offsetPos)
            {
                Vector2Int checkPos = item.Key + offset;
                if (nodeGrid.ContainsKey(checkPos))
                {
                    item.Value.SetEdge(offset, nodeGrid[checkPos]);
                }
            }
        }
    }

    private int GetColorId(int i, int j)
    {
        List<Edge> edges = currentLevelData.Edges;
        Vector2Int point = new Vector2Int(i, j);

        for (int colorId = 0; colorId < edges.Count; colorId++)
        {
            if (edges[colorId].StartPoint == point || edges[colorId].EndPoint == point)
            {
                return colorId;
            }
        }
        return -1;
    }

    public Color GetHighLightColor(int colorID)
    {
        Color result = nodeColors[colorID % nodeColors.Count];
        result.a = 0.4f;
        return result;
    }

    private void Update()
    {
        if (hasGameFinished) return;

        if (Input.GetMouseButtonDown(0))
        {
            startNode = null;
            return;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (startNode == null)
            {
                if (hit && hit.collider.gameObject.TryGetComponent(out Node tNode) && tNode.IsClickable)
                {
                    startNode = tNode;
                    clickHighlight.gameObject.SetActive(true);
                    clickHighlight.gameObject.transform.position = (Vector3)mousePos2D;
                    clickHighlight.color = GetHighLightColor(tNode.colorId);
                }
                return;
            }

            clickHighlight.gameObject.transform.position = (Vector3)mousePos2D;

            if (hit && hit.collider.gameObject.TryGetComponent(out Node tempNode) && startNode != tempNode)
            {
                if (startNode.colorId != tempNode.colorId && tempNode.IsEndNode)
                {
                    return;
                }

                startNode.UpdateInput(tempNode);
                CheckWin();
                startNode = null;
            }
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            startNode = null;
            clickHighlight.gameObject.SetActive(false);
        }
    }

    private void CheckWin()
    {
        bool isWinning = true;

        foreach (var item in nodes)
        {
            item.SolveHighlight();
        }

        foreach (var item in nodes)
        {
            isWinning &= item.IsWin;
            if (!isWinning)
            {
                return;
            }
        }

        GameManager.Instance.UnlockLevel();

        winText.gameObject.SetActive(true);
        clickHighlight.gameObject.SetActive(false);

        hasGameFinished = true;
    }

    public void ClickedBack()
    {
        GameManager.Instance.GoToMainMenu();
    }

    public void ClickedRestart()
    {
        GameManager.Instance.GoToGameplay();
    }

    public void ClickedNextLevel()
    {
        if (!hasGameFinished) return;

        GameManager.Instance.GoToGameplay();
    }
}
