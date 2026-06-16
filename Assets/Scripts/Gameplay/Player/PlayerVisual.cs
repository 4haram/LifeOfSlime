using UnityEngine;
namespace Gameplay.Player
{
    public class PlayerVisual : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        public Animator Animator { get; private set; }
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Animator = GetComponentInChildren<Animator>();
        }
        public void SetFacing(float x)
        {
            if (x > 0.01f)
                SetFlipX(false);
            else if (x < -0.01f)
                SetFlipX(true);
        }
        public void SetFlipX(bool b)
        {
            if (spriteRenderer)
                spriteRenderer.flipX = b;
        }
    }
}