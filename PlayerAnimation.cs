using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;

    private bool is_AnimationControler ;

    public void Awake()
    {
        is_AnimationControler = false;
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Trigger型でほしいパラメータのSet
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignore"></param>
    public void SetTrigger(string name, bool ignore = false)
    {
        if (!ignore)
        {
            if (is_AnimationControler) return;
            is_AnimationControler = true;
            anim.SetTrigger(name);
        }
        else
        {
            anim.SetTrigger(name);
        }
    }

    /// <summary>
    /// Int型でほしいパラメータのSet
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="ignore"></param>
    public void SetInt(string name,int value, bool ignore = false)
    {
        if (!ignore)
        {
            if (is_AnimationControler) return;
            is_AnimationControler = true;
            anim.SetInteger(name, value);
        }
        else
        {
            anim.SetInteger(name, value);
        }
    }

    /// <summary>
    /// Float型でほしいパラメータのSet
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="ignore"></param>
    public void SetFloat(string name, float value, bool ignore = false)
    {
        if (!ignore)
        {
            if (is_AnimationControler) return;
            is_AnimationControler = true;
            anim.SetFloat(name, value);
        }
        else
        {
            anim.SetFloat(name, value);
        }
    }

    /// <summary>
    /// Bool型でほしいパラメータのSet
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="ignore"></param>
    public void Setbool(string name, bool value, bool ignore = false)
    {
        if (!ignore)
        {
            if (is_AnimationControler) return;
            is_AnimationControler = true;
            anim.SetBool(name, value);
        }
        else
        {
            anim.SetBool(name, value);
        }
    }

    public bool AnimExit(bool animexit = false) => is_AnimationControler = animexit;
}
