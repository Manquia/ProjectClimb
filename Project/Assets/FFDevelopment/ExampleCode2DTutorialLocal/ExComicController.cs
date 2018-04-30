using UnityEngine;
using System.Collections;

/*
 * This is an example script which shows how
 * you might make a comic reader script to move
 * around the camera. This layout uses the FFAction
 * to make a state machine of sorts.
 */

public class ExComicController : FFComponent {

    FFAction.ActionSequence seq;

    float distAlongPath = 0;

    [Range(0.1f,100.0f)]
    public float ComicCameraSpeed;
    
    int currentPointNumber = 0;
    public FFPath PathToFollow;

	// Use this for initialization
	void Start () {
        if(PathToFollow != null && PathToFollow.points.Length > 1)
        {
            seq = action.Sequence();
            transform.position = PathToFollow.PointAlongPath(PathToFollow.LengthAlongPathToPoint(currentPointNumber));
            seq.Call(WaitForInput);
        }
        else
        {
            if (PathToFollow != null)
                Debug.Log("ExComicController has a path which has " + PathToFollow.points.Length + " points which is invalid.");
            else
                Debug.Log("EXComicController has no path to follow");
        }

	}

    public void Move(int index)
    {
        seq.ClearSequence();
        int currIndex = currentPointNumber;
        currentPointNumber = index;

        if (index >= currIndex)// move forward
            MoveForward();
        else
            MoveBackward();
    }

    // Move forward state
    void MoveForward()
    {
        float lengthToNextPoint = PathToFollow.LengthAlongPathToPoint(currentPointNumber);
        if (distAlongPath >= lengthToNextPoint) // reached next point
        {
            transform.position = PathToFollow.PointAlongPath(lengthToNextPoint); // goto point

            seq.Call(WaitForInput); // waitForInput
            return;
        }
        else // keep moving along path
        {
            transform.position = PathToFollow.PointAlongPath(distAlongPath);
        }

        distAlongPath += Time.deltaTime * ComicCameraSpeed;
        seq.Sync();
        seq.Call(MoveForward);
    }

    // Move Backward state
    void MoveBackward()
    {
        float lengthToPrevPoint = PathToFollow.LengthAlongPathToPoint(currentPointNumber);
        if (distAlongPath <= lengthToPrevPoint) // reached begining
        {
            transform.position = PathToFollow.PointAlongPath(lengthToPrevPoint); // goto point
            seq.Call(WaitForInput); // waitForInput
            return;
        }
        else // keep moving along path
        {
            transform.position = PathToFollow.PointAlongPath(distAlongPath);
        }

        distAlongPath -= Time.deltaTime * ComicCameraSpeed;
        seq.Sync();
        seq.Call(MoveBackward);
    }

    // Wait For Input State
    void WaitForInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))  // go forwards
        {
            if (currentPointNumber < PathToFollow.points.Length)
            {
                ++currentPointNumber;
                seq.Call(MoveForward);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))  // go backwards
        {
            if (currentPointNumber > 0)
            {
                --currentPointNumber;
                seq.Call(MoveBackward);
                return;
            }
        }

        seq.Sync();
        seq.Call(WaitForInput);
    }
	
}
