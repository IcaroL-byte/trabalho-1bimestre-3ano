using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Header("Coin Speed Boost Config")]
        [Tooltip("Aumento de velocidade por moeda coletada")]
        public float SpeedBoostPerCoin = 0.5f;

        [Tooltip("Multiplicador da velocidade de corrida em relação à velocidade normal")]
        public float SprintMultiplier = 2.66f;

        // Guarda a velocidade original configurada no Inspector.
        private float _baseMoveSpeed;

        [Header("Rotation")]
        [Tooltip("How fast the player turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Header("Audio")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        [Space(10)]

        [Header("Jump")]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]

        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera")]
        public GameObject CinemachineCameraTarget;

        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        public Vector2 LookSensitivity = new Vector2(7.5f, 5.0f);

        // Cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // Camera
        private Vector3 _cameraStartingPosition;
        private Quaternion _cameraStartingRotation;

        public bool IsRespawning { get; set; } = false;

        // Player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // Timeout
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        // Identidade deste jogador.
        private Player _identity;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse"
                    || _playerInput.currentControlScheme == "Player1";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            // Procura a Main Camera.
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            // Pega os componentes deste MESMO GameObject.
            _identity = GetComponent<Player>();
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            // Guarda a velocidade inicial.
            _baseMoveSpeed = MoveSpeed;
        }

        private void OnEnable()
        {
            // Escuta alterações de moedas.
            PlayerOM.ChangeCoins += OnMoedasAlteradas;
        }

        private void OnDisable()
        {
            // Para de escutar alterações.
            PlayerOM.ChangeCoins -= OnMoedasAlteradas;
        }

        private void Start()
        {
            _cinemachineTargetYaw =
                CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);

            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _identity = GetComponent<Player>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();

            ConfigurarControlScheme();
#else
            Debug.LogError(
                "Starter Assets package is missing dependencies. " +
                "Please use Tools/Starter Assets/Reinstall Dependencies to fix it"
            );
#endif

            AssignAnimationIDs();

            _cameraStartingPosition =
                CinemachineCameraTarget.transform.position;

            _cameraStartingRotation =
                CinemachineCameraTarget.transform.rotation;

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        /// <summary>
        /// Recebe alteração de moedas.
        /// IMPORTANTE:
        /// Este método só aplica a velocidade se o ID recebido
        /// for o mesmo ID deste Player.
        /// </summary>
        private void OnMoedasAlteradas(
            int targetPlayerID,
            int totalCoins)
        {
            if (_identity == null)
            {
                _identity = GetComponent<Player>();
            }

            if (_identity == null)
            {
                Debug.LogWarning(
                    $"[ThirdPersonController] " +
                    $"O objeto {gameObject.name} não possui Player."
                );

                return;
            }

            // NÃO é o evento deste jogador.
            if (targetPlayerID != _identity.PlayerID)
            {
                return;
            }

            // É o evento deste jogador.
            AplicarAumentoDeVelocidade(totalCoins);
        }

        /// <summary>
        /// Calcula a velocidade deste jogador usando
        /// somente as moedas dele.
        /// </summary>
        private void AplicarAumentoDeVelocidade(int totalCoins)
        {
            // Evita valores negativos.
            if (totalCoins < 0)
            {
                totalCoins = 0;
            }

            // Velocidade normal:
            // velocidade base + bônus das moedas deste Player.
            MoveSpeed =
                _baseMoveSpeed +
                (totalCoins * SpeedBoostPerCoin);

            // Sprint também pertence somente a este Player.
            SprintSpeed =
                MoveSpeed * SprintMultiplier;

            Debug.Log(
                $"<color=cyan>[Player {_identity.PlayerID}]</color> " +
                $"Moedas: {totalCoins} | " +
                $"MoveSpeed: {MoveSpeed:F2} | " +
                $"SprintSpeed: {SprintSpeed:F2}"
            );
        }

#if ENABLE_INPUT_SYSTEM
        private void ConfigurarControlScheme()
        {
            if (_playerInput == null)
            {
                return;
            }

            if (_identity == null)
            {
                Debug.LogWarning(
                    "[ThirdPersonController] " +
                    "Player não encontrado."
                );

                return;
            }

            int playerID = _identity.PlayerID;

            switch (playerID)
            {
                case 1:

                    _playerInput.SwitchCurrentControlScheme(
                        "Player1",
                        Keyboard.current
                    );

                    Debug.Log(
                        "<color=green>[ThirdPersonController]</color> " +
                        "Player 1 configurado para Control Scheme: Player1"
                    );

                    break;

                case 2:

                    _playerInput.SwitchCurrentControlScheme(
                        "Player2",
                        Keyboard.current
                    );

                    Debug.Log(
                        "<color=green>[ThirdPersonController]</color> " +
                        "Player 2 configurado para Control Scheme: Player2"
                    );

                    break;

                default:

                    Debug.LogWarning(
                        $"[ThirdPersonController] " +
                        $"PlayerID {playerID} não possui Control Scheme configurado."
                    );

                    break;
            }
        }
#endif

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition =
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                );

            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (_hasAnimator)
            {
                _animator.SetBool(
                    _animIDGrounded,
                    Grounded
                );
            }
        }

        private void CameraRotation()
        {
            if (IsRespawning)
            {
                _cinemachineTargetYaw = 0f;
                _cinemachineTargetPitch = 0f;

                CinemachineCameraTarget.transform.position =
                    _cameraStartingPosition;

                CinemachineCameraTarget.transform.rotation =
                    _cameraStartingRotation;

                IsRespawning = false;

                return;
            }

            if (_input.look.sqrMagnitude >= _threshold
                && !LockCameraPosition)
            {
                float deltaTimeMultiplier =
                    IsCurrentDeviceMouse
                        ? 1.0f
                        : Time.deltaTime;

                _cinemachineTargetYaw +=
                    _input.look.x *
                    deltaTimeMultiplier *
                    LookSensitivity.x;

                _cinemachineTargetPitch +=
                    _input.look.y *
                    deltaTimeMultiplier *
                    LookSensitivity.y;
            }

            _cinemachineTargetYaw =
                ClampAngle(
                    _cinemachineTargetYaw,
                    float.MinValue,
                    float.MaxValue
                );

            _cinemachineTargetPitch =
                ClampAngle(
                    _cinemachineTargetPitch,
                    BottomClamp,
                    TopClamp
                );

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch + CameraAngleOverride,
                    _cinemachineTargetYaw,
                    0.0f
                );
        }

        private void Move()
        {
            // Usa a velocidade deste Player.
            float targetSpeed =
                _input.sprint
                    ? SprintSpeed
                    : MoveSpeed;

            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }

            float currentHorizontalSpeed =
                new Vector3(
                    _controller.velocity.x,
                    0.0f,
                    _controller.velocity.z
                ).magnitude;

            float speedOffset = 0.1f;

            float inputMagnitude =
                _input.analogMovement
                    ? _input.move.magnitude
                    : 1f;

            if (
                currentHorizontalSpeed <
                    targetSpeed - speedOffset
                ||
                currentHorizontalSpeed >
                    targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );

                _speed =
                    Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend =
                Mathf.Lerp(
                    _animationBlend,
                    targetSpeed,
                    Time.deltaTime * SpeedChangeRate
                );

            if (_animationBlend < 0.01f)
            {
                _animationBlend = 0f;
            }

            Vector3 inputDirection =
                new Vector3(
                    _input.move.x,
                    0.0f,
                    _input.move.y
                ).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation =
                    Mathf.Atan2(
                        inputDirection.x,
                        inputDirection.z
                    ) * Mathf.Rad2Deg
                    +
                    _mainCamera.transform.eulerAngles.y;

                float rotation =
                    Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        _targetRotation,
                        ref _rotationVelocity,
                        RotationSmoothTime
                    );

                transform.rotation =
                    Quaternion.Euler(
                        0.0f,
                        rotation,
                        0.0f
                    );
            }

            Vector3 targetDirection =
                Quaternion.Euler(
                    0.0f,
                    _targetRotation,
                    0.0f
                ) * Vector3.forward;

            _controller.Move(
                targetDirection.normalized *
                (_speed * Time.deltaTime)
                +
                new Vector3(
                    0.0f,
                    _verticalVelocity,
                    0.0f
                ) * Time.deltaTime
            );

            if (_hasAnimator)
            {
                _animator.SetFloat(
                    _animIDSpeed,
                    _animationBlend
                );

                _animator.SetFloat(
                    _animIDMotionSpeed,
                    inputMagnitude
                );
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(
                        _animIDJump,
                        false
                    );

                    _animator.SetBool(
                        _animIDFreeFall,
                        false
                    );
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (
                    _input.jump &&
                    _jumpTimeoutDelta <= 0.0f
                )
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            JumpHeight *
                            -2f *
                            Gravity
                        );

                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDJump,
                            true
                        );
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -=
                        Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -=
                        Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDFreeFall,
                            true
                        );
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity +=
                    Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(
            float lfAngle,
            float lfMin,
            float lfMax)
        {
            if (lfAngle < -360f)
            {
                lfAngle += 360f;
            }

            if (lfAngle > 360f)
            {
                lfAngle -= 360f;
            }

            return Mathf.Clamp(
                lfAngle,
                lfMin,
                lfMax
            );
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen =
                new Color(
                    0.0f,
                    1.0f,
                    0.0f,
                    0.35f
                );

            Color transparentRed =
                new Color(
                    1.0f,
                    0.0f,
                    0.0f,
                    0.35f
                );

            if (Grounded)
            {
                Gizmos.color = transparentGreen;
            }
            else
            {
                Gizmos.color = transparentRed;
            }

            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }

        private void OnFootstep(
            AnimationEvent animationEvent)
        {
            if (
                animationEvent.animatorClipInfo.weight
                > 0.5f
            )
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index =
                        Random.Range(
                            0,
                            FootstepAudioClips.Length
                        );

                    AudioSource.PlayClipAtPoint(
                        FootstepAudioClips[index],
                        transform.TransformPoint(
                            _controller.center
                        ),
                        FootstepAudioVolume
                    );
                }
            }
        }

        private void OnLand(
            AnimationEvent animationEvent)
        {
            if (
                animationEvent.animatorClipInfo.weight
                > 0.5f
            )
            {
                AudioSource.PlayClipAtPoint(
                    LandingAudioClip,
                    transform.TransformPoint(
                        _controller.center
                    ),
                    FootstepAudioVolume
                );
            }
        }

        public void ResetCameraRotation(
            float targetYaw)
        {
            _cinemachineTargetYaw = targetYaw;
            _cinemachineTargetPitch = 0f;

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch,
                    _cinemachineTargetYaw,
                    0f
                );

            Debug.Log(
                $"Camera Yaw reset to {targetYaw} degrees."
            );
        }
    }
}