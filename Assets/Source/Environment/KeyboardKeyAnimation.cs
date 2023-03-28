using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Environment
{
    public class KeyboardKeyAnimation : MonoBehaviour
    {
        public KeyCode Key;

        [Header("Object Scale")]
        public float PresseScale;

        [Header("Mateiral Emission")]
        public float NormalEmission;
        public float PressedEmission;
        public MeshRenderer Renderer;

        private MaterialPropertyBlock _propertyBlock;

        void Start()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        void Update()
        {
            var _currentScale = transform.localScale;
            var _targetScale = new Vector3(1, Input.GetKey(Key) ? PresseScale : 1, 1);

            if (_currentScale != _targetScale)
            {
                transform.localScale = _targetScale;
            }

            if (Renderer)
            {
                var _targetEmission = Input.GetKey(Key) ? PressedEmission : NormalEmission;
                _propertyBlock.SetFloat("_Interaction", _targetEmission);

                Renderer.SetPropertyBlock(_propertyBlock);
            }


        }
    }
}