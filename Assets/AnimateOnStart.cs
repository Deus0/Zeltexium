using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
    public class AnimateOnStart : MonoBehaviour
    {
        [SerializeField]
        private float AnimateDelay = 1f;
        Animator MyAnimator;

        // Use this for initialization
        void Start()
        {
            MyAnimator = GetComponent<Animator>();
            GameManager.Get().OnBeginGame.AddEvent(OnBeginGame);
            GameManager.Get().OnEndGame.AddEvent(OnEndGame);
        }

        private void OnBeginGame()
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(AnimateAtStart());
        }
        private void OnEndGame()
        {
            MyAnimator.SetBool("Visible", false);
        }

        private IEnumerator AnimateAtStart()
        {
            yield return new WaitForSeconds(AnimateDelay);
            if (MyAnimator)
            {
                MyAnimator.SetBool("Visible", true);
            }
        }
    }

}