using System;
using UnityEngine;

public class ValleyNode
{
    // Child point constructor //
    private ValleyNode(ValleyNode creator, float angle, int depth, bool isBranch)
    {
        float randAngle = UnityEngine.Random.Range(angle - 30, angle + 30) % 360;
        float dist = UnityEngine.Random.Range(0.3f, 0.8f);

        float radians = randAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Vector2 offset = new(dist * cos, dist * sin);

        m_Position = creator.m_Position + offset;
        m_Creator = creator;

        if (depth > 10)
        {
            return;
        }

        if (UnityEngine.Random.Range(0, 7) != 1 || depth < 5)
        {
            bool branched = isBranch;

            if (UnityEngine.Random.Range(0, 5) == 1 && branched == false)
            {
                branched = true;

                if (UnityEngine.Random.Range(0, 2) == 1)
                {
                    m_ChildB = new ValleyNode(this, angle - 45, depth + 1, branched);
                }

                else
                {
                    m_ChildB = new ValleyNode(this, angle + 45, depth + 1, branched);
                }
            }

            m_ChildA = new ValleyNode(this, angle, depth + 1, branched);
        }
    }

    // Start point constructor //
    public ValleyNode()
    {
        m_Creator = null;

        m_Position = Vector2.zero;

        // Creates the two starting children //
        m_ChildA = new ValleyNode(this, 0, 1, false);
        m_ChildB = new ValleyNode(this, 180, 1, false);
    }

    private void CalculateBoundingBox_R(ref Vector2 min, ref Vector2 max)
    {
        min.x = Mathf.Min(min.x, m_Position.x);
        min.y = Mathf.Min(min.y, m_Position.y);

        max.x = Mathf.Max(max.x, m_Position.x);
        max.y = Mathf.Max(max.y, m_Position.y);

        if (m_ChildA != null)
        {
            m_ChildA.CalculateBoundingBox_R(ref min, ref max);
        }

        if (m_ChildB != null)
        {
            m_ChildB.CalculateBoundingBox_R(ref min, ref max);
        }
    }

    public void CalculateBoundingBox(out Vector2 min, out Vector2 max)
    {
        min = new Vector2( Mathf.Infinity,  Mathf.Infinity);
        max = new Vector2(-Mathf.Infinity, -Mathf.Infinity);

        CalculateBoundingBox_R(ref min, ref max);

        min.x = (int)min.x - 2;
        min.y = (int)min.y - 2;

        max.x = (int)max.x + 2;
        max.y = (int)max.y + 2;
    }

    public void Draw()
    {
        if (m_ChildA != null)
        {
            Debug.DrawLine(m_Position, m_ChildA.m_Position);
            m_ChildA.Draw();
        }

        if (m_ChildB != null)
        {
            Debug.DrawLine(m_Position, m_ChildB.m_Position);
            m_ChildB.Draw();
        }
    }

    public void CallFuncOnNodes(Action<ValleyNode> action)
    {
        // Calls the function on itself //
        action(this);

        // Calls the function on it's children (if it has any) //
        m_ChildA?.CallFuncOnNodes(action);
        m_ChildB?.CallFuncOnNodes(action);
    }

    public Vector3 Position() => new Vector3(m_Position.x, 0, m_Position.y);
    public ValleyNode Creator() => m_Creator;

    ValleyNode m_Creator = null;

    Vector2 m_Position = Vector2.zero;

    ValleyNode m_ChildA = null;
    ValleyNode m_ChildB = null;
}

public class ValleyGen : MonoBehaviour
{
    public ValleyNode m_Start = null;

    void Start()
    {
        m_Start = new ValleyNode();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            m_Start = new ValleyNode();
        }

        m_Start.Draw();

        // Draws the bounding box //
        m_Start.CalculateBoundingBox(out Vector2 min, out Vector2 max);

        Vector2 tl = new Vector2(min.x, min.y);
        Vector2 tr = new Vector2(min.x, max.y);

        Vector2 bl = new Vector2(max.x, min.y);
        Vector2 br = new Vector2(max.x, max.y);

        Debug.DrawLine(tl, tr);
        Debug.DrawLine(bl, br);

        Debug.DrawLine(tr, br);
        Debug.DrawLine(bl, tl);
    }
}
