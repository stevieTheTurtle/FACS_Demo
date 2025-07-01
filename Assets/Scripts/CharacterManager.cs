using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Camera Parameters")] 
    [SerializeField] private float heightMale = 1.68f;
    [SerializeField] private float heightFemale = 1.6f;
    
    private List<RuntimeAnimator> m_CharAnimators;

    private RuntimeAnimator m_ActiveAnim;
    private int m_AnimIndex;
    
    private List<ActionUnit> actionUnits = new List<ActionUnit>();
    private List<Emotion> emotions = new List<Emotion>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_CharAnimators = new List<RuntimeAnimator>(this.GetComponentsInChildren<RuntimeAnimator>());

        foreach (RuntimeAnimator anim in m_CharAnimators)
        {
            anim.gameObject.SetActive(false);
        }

        m_AnimIndex = 0;
        ChangeCharacter(m_AnimIndex);
    }

    void UpdateCameraHeight()
    {
        float x = Camera.main.transform.position.x;
        float z = Camera.main.transform.position.z;
        
        if (m_ActiveAnim.gameObject.name.Contains("Female"))
            Camera.main.transform.position = new Vector3(x, heightFemale, z);
        else if (m_ActiveAnim.gameObject.name.Contains("Male"))
            Camera.main.transform.position = new Vector3(x, heightMale, z);
    }
    
    void ChangeCharacter(int index)
    {
        if(m_ActiveAnim != null)
            m_ActiveAnim.gameObject.SetActive(false);

        m_ActiveAnim = m_CharAnimators[index];
        m_ActiveAnim.gameObject.SetActive(true);

        UpdateCameraHeight();
    }

    void ChangeToNextCharacter()
    {
        m_AnimIndex++;
        if (m_AnimIndex >= m_CharAnimators.Count)
            m_AnimIndex = 0;
        ChangeCharacter(m_AnimIndex);
    }

    void ChangeToPreviousCharacter()
    {
        m_AnimIndex--;
        if (m_AnimIndex < 0)
            m_AnimIndex = m_CharAnimators.Count - 1;
        ChangeCharacter(m_AnimIndex);
    }

    private void SaveSliderWeights()
    {
        actionUnits = m_ActiveAnim.ActionUnits;
        emotions = m_ActiveAnim.Emotions;
    }
    
    private void LoadSliderWeights()
    {
        m_ActiveAnim.SetActionUnits(actionUnits);
        m_ActiveAnim.SetEmotions(emotions);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SaveSliderWeights();
            ChangeToNextCharacter();
            LoadSliderWeights();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SaveSliderWeights();
            ChangeToPreviousCharacter();
            LoadSliderWeights();
        }
    }
}
