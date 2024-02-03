using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndIntegerParamBehaviour : StateMachineBehaviour
{
    public string parameter = "";
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger(parameter, 0);
    }
}
