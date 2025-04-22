using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : Character
{
	public float sensitivity = 300f;
	public float turnTreshold = 15f;
	private Vector3 mouseStartPos;
	public TextMeshProUGUI text2;
	public TextMeshProUGUI text3;
	public TextMeshProUGUI text4;
	public TextMeshProUGUI text5;
	public TextMeshProUGUI text6;
	public float value = 0f;
	public float value2 = 0f;
	public float value3 = 0f;
	public float value4 = 0f;
	public float value5 = 0f;
	public GameObject collisionEffectPrefab; // Prefab for the collision effect
	public GameObject trailShineEffectPrefab;
	public GameObject trailCollisionPrefab;
	public GameObject enemyDeathPrefab;
	public GameObject FastSpeedPrefab;
	public GameObject AreaCrossingPrefab;
	public GameObject PlayerDeathPrefab;
	private float lastShineTime = 0f;
	public float rotationSpeed = 90f; // degrees per second


	public override void Update()
	{

		if (Time.time - lastShineTime >= 2f)
		{
			GameObject trailShineEffect = Instantiate(trailShineEffectPrefab, transform.position, Quaternion.identity);

			// Parent the effect to the player so it moves with them
			trailShineEffect.transform.SetParent(transform);

			// Optionally unparent after 2 seconds and destroy it
			StartCoroutine(DestroyAfterDelay(trailShineEffect, 2f));

			lastShineTime = Time.time;
		}

		GameData.PlayerScore += 0.0010f; // increment every frame
		text2.text = value.ToString("F2") + "%";// show 2 decimal places

		value2 += 0.00001f; // increment every frame
		text3.text = value2.ToString("F2") + "%";// show 2 decimal places

		value3 += 0.00030f; // increment every frame
		text4.text = value3.ToString("F2") + "%";// show 2 decimal places

		value4 += 0.00040f; // increment every frame
		text5.text = value4.ToString("F2") + "%";// show 2 decimal places

		value5 += 0.00050f; // increment every frame
		text6.text = value5.ToString("F2") + "%";// show 2 decimal places
		var mousePos = Input.mousePosition;
		if (Input.GetMouseButtonDown(0))
		{
			mouseStartPos = mousePos;
		}
		else if (Input.GetMouseButton(0))
		{
			float distance = (mousePos - mouseStartPos).magnitude;
			if (distance > turnTreshold)
			{
				if (distance > sensitivity)
				{
					mouseStartPos = mousePos - (curDir * sensitivity / 2f);
				}

				var curDir2D = -(mouseStartPos - mousePos).normalized;
				curDir = new Vector3(curDir2D.x, 0, curDir2D.y);
			}
		}
		else
		{
			curDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
		}

		base.Update();
	}
	private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (obj != null)
		{
			obj.transform.SetParent(null);
			Destroy(obj);
		}
	}
}