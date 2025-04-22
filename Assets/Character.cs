using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Character : MonoBehaviour
{
	public bool player = false;
	public string characterName;
	public Color color;
	public Material material;
	public List<Character> attackedCharacters = new List<Character>();

	[Header("Area")]
	public int startAreaPoints = 100;
	public float startAreaRadius = 3f;
	public float minPointDistance = 0.1f;
	public CharacterArea area;
	public GameObject areaOutline;
	public List<Vector3> areaVertices = new List<Vector3>();
	public List<Vector3> newAreaVertices = new List<Vector3>();

	private MeshRenderer areaMeshRend;
	private MeshFilter areaFilter;
	private MeshRenderer areaOutlineMeshRend;
	private MeshFilter areaOutlineFilter;

	[Header("Movement")]
	public float speed = 2f;
	public float turnSpeed = 14f;
	public TrailRenderer trail;
	public GameObject trailCollidersHolder;
	public List<SphereCollider> trailColls = new List<SphereCollider>();

	protected Rigidbody rb;
	protected Vector3 curDir;
	protected Quaternion targetRot;
	private GameObject collisionEffectPrefab;
	private GameObject trailCollisionPrefab;
	private GameObject enemyDeathPrefab;
	private GameObject FastSpeedPrefab;
	private GameObject AreaCrossingPrefab;
	public GameObject PlayerDeathPrefab;
	public float speedBoostAmount = 2f;      // How much to increase the speed
	public float speedBoostDuration = 10f;   // How long the boost lasts (in seconds)
	private bool isSpeedBoosted = false;
	bool isMoving = false;

	private void Awake()
	{

		rb = GetComponent<Rigidbody>();
		trail = transform.Find("Trail").GetComponent<TrailRenderer>();
		trail.material.color = new Color(color.r, color.g, color.b, 0.65f);
		GetComponent<MeshRenderer>().material.color = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f);
	}

	public virtual void Start()
	{
		InitializeCharacter();
		// Get the PlayerMovement component from the same GameObject
		PlayerMovement playerMovement = GetComponent<PlayerMovement>();

		if (playerMovement != null)
		{
			collisionEffectPrefab = playerMovement.collisionEffectPrefab;
			trailCollisionPrefab = playerMovement.trailCollisionPrefab;
			enemyDeathPrefab = playerMovement.enemyDeathPrefab;
			FastSpeedPrefab = playerMovement.FastSpeedPrefab;
			AreaCrossingPrefab = playerMovement.AreaCrossingPrefab;
			PlayerDeathPrefab = playerMovement.PlayerDeathPrefab;
		}
	}

	public virtual void Update()
	{
		var trans = transform;
		var transPos = trans.position;
		trans.position = Vector3.ClampMagnitude(transPos, 73.5f);
		bool isOutside = !GameManager.IsPointInPolygon(new Vector2(transPos.x, transPos.z), Vertices2D(areaVertices));
		int count = newAreaVertices.Count;

		if (isOutside)
		{
			if (count == 0 || !newAreaVertices.Contains(transPos) && (newAreaVertices[count - 1] - transPos).magnitude >= minPointDistance)
			{
				count++;
				newAreaVertices.Add(transPos);

				int trailCollsCount = trailColls.Count;
				float trailWidth = trail.startWidth;
				SphereCollider lastColl = trailCollsCount > 0 ? trailColls[trailCollsCount - 1] : null;
				if (!lastColl || (transPos - lastColl.center).magnitude > trailWidth)
				{
					SphereCollider trailCollider = trailCollidersHolder.AddComponent<SphereCollider>();
					trailCollider.center = transPos;
					trailCollider.radius = trailWidth / 2f;
					trailCollider.isTrigger = true;
					trailCollider.enabled = false;
					trailColls.Add(trailCollider);

					if (trailCollsCount > 1)
					{
						trailColls[trailCollsCount - 2].enabled = true;
					}
				}
			}

			if (!trail.emitting)
			{
				trail.Clear();
				trail.emitting = true;
			}
		}
		else if (count > 0)
		{
			GameManager.DeformCharacterArea(this, newAreaVertices);

			foreach (var character in attackedCharacters)
			{
				List<Vector3> newCharacterAreaVertices = new List<Vector3>();
				foreach (var vertex in newAreaVertices)
				{
					if (GameManager.IsPointInPolygon(new Vector2(vertex.x, vertex.z), Vertices2D(character.areaVertices)))
					{
						newCharacterAreaVertices.Add(vertex);
					}
				}

				GameManager.DeformCharacterArea(character, newCharacterAreaVertices);
			}
			attackedCharacters.Clear();
			newAreaVertices.Clear();

			if (trail.emitting)
			{
				trail.Clear();
				trail.emitting = false;
			}
			foreach (var trailColl in trailColls)
			{
				Destroy(trailColl);
			}
			trailColls.Clear();
		}
	}

	public virtual void FixedUpdate()
	{
		rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);

		if (curDir != Vector3.zero)
		{
			targetRot = Quaternion.LookRotation(curDir);
			if (rb.rotation != targetRot)
			{
				rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed);
			}
		}
	}

	private void InitializeCharacter()
	{
		area = new GameObject().AddComponent<CharacterArea>();
		area.name = characterName + "Area";
		area.character = this;
		Transform areaTrans = area.transform;
		areaFilter = area.gameObject.AddComponent<MeshFilter>();
		areaMeshRend = area.gameObject.AddComponent<MeshRenderer>();
		areaMeshRend.material = material;
		areaMeshRend.material.color = color;

		areaOutline = new GameObject();
		areaOutline.name = characterName + "AreaOutline";
		Transform areaOutlineTrans = areaOutline.transform;
		areaOutlineTrans.position += new Vector3(0, -0.495f, -0.1f);
		areaOutlineTrans.SetParent(areaTrans);
		areaOutlineFilter = areaOutline.AddComponent<MeshFilter>();
		areaOutlineMeshRend = areaOutline.AddComponent<MeshRenderer>();
		areaOutlineMeshRend.material = material;
		areaOutlineMeshRend.material.color = new Color(color.r * .7f, color.g * .7f, color.b * .7f);

		float step = 360f / startAreaPoints;
		for (int i = 0; i < startAreaPoints; i++)
		{
			areaVertices.Add(transform.position + Quaternion.Euler(new Vector3(0, step * i, 0)) * Vector3.forward * startAreaRadius);
		}
		UpdateArea();

		trailCollidersHolder = new GameObject();
		trailCollidersHolder.transform.SetParent(areaTrans);
		trailCollidersHolder.name = characterName + "TrailCollidersHolder";
		trailCollidersHolder.layer = 8;
	}

	public void UpdateArea()
	{
		if (areaFilter)
		{
			Mesh areaMesh = GenerateMesh(areaVertices, characterName);
			areaFilter.mesh = areaMesh;
			areaOutlineFilter.mesh = areaMesh;
			area.coll.sharedMesh = areaMesh;
		}
	}

	private Mesh GenerateMesh(List<Vector3> vertices, string meshName)
	{
		Triangulator tr = new Triangulator(Vertices2D(vertices));
		int[] indices = tr.Triangulate();

		Mesh msh = new Mesh();
		msh.vertices = vertices.ToArray();
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		msh.name = meshName + "Mesh";

		return msh;
	}

	private Vector2[] Vertices2D(List<Vector3> vertices)
	{
		List<Vector2> areaVertices2D = new List<Vector2>();
		foreach (Vector3 vertex in vertices)
		{
			areaVertices2D.Add(new Vector2(vertex.x, vertex.z));
		}

		return areaVertices2D.ToArray();
	}

	public int GetClosestAreaVertice(Vector3 fromPos)
	{
		int closest = -1;
		float closestDist = Mathf.Infinity;
		for (int i = 0; i < areaVertices.Count; i++)
		{
			float dist = (areaVertices[i] - fromPos).magnitude;
			if (dist < closestDist)
			{
				closest = i;
				closestDist = dist;
			}
		}

		return closest;
	}
	private void OnTriggerEnter(Collider other)
	{
		//............................................................................
		if (other.gameObject.CompareTag("PowerUps"))
		{
			Destroy(other.gameObject);

			if (collisionEffectPrefab != null)
			{
				GameObject effect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
				Destroy(effect, 2f); // Destroy the effect after 2 seconds
			}

			if (!isSpeedBoosted)
			{
				StartCoroutine(SpeedBoost());
				GameObject effect = Instantiate(FastSpeedPrefab, transform.position, Quaternion.identity);
				effect.transform.SetParent(transform);

				// Optionally reset local position if needed
				effect.transform.localPosition = Vector3.zero;
				Destroy(effect, 10f); // Destroy the effect after 2 seconds
			}
		}

		Debug.Log($"{gameObject.name} triggered with {other.gameObject.name}");

		CharacterArea characterArea = other.GetComponent<CharacterArea>();

		if (characterArea && characterArea != area && !attackedCharacters.Contains(characterArea.character))
		{
			attackedCharacters.Add(characterArea.character);
		}
		//		Initiate an effect when player enters a character area
		if (other.gameObject.layer == 8 && other.gameObject.CompareTag("Player"))
		{
			GameObject effect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
			Destroy(effect, 2f); // Destroy the effect after 2 seconds
		}
		if (other.gameObject.layer == 12)
		{
			GameObject effect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
			Destroy(effect, 2f); // Destroy the effect after 2 seconds
		}
		if (other.gameObject.layer == 8)
		{
			characterArea = other.transform.parent.GetComponent<CharacterArea>();
			if (characterArea != null && characterArea.character != null)
			{
				Character otherCharacter = characterArea.character;

				if (characterName == "FlopCoat" && otherCharacter.characterName != "FlopCoat")
				{
					Debug.Log($"{characterName} collided with {otherCharacter.characterName} — FlopCoat is dead.");

					GameObject effetc2 = Instantiate(enemyDeathPrefab, otherCharacter.transform.position, Quaternion.identity);

					Vector3 offset = new Vector3(0, -2.5f, 0);
					otherCharacter.transform.position += offset;
					Destroy(effetc2, 2f);

					GameObject effect = Instantiate(trailCollisionPrefab, transform.position, Quaternion.identity);
					Destroy(effect, 2f);
					other.gameObject.layer = 12;
				}
				else if (characterName != "FlopCoat" && otherCharacter.characterName == "FlopCoat")
				{
					Debug.Log(characterName + " collided with " + otherCharacter.characterName + " — FlopCoat is dead.");
					GameObject effect = Instantiate(PlayerDeathPrefab, transform.position, Quaternion.identity);
					Destroy(effect, 2f);
					Vector3 offset = new Vector3(0, -2.5f, 0);
					transform.position += offset;
					StartCoroutine(TimerDelay(3f));
					StopCharacterMovement(otherCharacter.gameObject);
					//					SceneManager.LoadScene(1);
					return;
				}
				else if (characterName == "FlopCoat" && otherCharacter.characterName == "FlopCoat")
				{
					Debug.Log(characterName + " collided with " + otherCharacter.characterName + " — FlopCoat is dead.");
					GameObject effect = Instantiate(PlayerDeathPrefab, otherCharacter.transform.position, Quaternion.identity);
					Destroy(effect, 2f);
					Vector3 offset = new Vector3(0, -2.5f, 0);
					transform.position += offset;
					StartCoroutine(TimerDelay(3f));
					StopCharacterMovement(otherCharacter.gameObject);
					//SceneManager.LoadScene(1);
					return;
				}
				Color myColor = trail != null ? trail.material.color : Color.white;
				Color otherColor = otherCharacter.trail != null ? otherCharacter.trail.material.color : Color.white;
				Color mixedColor = MixColors(myColor, otherColor);

				if (otherCharacter.trail != null) otherCharacter.trail.material.color = mixedColor;
				
				//Change Player Area Color
				ChangeCharacterColor(gameObject, mixedColor);
				ChangeCharacterAreaColor(this, mixedColor);
				// Change Enemy Color
				ChangeCharacterColor(otherCharacter.gameObject, mixedColor);
				ChangeCharacterAreaColor(otherCharacter, mixedColor);
				StopCharacterMovement(otherCharacter.gameObject);
				Debug.Log($"{characterName} and {otherCharacter.characterName} collided — mixed color applied.");
			}
		}
	}
	private IEnumerator TimerDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(1);
	}
	Color MixColors(Color color1, Color color2)
	{
		float h1, s1, v1;
		float h2, s2, v2;

		// Convert both to HSV
		Color.RGBToHSV(color1, out h1, out s1, out v1);
		Color.RGBToHSV(color2, out h2, out s2, out v2);

		// Average hue, saturation, and value
		float mixedH = (h1 + h2) / 2f;
		float mixedS = (s1 + s2) / 2f;
		float mixedV = (v1 + v2) / 2f;

		return Color.HSVToRGB(mixedH, mixedS, mixedV);
	}

	void ChangeCharacterColor(GameObject character, Color color)
	{
		Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renderers)
		{
			renderer.material.color = color;
		}
	}

	void StopCharacterMovement(GameObject character)
	{
		EnemyMovement movementScript = character.GetComponent<EnemyMovement>();
		if (movementScript != null)
		{
			movementScript.enabled = false;
		}

		Rigidbody rb = character.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.isKinematic = true;
			Destroy(rb);
		}
	}
	void ChangeCharacterAreaColor(Character character, Color color)
	{
		if (character.areaMeshRend != null)
		{
			character.areaMeshRend.material.color = color;
		}

		if (character.areaOutlineMeshRend != null)
		{
			character.areaOutlineMeshRend.material.color = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f);
		}
	}
	public void Die()
	{
		if (player)
		{
			GameManager.gm.GameOver();
		}
		else
		{
			Destroy(area.gameObject);
			Destroy(areaOutline);
			Destroy(gameObject);
		}
	}
	private IEnumerator SpeedBoost()
	{
		isSpeedBoosted = true;
		speed *= speedBoostAmount;  // `moveSpeed` should be the variable controlling movement speed

		yield return new WaitForSeconds(speedBoostDuration);

		speed /= speedBoostAmount;  // Revert speed back to normal
		isSpeedBoosted = false;
	}
}