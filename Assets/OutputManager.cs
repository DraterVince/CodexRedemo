using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputManager : MonoBehaviour
{
    public List<GameObject> codes = new List<GameObject>();
    public List<GameObject> outputs = new List<GameObject>();
    public List<GameObject> answerList = new List<GameObject>();
    public List<AnswerList> answerListContainer = new List<AnswerList>();

    //[System.Serializable]
    //public class CodeList
    //{
    //    public List<GameObject> codes = new List<GameObject>();
    //}

    [System.Serializable]
    public class AnswerList
    {
        public List<GameObject> answers = new List<GameObject>();
    }
    public int counter;
}
