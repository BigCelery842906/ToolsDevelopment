using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    [SerializeField] private float Val1 = 1.0f;
    [SerializeField] private float Val2 = 2.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void IncrementVal1()
    {
        Val1++;
    }
}