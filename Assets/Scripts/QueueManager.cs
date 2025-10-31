using UnityEngine;
using System.Collections.Generic;

public class QueueManager : MonoBehaviour
{
	[Header("Queue slots (front to back)")]
	public Transform[] queueSlots;

	// Actual queue of customers (front = 0)
	private List<CustomerBehavior> queue = new List<CustomerBehavior>();

	/// <summary>
	/// Called by a customer when they finish shopping and head to checkout.
	/// </summary>
	public Transform RequestSlot(CustomerBehavior customer)
	{
		// Add to queue if not already present
		if (!queue.Contains(customer))
			queue.Add(customer);

		int index = Mathf.Min(queue.Count - 1, queueSlots.Length - 1);
		return queueSlots[index];
	}

	/// <summary>
	/// Called when the first customer in line finishes checkout and leaves.
	/// </summary>
	public void OnCustomerFinished(CustomerBehavior finishedCustomer)
	{
		if (!queue.Contains(finishedCustomer))
			return;

		// Remove the finished customer
		queue.Remove(finishedCustomer);

		// Shift everyone else forward
		UpdateQueuePositions();
	}

	/// <summary>
	/// Returns the current position in line (0 = front).
	/// </summary>
	public int GetPositionInQueue(CustomerBehavior c)
	{
		return queue.IndexOf(c);
	}

	/// <summary>
	/// Makes everyone move up one slot when someone leaves.
	/// </summary>
	private void UpdateQueuePositions()
	{
		for (int i = 0; i < queue.Count; i++)
		{
			CustomerBehavior customer = queue[i];
			if (customer != null)
			{
				// Clamp to available slot range
				int slotIndex = Mathf.Min(i, queueSlots.Length - 1);
				Transform newSlot = queueSlots[slotIndex];

				// Send them to the new position
				var agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();
				if (agent != null)
					agent.SetDestination(newSlot.position);
			}
		}
	}
}
