using UnityEngine;

public class ValleyNode
{
    // Child point constructor //
    private ValleyNode(ValleyNode creator, float angle, int depth, bool isBranch, ref Unity.Mathematics.Random rng)
    {
        // Generates a random angle and distance using the pseduo-random number generator //
        float randAngle = rng.NextFloat(angle - 30, angle + 30) % 360;
        float dist = rng.NextFloat(0.3f, 0.8f);

        // Turns the angle into it's sin and cos to calculate it's position //
        float radians = randAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Vector2 offset = new(dist * cos, dist * sin);

        // Assigns the position and the creator //
        m_Position = creator.m_Position + offset;
        m_Creator = creator;

        // If the depth is above the cuttoff does not allow any more children to be generated //
        if (depth > 10) { return; }

        // Has a random chance to stop (1 in 7) cannot happen when the depth is less than 5 long //
        if (rng.NextInt(0, 7) != 1 || depth < 5)
        {
            // Has a 1 in 5 chance to create a branch (only applies when the side has not branched yet) //
            if (rng.NextInt(0, 5) == 1 && isBranch == false)
            {
                // Tracker to alert the a-branch that the valley has branched //
                isBranch = true;

                // Calculates the angle offset and creates the branch //
                float angleOffset = rng.NextFloat(0, 2) == 1 ? angle - 45 : angle + 45;
                m_ChildB = new ValleyNode(this, angleOffset, depth + 1, true, ref rng);
            }

            // Creates the next node along the valley //
            m_ChildA = new ValleyNode(this, angle, depth + 1, isBranch, ref rng);
        }
    }

    // Start point constructor //
    public ValleyNode(uint seed)
    {
        // Creates the random number generator with the given seed //
        Unity.Mathematics.Random rng = new(seed);

        // Sets the starting values //
        m_Creator = null;
        m_Position = Vector2.zero;

        // Creates the two starting children //
        m_ChildA = new ValleyNode(this, 0, 1, false, ref rng);
        m_ChildB = new ValleyNode(this, 180, 1, false, ref rng);
    }

    private void CalculateBoundingBox_R(ref Vector2 min, ref Vector2 max)
    {
        // Updates the smallest tracker //
        min.x = Mathf.Min(min.x, m_Position.x);
        min.y = Mathf.Min(min.y, m_Position.y);

        // Updates the largest tracker //
        max.x = Mathf.Max(max.x, m_Position.x);
        max.y = Mathf.Max(max.y, m_Position.y);

        // Calls the recursive function on the children (if they exist) //
        m_ChildA?.CalculateBoundingBox_R(ref min, ref max);
        m_ChildB?.CalculateBoundingBox_R(ref min, ref max);
    }

    public void CalculateBoundingBox(out Vector2 min, out Vector2 max)
    {
        // Creates the vectors for tracking the size of the bounding box //
        min = new Vector2( Mathf.Infinity,  Mathf.Infinity);
        max = new Vector2(-Mathf.Infinity, -Mathf.Infinity);

        // Calls the recursive function to find the size of the bounding box //
        CalculateBoundingBox_R(ref min, ref max);

        // Adds padding around the x-axis to the next value //
        min.x = (int)min.x - 1;
        max.x = (int)max.x + 1;

        // Adds padding around the y-axis to the next value //
        min.y = (int)min.y - 1;
        max.y = (int)max.y + 1;
    }

    private void Shift_R(Vector2 shift)
    {
        // Shifts it's position //
        m_Position += shift;

        // Calls the recursive function on the children //
        m_ChildA?.Shift_R(shift);
        m_ChildB?.Shift_R(shift);
    }

    public void Shift(out Vector2 size)
    {
        // Calculates the current bounding box //
        CalculateBoundingBox(out Vector2 oMin, out Vector2 oMax);

        // Shifts all of the nodes so they are in positive coordinates //
        Shift_R(new Vector2(Mathf.Abs(oMin.x) + 1, Mathf.Abs(oMin.y) + 1));

        // Returns the new bounding box //
        CalculateBoundingBox(out Vector2 _, out size);
    }

    // The position the node is in //
    Vector2 m_Position = Vector2.zero;
    
    // The connections to make it a double-linked tree //
    readonly ValleyNode m_Creator = null;

    readonly ValleyNode m_ChildA = null;
    readonly ValleyNode m_ChildB = null;
}
