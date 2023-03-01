using UnityEngine;

namespace Game.Effects
{
    public class FireMeshAnimation : MonoBehaviour
    {
        public float animationLenght;
        public AnimationCurve sizeCurve;
        public Gradient color;
        public MeshRenderer[] renderers;

        private float currentAnimTime;

        private MaterialPropertyBlock materialProperty;

        void Start()
        {
            materialProperty = new MaterialPropertyBlock();
        }

        void Update()
        {
            if (currentAnimTime < animationLenght)
            {
                currentAnimTime += Time.deltaTime;
                currentAnimTime = Mathf.Min(currentAnimTime, animationLenght);

                UpdateColor();
                UpdateSize();
            }
        }

        public void UpdateColor()
        {
            materialProperty.SetColor("_Multiply_Color", color.Evaluate(currentAnimTime / animationLenght));

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].SetPropertyBlock(materialProperty);
            }
        }

        public void UpdateSize()
        {
            transform.localScale = Vector3.one * sizeCurve.Evaluate(currentAnimTime / animationLenght);
        }
    }
}
