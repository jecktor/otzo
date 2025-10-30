using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CustomerMovement : MonoBehaviour
{
	public Transform target;

	NavMeshAgent agent;
	Animator anim;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		if (target != null)
			agent.SetDestination(target.position);
	}

	void Update()
	{
		if (target == null) return;

		// Compute movement speed from the agent’s velocity magnitude
		float speed = agent.velocity.magnitude;

		// Pass it into the Animator’s blend tree
		anim.SetFloat("Speed", speed);

		// Optional: stop agent exactly when it reaches destination
		if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
			anim.SetFloat("Speed", 0f);
	}
}
