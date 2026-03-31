using UnityEngine;

public class BoardPlayerAnimationHandler : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    private PlayerMovement pMovemnt;

    private bool oldMoving = false;
    private void Start()
    {
        pMovemnt = GetComponent<PlayerMovement>();

        oldMoving = pMovemnt.isMoving;
    }

    private void Update()
    {
        if (oldMoving == pMovemnt.isMoving)
            return;
        
        animator.SetBool("Walking", pMovemnt.isMoving);
        oldMoving = pMovemnt.isMoving;
    }



}
