using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class ThirdPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.335f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;
		
		
		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;
		[Tooltip("What layers the character uses as ground")]

		public static ThirdPersonController m_ThirdPersonController;

		// player
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;


		// animation IDs
		private int _animIDSpeed;
		private int _animIDMotionSpeed;

		private Animator _animator;
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;


		private bool _hasAnimator;

		public GameObject fireBall;
		private IEnumerator coroutine;

		private bool _alreadyAttacked=false;
		public float timeBetweenAttacks;

		private Transform enemy;
		private void Awake()
		{
			
			if (m_ThirdPersonController == null)
			{
				m_ThirdPersonController = this;
			}
			else
			{
				Destroy(this);
			}
			
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}

			enemy = GameObject.FindWithTag("Enemy").GetComponent<Transform>();

		}

		private void Start()
		{
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();

			AssignAnimationIDs();
			
	#if !UNITY_EDITOR
			MoveSpeed=MoveSpeed/9;
	#endif
		}

		private void Update()
		{
			_hasAnimator = TryGetComponent(out _animator);
			Move();
#if UNITY_EDITOR
			Attack();
#endif
			
		}

		public void Attack()
		{
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Space)&&!_alreadyAttacked)
			{
				transform.LookAt(enemy);
				_alreadyAttacked = true;
				Invoke(nameof(ResetAttack), timeBetweenAttacks);
				_animator.SetTrigger("isAttack");
				//GameObject fb= Instantiate(fireBall, fireBall.transform.position+FindObjectOfType<ThirdPersonController>().transform.position, Quaternion.identity);
				Vector3 playerPos = FindObjectOfType<ThirdPersonController>().transform.position;
				Vector3 playerDirection = FindObjectOfType<ThirdPersonController>().transform.forward;
				float spawnDistance = 2;
	 
				Vector3 spawnPos = playerPos + new Vector3(0,0.5f,0) +playerDirection*spawnDistance;
				GameObject fb=Instantiate(fireBall,spawnPos,Quaternion.identity  );
				
				Vector3 tolerance = new Vector3(0, 0, 0);
				if (!CanTolerance())
				{
					tolerance = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
				}

				fb.GetComponent<Rigidbody>().AddForce((transform.forward+tolerance)*200);
				Invoke(nameof(ResetAttack), timeBetweenAttacks);
				coroutine = DestroyFireBall(fb);
				StartCoroutine(coroutine);
			}	
#else 	
			if (!_alreadyAttacked)
			{
				transform.LookAt(enemy);
				_alreadyAttacked = true;
				Invoke(nameof(ResetAttack), timeBetweenAttacks);
				_animator.SetTrigger("isAttack");
				//GameObject fb= Instantiate(fireBall, fireBall.transform.position+FindObjectOfType<ThirdPersonController>().transform.position, Quaternion.identity);
				Vector3 playerPos = FindObjectOfType<ThirdPersonController>().transform.position;
				Vector3 playerDirection = FindObjectOfType<ThirdPersonController>().transform.forward;
				float spawnDistance = 2;
	 
				Vector3 spawnPos = playerPos + new Vector3(0,0.5f,0) +playerDirection*spawnDistance;
				GameObject fb=Instantiate(fireBall,spawnPos,Quaternion.identity  );
				
				Vector3 tolerance = new Vector3(0, 0, 0);
				if (!CanTolerance())
				{
					tolerance = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
				}

				fb.GetComponent<Rigidbody>().AddForce((transform.forward+tolerance)*200);
				Invoke(nameof(ResetAttack), timeBetweenAttacks);
				coroutine = DestroyFireBall(fb);
				StartCoroutine(coroutine);
			}
#endif
		}
	private bool CanTolerance ()
	{
        if((Random.Range(0, 10)>6))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
		private void ResetAttack()
		{
			_alreadyAttacked = false;
		}
		private IEnumerator DestroyFireBall(GameObject _fireball)
		{
			yield return new WaitForSeconds(2);
			Destroy(_fireball);
		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}


		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

				// rotate to face input direction relative to camera position
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
				_animator.SetFloat("isWalk", 1);
			}

			else
			{
				_animator.SetFloat("isWalk", 0);
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// move the player
			_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// update animator if using character
			if (_hasAnimator)
			{
				//_animator.SetBool("isWalk", true);
				
			}
		}
		

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}