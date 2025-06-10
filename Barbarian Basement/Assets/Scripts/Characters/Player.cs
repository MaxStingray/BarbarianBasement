using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Player : CharacterSheet
{
    public bool IsDead { get; private set; }

    [Header("On Hit Effects")]
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private Volume _postProcessingVolume;
    [SerializeField] private float _hitEffectDuration = 0.2f;
    [SerializeField] private float _shakeIntensity = 0.3f;

    //the particle system to play on hit for this character
    [SerializeField] private ParticleSystem _hitParticle;

    private Vignette vignette;
    private Coroutine hitEffectCoroutine;

    public void UpdateName(string name)
    {
        characterName = name;
    }

    protected override void Awake()
    {
        base.Awake();

        if (_postProcessingVolume != null && _postProcessingVolume.profile.TryGet(out Vignette v))
        {
            vignette = v;
            vignette.active = true;
        }
    }


    protected override void Die()
    {
        base.Die();
        IsDead = true;
        // death effects
    }

    public void ResetCharacter()
    {
        //TODO: Clear and reset inventory here
        CurrentBodyPoints = BodyPoints;
        CurrentDefendDice = DefendDice;
        CurrentAttackDice = AttackDice;
    }

    public override void TakeHits(int numHits)
    {
        base.TakeHits(numHits);
        if (numHits > 0)
        {
            if (hitEffectCoroutine != null)
            {
                StopCoroutine(hitEffectCoroutine);
            }
            hitEffectCoroutine = StartCoroutine(OnHitEffects());
        }
    }

    private IEnumerator OnHitEffects()
    {
        Vector3 originalPosition = _cameraHolder.localPosition;
        float elapsed = 0f;

        // Configure Vignette
        if (vignette != null)
        {
            vignette.color.Override(Color.red);
            vignette.intensity.Override(0.4f);
        }

        while (elapsed < _hitEffectDuration)
        {
            // Screen shake
            float offsetX = Random.Range(-_shakeIntensity, _shakeIntensity);
            float offsetY = Random.Range(-_shakeIntensity, _shakeIntensity);
            _cameraHolder.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            // Fade vignette out
            if (vignette != null)
            {
                float t = 1f - (elapsed / _hitEffectDuration);
                vignette.intensity.value = 0.4f * t;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _cameraHolder.localPosition = originalPosition;

        if (vignette != null)
        {
            vignette.intensity.Override(0f);
        }

        hitEffectCoroutine = null;
    }

    public void PlayHitEffect()
    {
        if (_hitParticle)
        {
            _hitParticle.Play();
        }
    }
}
