using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBoolParamBehaviour : StateMachineBehaviour
{
    public string parameter = "";
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    /*override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateEnter");
    }*/

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    /*override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateUpdate");
    }*/

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(parameter, false);
        Debug.Log(parameter + " : OnStateExit");
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    /*override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that processes and affects root motion
        Debug.Log("OnStateMove");
    }*/

    // OnStateIK is called right after Animator.OnAnimatorIK()
    /*override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that sets up animation IK (inverse kinematics)
        Debug.Log("OnStateIK");
    }*/
}
