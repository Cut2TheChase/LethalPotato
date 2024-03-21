using UnityEngine;

using UnityEngine;
using System.Collections;
using Unity.Netcode;

namespace HotPotato.Behaviours
{
    public class FancyShake : NetworkBehaviour
    {
        [Header("Info")]
        private Vector3 _startPos;
        private float _timer;
        private Vector3 _randomPos;

        [Header("Settings")]
        [Range(0f, 2f)]
        public float _time = 2f;
        [Range(0f, 2f)]
        public float _distance = 0.0175f;
        [Range(0f, 0.1f)]
        public float _delayBetweenShakes = 0.025f;

        public Vector3 shakeval;
        public Vector2 xyshake;


        GrabbableObject objectToShake;
        public bool shouldShake = false;
        private float beginAtCharge = 0.2f; //point where shaking should begin if battery charge is at this number


        public void Begin(GrabbableObject shakeThis, float whenToBegin)
        {
            objectToShake = shakeThis;
            shouldShake = true;
            //_startPos = objectToShake.itemProperties.positionOffset;
            _startPos = objectToShake.transform.position;
            beginAtCharge = whenToBegin;
            StopAllCoroutines();
            StartCoroutine(Shake());
        }

        private IEnumerator Shake()
        {
            _timer = 0f;

            while (_timer < _time)
            {
                _timer += Time.deltaTime;

                xyshake = Random.insideUnitCircle * _distance; 
                _randomPos = _startPos + new Vector3 (0, 0, xyshake.y);

                //objectToShake.itemProperties.positionOffset = _randomPos;
                //objectToShake.transform.position = _randomPos;
                shakeval = _randomPos;


                if (_delayBetweenShakes > 0f)
                {
                    yield return new WaitForSeconds(_delayBetweenShakes);
                }
                else
                {
                    yield return null;
                }
            }

            shakeval = _startPos;
        }

        public void stopShake()
        {
            StopAllCoroutines();
            shouldShake = false;
            objectToShake.transform.position = _startPos;
        }

    }
}
