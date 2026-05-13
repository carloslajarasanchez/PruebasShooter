using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Controla el Multi-Aim Constraint de la cabeza del enemigo.
/// - Detectando jugador  → apunta al jugador suavemente.
/// - Perdiendo jugador   → barrido de búsqueda en la última posición conocida.
/// - Idle / Patrulla     → vuelve a la rotación neutra.
/// </summary>
public class EnemyHeadAim : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────
    [Header("Rig")]
    [SerializeField] private MultiAimConstraint _headConstraint;

    [Header("Target")]
    /// Objeto vacío que el Multi-Aim Constraint usa como fuente.
    /// Muévelo en código; el constraint lo seguirá.
    [SerializeField] private Transform _aimTarget;

    [Header("Seguimiento")]
    [SerializeField] private float _trackSmoothing   = 4f;   // velocidad al seguir al jugador
    [SerializeField] private float _weightBlendSpeed = 3f;   // velocidad de blend del peso del constraint

    [Header("Búsqueda (perdió al jugador)")]
    [SerializeField] private float _searchRadius     = 3f;   // radio del barrido de búsqueda
    [SerializeField] private float _searchDuration   = 0.8f; // tiempo por punto de barrido
    [SerializeField] private int   _searchPoints     = 3;    // cuántos puntos barrer


    // ── Estado interno ────────────────────────────────────────────
    public enum HeadMode { Idle, Tracking, Searching,Scanning }

    private HeadMode _mode = HeadMode.Idle;
    private Transform _playerTransform;

    private Vector3 _targetPosition;      // posición actual del aimTarget
    private Vector3 _searchCenter;        // última posición conocida del jugador
    private Vector3[] _searchPoints3D;    // puntos de barrido generados
    private int   _searchIndex;
    private float _searchTimer;

    // ── API pública ───────────────────────────────────────────────

    /// Llama esto cuando detectas al jugador.
    public void SetTracking(Transform player)
    {
        _playerTransform = player;
        _mode = HeadMode.Tracking;
        SetConstraintWeight(1f);
    }

    /// Llama esto cuando pierdes al jugador; empieza barrido desde lastKnownPos.
    public void SetSearching(Vector3 lastKnownPos)
    {
        _searchCenter = lastKnownPos;
        _playerTransform = null;
        _mode = HeadMode.Searching;
        _searchIndex = 0;
        _searchTimer = 0f;
        GenerateSearchPoints();
        SetConstraintWeight(1f);
    }

    /// Llama esto en Idle / Patrulla.
    public void SetIdle()
    {
        _playerTransform = null;
        _mode = HeadMode.Idle;
        SetConstraintWeight(0f);
    }

    // ── Unity ─────────────────────────────────────────────────────
    private void Update()
    {
        switch (_mode)
        {
            case HeadMode.Tracking:  UpdateTracking();  break;
            case HeadMode.Searching: UpdateSearching(); break;
            case HeadMode.Idle:      UpdateIdle();      break;
        }

        // Mueve el objeto target hacia la posición deseada
        if (_aimTarget != null)
            _aimTarget.position = Vector3.Lerp(
                _aimTarget.position, _targetPosition, Time.deltaTime * _trackSmoothing);
    }

    // ── Modos internos ────────────────────────────────────────────
    private void UpdateTracking()
    {
        if (_playerTransform == null) { SetIdle(); return; }

        // Apunta al centro de masa del jugador (pelvis aprox.)
        _targetPosition = _playerTransform.position + Vector3.up * 1.2f;
    }

    private void UpdateSearching()
    {
        if (_searchPoints3D == null || _searchPoints3D.Length == 0)
        {
            SetIdle();
            return;
        }

        _targetPosition = _searchPoints3D[_searchIndex];
        _searchTimer   += Time.deltaTime;

        if (_searchTimer >= _searchDuration)
        {
            _searchTimer = 0f;
            _searchIndex++;
            if (_searchIndex >= _searchPoints3D.Length)
                SetIdle();
        }
    }

    private void UpdateIdle()
    {
        // Devuelve el target a la posición neutra (delante del enemigo)
        if (_aimTarget != null)
            _targetPosition = transform.position + transform.forward * 3f + Vector3.up * 1.6f;
    }

    // ── Helpers ───────────────────────────────────────────────────
    private void GenerateSearchPoints()
    {
        _searchPoints3D = new Vector3[_searchPoints];
        float angleStep = 360f / _searchPoints;
        for (int i = 0; i < _searchPoints; i++)
        {
            float angle = i * angleStep;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * _searchRadius;
            _searchPoints3D[i] = _searchCenter + offset + Vector3.up * 1.2f;
        }
    }

    private void SetConstraintWeight(float target)
    {
        if (_headConstraint == null) return;
        // Blending suave del peso del constraint
        StartCoroutine(BlendWeight(target));
    }

    private System.Collections.IEnumerator BlendWeight(float target)
    {
        if (_headConstraint == null) yield break;
        while (!Mathf.Approximately(_headConstraint.weight, target))
        {
            _headConstraint.weight = Mathf.MoveTowards(
                _headConstraint.weight, target, Time.deltaTime * _weightBlendSpeed);
            yield return null;
        }
        _headConstraint.weight = target;
    }
}
