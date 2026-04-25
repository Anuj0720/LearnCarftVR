using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Ball Setup")]
    public GameObject ballPrefab;
    public int ballCount = 1;
    public int ballsPerRow = 3;

    [Header("Spacing Controls")]
    public float horizontalSpacing = -0.14f;
    public float verticalSpacing = 0.14f;

    [Header("Height Offset")]
    public float firstRowHeightOffset = 0.18f;

    [Header("Ball Scale")]
    public Vector3 ballScale = Vector3.one;

    [Header("Glow Settings")]
    public bool enableGlowSequence = false;

    [ColorUsage(true, true)]
    public Color glowColor = Color.yellow;

    public float glowIntensity = 3f;
    public float glowDuration = 1.2f;

    private readonly List<GameObject> spawnedBalls = new List<GameObject>();
    private readonly List<Material> mats = new List<Material>();
    private readonly List<Color> originalEmission = new List<Color>();

    void Start()
    {
        SpawnBalls();

        if (enableGlowSequence)
            StartCoroutine(GlowSequence());
    }

    void Update()
    {
        UpdateBallPositions();
    }

    void SpawnBalls()
    {
        if (ballPrefab == null)
        {
            Debug.LogError("Ball prefab is missing!");
            return;
        }

        spawnedBalls.Clear();
        mats.Clear();
        originalEmission.Clear();

        for (int i = 0; i < ballCount; i++)
        {
            GameObject ball = Instantiate(ballPrefab, transform);

            ball.transform.localScale = ballScale;
            ball.name = $"Ball_{i + 1}";

            spawnedBalls.Add(ball);

            Renderer r = ball.GetComponent<Renderer>();
            if (r == null)
                r = ball.GetComponentInChildren<Renderer>();

            if (r != null)
            {
                Material m = r.material;
                m.EnableKeyword("_EMISSION");

                mats.Add(m);

                if (m.HasProperty("_EmissionColor"))
                    originalEmission.Add(m.GetColor("_EmissionColor"));
                else
                    originalEmission.Add(Color.black);
            }
        }

        UpdateBallPositions();
    }

    void UpdateBallPositions()
    {
        for (int i = 0; i < spawnedBalls.Count; i++)
        {
            int row = i / ballsPerRow;
            int col = i % ballsPerRow;

            int ballsInRow = Mathf.Min(ballsPerRow, spawnedBalls.Count - row * ballsPerRow);

            float rowWidth = (ballsInRow - 1) * horizontalSpacing;
            float startX = -rowWidth / 2f;

            Vector3 localPos = new Vector3(
                startX + col * horizontalSpacing,
                firstRowHeightOffset + row * verticalSpacing,
                0
            );

            spawnedBalls[i].transform.localPosition = localPos;
        }
    }

    IEnumerator GlowSequence()
    {
        for (int i = 0; i < mats.Count; i++)
        {
            if (mats[i] == null) continue;

            mats[i].SetColor("_EmissionColor", glowColor * glowIntensity);

            yield return new WaitForSeconds(glowDuration);

            mats[i].SetColor("_EmissionColor", originalEmission[i]);
        }
    }
}