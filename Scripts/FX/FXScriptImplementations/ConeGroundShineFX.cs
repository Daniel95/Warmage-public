using System.Collections;
using UnityEngine;

public class ConeGroundShineFX : FXScriptBase
{
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private float speed = 25f;
    [SerializeField] private float drug = 1f;
    [SerializeField] private GameObject craterPrefab = null;
    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private float spawnDuration = 0.6f;
    [SerializeField] private float positionOffset = 2f;
    [SerializeField] private bool changeScale = false;

    private Coroutine coroutine;

    public override void Play()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = StartCoroutine(StartMove());

        if(audioSource != null)
        {
            audioSource.Play();
        }
    }

    public IEnumerator StartMove()
    {
        float randomTimer = 0;
        float startSpeed = speed;
        Vector3 startPosition = transform.position;

        while (true)
        {
            randomTimer += Time.deltaTime;
            startSpeed *= drug;
            transform.position += transform.forward * (startSpeed * Time.deltaTime);

            var heading = transform.position - startPosition;
            var distance = heading.magnitude;

            if (distance > spawnRate)
            {
                if (craterPrefab != null)
                {
                    Vector3 randomPosition = new Vector3(Random.Range(-positionOffset, positionOffset), 0, Random.Range(-positionOffset, positionOffset));

                    Vector3 pos = transform.position + (randomPosition * randomTimer * 2);

                    var craterInstance = Instantiate(craterPrefab, pos, Quaternion.identity);
                    if (changeScale == true) { craterInstance.transform.localScale += new Vector3(randomTimer * 2, randomTimer * 2, randomTimer * 2); }
                    var craterPs = craterInstance.GetComponent<ParticleSystem>();
                    if (craterPs != null)
                    {
                        Destroy(craterInstance, craterPs.main.duration);
                    }
                    else
                    {
                        var flashPsParts = craterInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                        Destroy(craterInstance, flashPsParts.main.duration);
                    }
                }
                startPosition = transform.position;
            }
            if (randomTimer > spawnDuration)
            {
                transform.position = transform.parent.position;
                coroutine = null;
                yield break;
            }
            yield return null;
        }
    }

    public override void Stop()
    {
        if(coroutine != null)
        {
            transform.position = transform.parent.position;

            StopCoroutine(coroutine);
            coroutine = null;
        } 
    }
}
