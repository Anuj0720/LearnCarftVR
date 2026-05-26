using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Room4Manager : MonoBehaviour
{
    // --------------------------------------------------------
    // SECTION 1 — SENTENCE DATA
    // --------------------------------------------------------

    [System.Serializable]
    public class SentenceData
    {
        public string    sentenceName;
        public string[]  words;
        public AudioClip sentenceAudio;
        public Sprite    picture;
    }

    [Header("── SENTENCES ──────────────────────────────")]
    public SentenceData[] sentences = new SentenceData[5];


    // --------------------------------------------------------
    // SECTION 2 — SCENE REFERENCES
    // --------------------------------------------------------

    [Header("── SLOTS (4 answer boxes) ──────────────────")]
    public Transform[] slots = new Transform[4];

    [Header("── WORD CUBE PREFAB ──────────────────────────")]
    public GameObject wordCubePrefab;

    [Header("── SPAWN POINTS ────────────────────────────")]
    public Transform[] spawnPoints = new Transform[4];

    [Header("── PICTURE FRAME ────────────────────────────")]
    public SpriteRenderer pictureRenderer;

    [Header("── ROBOT ──────────────────────────────────")]
    public Animator robotAnimator;

    [Header("── AUDIO SOURCES ────────────────────────────")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    [Header("── SOUND EFFECTS ──────────────────────────")]
    public AudioClip introVoiceover;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip completeSound;

    [Header("── SLOT MATERIALS ──────────────────────────")]
    public Material slotDefaultMaterial;
    public Material slotCorrectMaterial;
    public Material slotWrongMaterial;

    [Header("── TIMELINES ────────────────────────────────")]
    public PlayableDirector timeline1_Intro;
    public PlayableDirector timeline2_Round1;
    public PlayableDirector timeline3_Round2;
    public PlayableDirector timeline4_Round3;
    public PlayableDirector timeline5_Rounds45;
    public PlayableDirector timeline6_Complete;


    // --------------------------------------------------------
    // SECTION 3 — PRIVATE RUNTIME VARIABLES
    // --------------------------------------------------------

    private int              currentRound  = 0;
    private List<GameObject> activeCubes   = new List<GameObject>();
    public  GameObject[]     slotContents  = new GameObject[4];
    private bool             roundActive   = false;


    // --------------------------------------------------------
    // SECTION 4 — START
    // --------------------------------------------------------

    void Start()
    {
        currentRound = 0;
        ClearAllSlots();
        // Timeline 1 plays automatically via Play On Awake
        // It will fire a signal to call StartRound1 when done
    }


    // --------------------------------------------------------
    // SECTION 5 — TIMELINE SIGNAL RECEIVERS
    // These are called by Signal Emitters inside each Timeline
    // --------------------------------------------------------

    // Called by Signal at end of Timeline 1
    public void StartRound1()
    {
        timeline2_Round1.Play();
    }

    // Called by Signal inside Timeline 2 at pause point
    public void PauseTimeline2()
    {
        timeline2_Round1.Pause();
        currentRound = 0;
        roundActive  = true;
    }

    // Called by Signal inside Timeline 3 at pause point
    public void PauseTimeline3()
    {
        timeline3_Round2.Pause();
        currentRound = 1;
        roundActive  = true;
    }

    // Called by Signal inside Timeline 4 at pause point
    public void PauseTimeline4()
    {
        timeline4_Round3.Pause();
        currentRound = 2;
        roundActive  = true;
    }

    // Called by Signal inside Timeline 5 at first pause point
    public void PauseTimeline5_Round4()
    {
        timeline5_Rounds45.Pause();
        currentRound = 3;
        roundActive  = true;
    }

    // Called by Signal inside Timeline 5 at second pause point
    public void PauseTimeline5_Round5()
    {
        timeline5_Rounds45.Pause();
        currentRound = 4;
        roundActive  = true;
    }

    // Called by Signal at end of Timeline 6
    public void LoadRoom5()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Room5");
    }


    // --------------------------------------------------------
    // SECTION 6 — SPAWN WORD CUBES
    // Called by Timeline Activation Tracks via Signal
    // or you can call SpawnAllCubes() from a signal
    // --------------------------------------------------------

    public void SpawnAllCubes()
    {
        SentenceData data     = sentences[currentRound];
        string[]     shuffled = ShuffleWords(data.words);

        for (int i = 0; i < shuffled.Length; i++)
        {
            StartCoroutine(SpawnAfterDelay(shuffled[i], spawnPoints[i].position, i * 0.35f));
        }
    }

    IEnumerator SpawnAfterDelay(string word, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3    spawnPos = position + Vector3.up * 1.5f;
        GameObject cube     = Instantiate(wordCubePrefab, spawnPos, Quaternion.identity);

        WordCube wc  = cube.GetComponent<WordCube>();
        wc.wordValue = word;
        wc.manager   = this;

        activeCubes.Add(cube);

        StartCoroutine(DropCube(cube, spawnPos, position));
    }

    IEnumerator DropCube(GameObject cube, Vector3 from, Vector3 to)
    {
        float t        = 0f;
        float duration = 0.5f;

        while (t < duration)
        {
            if (cube == null) yield break;
            t                      += Time.deltaTime;
            cube.transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }

        if (cube != null)
            cube.transform.position = to;
    }


    // --------------------------------------------------------
    // SECTION 7 — SLOT SNAPPING
    // Called by WordCube.cs when student releases a cube
    // --------------------------------------------------------

    public void TrySnapCubeToSlot(GameObject cube, int slotIndex)
    {
        if (!roundActive) return;

        if (slotContents[slotIndex] != null)
        {
            EjectCubeFromSlot(slotIndex);
        }

        slotContents[slotIndex]                    = cube;
        cube.transform.position                    = slots[slotIndex].position;
        cube.GetComponent<WordCube>().isSnapped    = true;
        cube.GetComponent<WordCube>().snappedSlot  = slotIndex;

        if (AllSlotsFilled())
        {
            StartCoroutine(ValidateAnswer());
        }
    }

    void EjectCubeFromSlot(int slotIndex)
    {
        GameObject old = slotContents[slotIndex];
        if (old == null) return;

        WordCube wc    = old.GetComponent<WordCube>();
        wc.isSnapped   = false;
        wc.snappedSlot = -1;

        old.transform.position  += new Vector3(0.3f, 0.3f, 0f);
        slotContents[slotIndex]  = null;
    }

    bool AllSlotsFilled()
    {
        for (int i = 0; i < slotContents.Length; i++)
        {
            if (slotContents[i] == null) return false;
        }
        return true;
    }


    // --------------------------------------------------------
    // SECTION 8 — VALIDATE ANSWER
    // --------------------------------------------------------

    IEnumerator ValidateAnswer()
    {
        roundActive      = false;
        string[] correct = sentences[currentRound].words;
        bool     allOk   = true;

        for (int i = 0; i < 4; i++)
        {
            string placed = slotContents[i].GetComponent<WordCube>().wordValue;

            if (placed == correct[i])
            {
                SetSlotMaterial(i, slotCorrectMaterial);
            }
            else
            {
                SetSlotMaterial(i, slotWrongMaterial);
                StartCoroutine(ShakeSlot(i));
                allOk = false;
            }
        }

        yield return new WaitForSeconds(0.8f);

        if (allOk)
            StartCoroutine(CelebrationSequence());
        else
            StartCoroutine(WrongAnswerSequence());
    }


    // --------------------------------------------------------
    // SECTION 9 — CORRECT ANSWER
    // --------------------------------------------------------

    IEnumerator CelebrationSequence()
    {
        robotAnimator.Play("Dance");
        sfxAudioSource.clip = correctSound;
        sfxAudioSource.Play();

        for (int i = 0; i < 4; i++)
            SetSlotMaterial(i, slotCorrectMaterial);

        yield return new WaitForSeconds(3f);

        StartCoroutine(CleanupAndNextRound());
    }


    // --------------------------------------------------------
    // SECTION 10 — WRONG ANSWER
    // --------------------------------------------------------

    IEnumerator WrongAnswerSequence()
    {
        robotAnimator.Play("GentleHeadShake");
        sfxAudioSource.clip = wrongSound;
        sfxAudioSource.Play();

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < 4; i++)
            SetSlotMaterial(i, slotDefaultMaterial);

        foreach (GameObject cube in activeCubes)
        {
            if (cube == null) continue;
            WordCube wc    = cube.GetComponent<WordCube>();
            wc.isSnapped   = false;
            wc.snappedSlot = -1;
        }

        ClearAllSlots();

        yield return new WaitForSeconds(0.5f);

        // Replay sentence so student remembers the order
        robotAnimator.Play("Talking");
        voiceAudioSource.clip = sentences[currentRound].sentenceAudio;
        voiceAudioSource.Play();

        yield return new WaitForSeconds(voiceAudioSource.clip.length + 0.3f);

        robotAnimator.Play("EncouragingIdle");

        roundActive = true;
    }


    // --------------------------------------------------------
    // SECTION 11 — CLEANUP AND LOAD NEXT ROUND
    // --------------------------------------------------------

    IEnumerator CleanupAndNextRound()
    {
        // Float all cubes up and destroy
        foreach (GameObject cube in activeCubes)
        {
            if (cube != null)
                StartCoroutine(FloatAndDestroy(cube));
        }

        yield return new WaitForSeconds(1f);

        activeCubes.Clear();
        ClearAllSlots();

        for (int i = 0; i < 4; i++)
            SetSlotMaterial(i, slotDefaultMaterial);

        // Resume the correct timeline based on which round just finished
        if      (currentRound == 0) timeline3_Round2.Play();
        else if (currentRound == 1) timeline4_Round3.Play();
        else if (currentRound == 2) timeline5_Rounds45.Play();
        else if (currentRound == 3) timeline5_Rounds45.Resume();
        else if (currentRound == 4) timeline6_Complete.Play();
    }

    IEnumerator FloatAndDestroy(GameObject cube)
    {
        float   t        = 0f;
        float   duration = 0.8f;
        Vector3 start    = cube.transform.position;
        Vector3 end      = start + Vector3.up * 2f;

        while (t < duration)
        {
            if (cube == null) yield break;
            t                      += Time.deltaTime;
            cube.transform.position = Vector3.Lerp(start, end, t / duration);

            Renderer r = cube.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a     = Mathf.Lerp(1f, 0f, t / duration);
                r.material.color = c;
            }

            yield return null;
        }

        Destroy(cube);
    }


    // --------------------------------------------------------
    // SECTION 12 — HELPER FUNCTIONS
    // --------------------------------------------------------

    void ClearAllSlots()
    {
        for (int i = 0; i < slotContents.Length; i++)
            slotContents[i] = null;
    }

    void SetSlotMaterial(int slotIndex, Material mat)
    {
        Renderer r = slots[slotIndex].GetComponent<Renderer>();
        if (r != null) r.material = mat;
    }

    IEnumerator ShakeSlot(int slotIndex)
    {
        Vector3 origin = slots[slotIndex].position;
        float   t      = 0f;

        while (t < 0.4f)
        {
            t                         += Time.deltaTime;
            float offsetX              = Mathf.Sin(t * 60f) * 0.05f;
            slots[slotIndex].position  = origin + new Vector3(offsetX, 0, 0);
            yield return null;
        }

        slots[slotIndex].position = origin;
    }

    IEnumerator FadeSprite(SpriteRenderer sr, float from, float to, float duration)
    {
        float t     = 0f;
        Color color = sr.color;

        while (t < duration)
        {
            t        += Time.deltaTime;
            color.a   = Mathf.Lerp(from, to, t / duration);
            sr.color  = color;
            yield return null;
        }

        color.a  = to;
        sr.color = color;
    }

    string[] ShuffleWords(string[] words)
    {
        string[] copy = (string[])words.Clone();

        for (int i = copy.Length - 1; i > 0; i--)
        {
            int    j   = Random.Range(0, i + 1);
            string tmp = copy[i];
            copy[i]    = copy[j];
            copy[j]    = tmp;
        }

        return copy;
    }
}