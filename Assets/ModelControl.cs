using UnityEngine;

public class ModelControl : MonoBehaviour
{
    private Animator _animator;
    private DetectHands _inferenceYolo;
    public GameObject obj;
    private bool _switched;
    private bool _oldIsFree;
    public AudioClip sound1;
    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _inferenceYolo = obj.GetComponent<DetectHands>();
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        bool nowIsFree = _animator.GetCurrentAnimatorStateInfo(0).IsName("free");
        if (_oldIsFree != nowIsFree && nowIsFree)
        {
            _switched = false;
        }

        _oldIsFree = nowIsFree;
        if (_inferenceYolo.isWave && !_switched)
        {
            Debug.Log("wave!!!");
            _switched = true;
            _animator.CrossFadeInFixedTime("waveHand", 0.2f);
            _audioSource.PlayOneShot(sound1);
        }
    }
}