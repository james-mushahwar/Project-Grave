using UnityEngine;
using TMPro;
using _Scripts.Gameplay.Architecture.Managers;

[RequireComponent(typeof(ParticleSystem))]
public class TextParticleEmitter : MonoBehaviour
{
    [SerializeField] private string[] textOptions = { "Hello", "Wow", "Cool", "Nice" };
    [SerializeField] private float textSize = 0.5f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private GameObject _floatingTextGOPrefab;
    //[SerializeField] private TMP_FontAsset font;
    //[SerializeField] private Material textMaterial; // Assign a TMP material with outline/shadow

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private GameObject[] textObjects;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 1f;
        main.startLifetime = lifetime;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Ensure world space


        // Initialize arrays
        particles = new ParticleSystem.Particle[main.maxParticles];
        textObjects = new GameObject[main.maxParticles];

        // Ensure emission is enabled
        var emission = ps.emission;
        emission.enabled = true;
    }

    void Update()
    {
        int numParticles = ps.GetParticles(particles);

        for (int i = 0; i < numParticles; i++)
        {
            // Create text object for new particles
            if (textObjects[i] == null || !textObjects[i].activeInHierarchy)
            {
                textObjects[i] = CreateTextObject(particles[i].position);
                // Apply initial random direction within spread angle
                float angle = Random.Range(-spreadAngle, spreadAngle);
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.up;
                particles[i].velocity = direction * floatSpeed;
            }

            // Update text position to follow particle
            textObjects[i].transform.position = particles[i].position;

            // Fade out effect
            float lifeRatio = particles[i].remainingLifetime / particles[i].startLifetime;
            TextMeshPro textMesh = textObjects[i].GetComponent<TextMeshPro>();
            Color color = textColor;
            color.a = lifeRatio;
            textMesh.color = color;
        }

        ps.SetParticles(particles, numParticles);
    }

    GameObject CreateTextObject(Vector3 position)
    {
        //GameObject textObj = new GameObject("TextParticle");
        GameObject textObj = GameObject.Instantiate(_floatingTextGOPrefab, transform, true);
        TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

        textMesh.text = textOptions[Random.Range(0, textOptions.Length)];
        //textMesh.fontSize = 10 * textSize; // TMP uses larger font sizes
        //textMesh.color = textColor;
        //textMesh.alignment = TextAlignmentOptions.Center;

        //if (font != null)
        //    textMesh.font = font;
        //if (textMaterial != null)
        //    textMesh.fontMaterial = textMaterial;

        //textObj.transform.position = position;
        //textObj.transform.rotation = Quaternion.LookRotation(CameraManager.Instance.MainCamera.transform.forward);
        //textObj.transform.parent = transform;

        return textObj;
    }

    void OnParticleSystemStopped()
    {
        // Clean up text objects when particle system stops
        foreach (GameObject textObj in textObjects)
        {
            if (textObj != null)
                Destroy(textObj);
        }
    }
}