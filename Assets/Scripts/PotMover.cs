using System;
using UnityEngine;

public class PotMover : MonoBehaviour
{
    [SerializeField] private float lifeTime = 15f;         // auto-despawn fallback
    private Vector3 _dir;
    private float _speed;
    private Action<GameObject> _onDespawn;
    private float _t;

    public void Initialize(Vector3 direction, float speed, Action<GameObject> onDespawn)
    {
        _dir = direction.normalized;
        _speed = speed;
        _onDespawn = onDespawn;
    }

    private void Update()
    {
        transform.position += _dir * (_speed * Time.deltaTime);
        _t += Time.deltaTime;

        if (_t >= lifeTime)
        {
            _onDespawn?.Invoke(gameObject);
        }
    }
}
