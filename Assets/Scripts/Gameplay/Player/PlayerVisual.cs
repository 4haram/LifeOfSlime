using UnityEngine;
namespace Gameplay.Player
{
    public class PlayerVisual : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        public Animator Animator => animator;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
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
