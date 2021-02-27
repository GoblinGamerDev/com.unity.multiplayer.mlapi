using UnityEngine;

    [HideInInspector]
    [SerializeField]



        if(m_Rigidbody)
            if(m_DefaultConstraints == RigidbodyConstraints.None)
            {
                m_DefaultConstraints = m_Rigidbody.constraints;
            }
            else
            {
                m_Rigidbody.constraints = m_DefaultConstraints;
            }


        if (!m_MeshRenderer)
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }

        {
            m_MeshRenderer.enabled = !isHidden;
        }

    public void OnPaused(bool isPaused)
        if(!m_Rigidbody)
            m_Rigidbody = GetComponent<Rigidbody>();
            if(m_Rigidbody)
                if(m_DefaultConstraints == RigidbodyConstraints.None)
                {
                    m_DefaultConstraints = m_Rigidbody.constraints;
                }
                else
                {
                    m_Rigidbody.constraints = m_DefaultConstraints;
                }

        if(m_Rigidbody)
        {
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
        }