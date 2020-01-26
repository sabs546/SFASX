using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private float SingleNodeMoveTime = 0.5f;

    public EnvironmentTile CurrentPosition { get; set; }

    public IEnumerator DoMove(Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = transform.position;
            float t = 0.0f;
            if (GetComponent<AudioSource>() != null)
            {
                AudioSource[] steps = GetComponents<AudioSource>();
                steps[1].Play();
            }

            while (t < SingleNodeMoveTime)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp(position, destination, t / SingleNodeMoveTime);
                transform.position = p;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoTo(List<EnvironmentTile> route, bool full)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = CurrentPosition.Position;
            for (int count = 0; count < route.Count; ++count)
            {
                if (!full && count == route.Count - 1)
                    break;
                Vector3 next = route[count].Position;
                yield return DoMove(position, next);
                CurrentPosition = route[count];
                position = next;
                GetComponent<CharacterManager>().currentTile = route[count];
                GetComponent<CharacterManager>().characterState = 1;
                CurrentPosition.occupant = gameObject;
            }
            GetComponent<Animator>().SetBool("walking", false);
            GetComponent<CharacterManager>().characterState = 0; // Character is idle after walking
        }
    }

    public void GoTo(List<EnvironmentTile> route, bool full = true)
    {
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route, full));
    }
}
