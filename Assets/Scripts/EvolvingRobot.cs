using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EvolvingRobot : MonoBehaviour
{
    [Tooltip("Level 1, 2 and 3 models, in order. Only one is active at a time.")]
    [SerializeField] GameObject[] levels = new GameObject[3];
    [Tooltip("Trash needed to reach level 2 and level 3.")]
    [SerializeField] int[] trashToEvolve = { 5, 10 };
    [SerializeField] float popScale = 1.25f;
    [SerializeField] float popTime = 0.35f;

    public int Level { get; private set; }
    public int Trash { get; private set; }

    void Start() => SetLevel(0, false);

    void Update()
    {
        // Debug key: press E to evolve to the next level.
        var kb = Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame) Evolve();
    }

    public void AddTrash(int amount = 1)
    {
        Trash += amount;
        if (Level < levels.Length - 1 && Trash >= trashToEvolve[Level]) Evolve();
    }

    public void Evolve()
    {
        if (Level < levels.Length - 1) SetLevel(Level + 1, true);
    }

    void SetLevel(int index, bool animate)
    {
        Level = index;
        for (int i = 0; i < levels.Length; i++)
            if (levels[i] != null) levels[i].SetActive(i == index);
        if (animate) StartCoroutine(Pop());
    }

    IEnumerator Pop()
    {
        for (float t = 0f; t < popTime; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / popTime * Mathf.PI);
            transform.localScale = Vector3.one * Mathf.Lerp(1f, popScale, k);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
