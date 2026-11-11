using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField, Min(0.1f)] float tileSize = 1f;
    [SerializeField] int Points;

    // Retained for existing Patrol scripts and prefab compatibility.
    public bool isChasing;

    enum Personality { Blinky, Pinky, Inky, Clyde }
    enum Mode { Scatter, Chase, Frightened, Returning }

    static readonly Vector3[] Directions =
        { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };
    static readonly float[] FirstLevel = { 7f, 20f, 7f, 20f, 5f, 20f, 5f };
    static readonly float[] MiddleLevels = { 7f, 20f, 7f, 20f, 5f, 1033f, 1f / 60f };
    static readonly float[] LaterLevels = { 5f, 20f, 5f, 20f, 5f, 1037f, 1f / 60f };
    static int sceneHandle = int.MinValue, clockFrame = -1, phase;
    static float phaseTime;
    static float[] schedule = FirstLevel;
    static Bounds maze;
    static bool hasBounds;
    static GhostScript blinky;

    NavMeshAgent agent;
    AudioSource death;
    ScoreSystemScript score;
    Personality personality;
    Mode mode;
    Vector3 home, legEnd;
    float normalSpeed, legStarted, nextRetry, normalStoppingDistance;
    int direction = -1, seenPhase, seenPower, eatenPower = -1;
    bool hasLeg, reversePending;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        death = GetComponent<AudioSource>();
        normalSpeed = agent.speed;
        normalStoppingDistance = agent.stoppingDistance;
        home = transform.position;
        personality = Identify(gameObject.name);
        if (GetType() == typeof(GhostScript))
        {
            var prefabBrain = GetComponent<GhostPrefabScript>();
            if (prefabBrain != null && prefabBrain.enabled) enabled = false;
        }

        // A ghost must have exactly one writer of its agent's destination.
        var patrol = GetComponent<PatrolScript>();
        if (patrol != null) patrol.enabled = false;
        var prefabPatrol = GetComponent<PatrolPrefabScript>();
        if (prefabPatrol != null) prefabPatrol.enabled = false;
    }

    static Personality Identify(string name)
    {
        string identity = name.ToLowerInvariant();
        return identity.Contains("blinky") || identity.Contains("red") ? Personality.Blinky :
            identity.Contains("pinky") || identity.Contains("pink") ? Personality.Pinky :
            identity.Contains("inky") || identity.Contains("blue") ? Personality.Inky :
            identity.Contains("clyde") || identity.Contains("orange") ? Personality.Clyde : Personality.Blinky;
    }

    protected virtual void Start()
    {
        InitialiseScene();
        if (personality == Personality.Blinky) blinky = this;
        if (player == null) player = GameObject.FindWithTag("Player");
        var controller = GameObject.FindWithTag("GameController");
        if (controller != null) score = controller.GetComponent<ScoreSystemScript>();
        seenPhase = phase;
        seenPower = PacManScript.PowerActivation;
    }

    public void StartChase(GameObject target)
    {
        player = target;
    }

    static void InitialiseScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (sceneHandle == scene.handle) return;
        sceneHandle = scene.handle;
        phase = 0;
        phaseTime = 0f;
        clockFrame = -1;
        blinky = null;
        hasBounds = false;
        int level = 1;
        if (scene.name.StartsWith("Level ") && int.TryParse(scene.name.Substring(6), out int parsed))
            level = parsed;
        schedule = level == 1 ? FirstLevel : level <= 4 ? MiddleLevels : LaterLevels;
    }

    static void TickClock()
    {
        if (clockFrame == Time.frameCount) return;
        clockFrame = Time.frameCount;
        if (PacManScript.isPower) return;
        phaseTime += Time.deltaTime;
        while (phase < schedule.Length && phaseTime >= schedule[phase])
        {
            phaseTime -= schedule[phase];
            phase++;
        }
    }

    Vector3 ScatterTarget()
    {
        if (!hasBounds)
        {
            var vertices = NavMesh.CalculateTriangulation().vertices;
            if (vertices.Length == 0) return home;
            maze = new Bounds(vertices[0], Vector3.zero);
            foreach (var vertex in vertices) maze.Encapsulate(vertex);
            hasBounds = true;
        }
        bool right = personality == Personality.Blinky || personality == Personality.Inky;
        bool top = personality == Personality.Blinky || personality == Personality.Pinky;
        return new Vector3(right ? maze.max.x + tileSize : maze.min.x - tileSize,
            transform.position.y, top ? maze.max.z + tileSize : maze.min.z - tileSize);
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    static Vector3 ArcadeOffset(Vector3 heading, float tiles)
    {
        // The original arcade upward look-ahead bug also offsets left.
        return (heading == Vector3.forward ? heading + Vector3.left : heading) * tiles;
    }

    Vector3 ChaseTarget()
    {
        var position = player.transform.position;
        var pacman = player.GetComponent<PacManScript>();
        var movement = Flat(pacman != null ? pacman.TravelHeading : player.transform.forward);
        var heading = Mathf.Abs(movement.x) > Mathf.Abs(movement.z)
            ? (movement.x < 0f ? Vector3.left : Vector3.right)
            : (movement.z < 0f ? Vector3.back : Vector3.forward);
        switch (personality)
        {
            case Personality.Pinky: return position + ArcadeOffset(heading, 4f * tileSize);
            case Personality.Inky:
                var pivot = position + ArcadeOffset(heading, 2f * tileSize);
                return blinky != null ? pivot + Flat(pivot - blinky.transform.position) : pivot;
            case Personality.Clyde:
                return Flat(transform.position - position).sqrMagnitude < 64f * tileSize * tileSize
                    ? ScatterTarget() : position;
            default: return position;
        }
    }

    NavMeshQueryFilter Filter => new NavMeshQueryFilter
    {
        agentTypeID = agent.agentTypeID, areaMask = agent.areaMask
    };

    bool TryStep(Vector3 origin, int turn, float length, out Vector3 point)
    {
        point = origin;
        Vector3 desired = origin + Directions[turn] * length;
        float tolerance = Mathf.Min(length * 0.2f, 0.15f);
        if (!NavMesh.SamplePosition(desired, out var hit, tolerance, Filter) ||
            NavMesh.Raycast(origin, hit.position, out _, Filter))
            return false;
        point = hit.position;
        return true;
    }

    bool SelectStep(Vector3 origin, Vector3 target, float length, bool allowDeadEndReverse)
    {
        int reverse = direction < 0 ? -1 : (direction + 2) % Directions.Length;
        if (reversePending && reverse >= 0 && TryStep(origin, reverse, length, out var turnBack))
            return SetLeg(turnBack, reverse);

        int chosen = -1, eligible = 0;
        float best = float.PositiveInfinity;
        Vector3 destination = origin;
        for (int i = 0; i < Directions.Length; i++)
        {
            if (i == reverse || !TryStep(origin, i, length, out var next)) continue;
            float distance = Flat(target - next).sqrMagnitude;
            // Strict comparison preserves arcade tie priority: up, left, down, right.
            bool take = mode == Mode.Frightened
                ? Random.Range(0, ++eligible) == 0 : distance < best;
            if (!take) continue;
            chosen = i;
            destination = next;
            best = distance;
        }
        if (chosen >= 0) return SetLeg(destination, chosen);
        return allowDeadEndReverse && reverse >= 0 && TryStep(origin, reverse, length, out var escape)
            && SetLeg(escape, reverse);
    }

    bool SetLeg(Vector3 destination, int turn)
    {
        if (!agent.SetDestination(destination)) return false;
        direction = turn;
        legEnd = destination;
        legStarted = Time.time;
        hasLeg = true;
        reversePending = false;
        return true;
    }

    protected virtual void Update()
    {
        TickClock();
        if (!agent.isOnNavMesh || player == null) return;

        if (mode == Mode.Returning)
        {
            if (!hasLeg)
            {
                if (NavMesh.SamplePosition(home, out var spawn, tileSize * 3f, Filter))
                    SetLeg(spawn.position, -1);
            }
            if (!hasLeg || agent.pathPending) return;
            if (Flat(agent.nextPosition - legEnd).sqrMagnitude > 0.25f * tileSize * tileSize)
                return;
            hasLeg = false;
            // Do not award points twice on the same pellet, or hurt powered Pac-Man.
            mode = phase % 2 == 0 ? Mode.Scatter : Mode.Chase;
            seenPhase = phase;
            reversePending = false;
        }

        bool frightened = PacManScript.isPower && eatenPower != PacManScript.PowerActivation;
        var desired = frightened ? Mode.Frightened : phase % 2 == 0 ? Mode.Scatter : Mode.Chase;
        if ((desired == Mode.Frightened && (mode != desired || seenPower != PacManScript.PowerActivation)) ||
            (mode != Mode.Frightened && desired != Mode.Frightened && (phase - seenPhase) % 2 != 0))
            reversePending = !reversePending;
        seenPhase = phase;
        seenPower = PacManScript.PowerActivation;
        mode = desired;
        isChasing = mode == Mode.Chase;
        agent.speed = normalSpeed * (frightened ? 0.6f : 1f);
        agent.stoppingDistance = Mathf.Min(normalStoppingDistance, tileSize * 0.1f);

        if (agent.pathPending || Time.time < nextRetry) return;
        float arrival = tileSize * 0.18f;
        if (hasLeg && Flat(agent.nextPosition - legEnd).sqrMagnitude > arrival * arrival &&
            agent.hasPath && Time.time - legStarted < Mathf.Max(2f, 3f * tileSize / Mathf.Max(agent.speed, 0.1f)))
            return;

        hasLeg = false;
        var target = mode == Mode.Chase ? ChaseTarget() : ScatterTarget();
        var origin = agent.nextPosition;
        if (SelectStep(origin, target, tileSize, false) ||
            SelectStep(origin, target, tileSize * 0.4f, true)) return;
        // Narrow or irregular NavMesh geometry may not admit a cardinal step.
        if (mode != Mode.Frightened &&
            NavMesh.SamplePosition(target, out var fallback, tileSize * 6f, Filter) &&
            Flat(fallback.position - origin).sqrMagnitude > arrival * arrival)
            SetLeg(fallback.position, -1);
        nextRetry = Time.time + 0.12f;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Unity delivers trigger messages even to disabled MonoBehaviours.
        if (!isActiveAndEnabled || !other.CompareTag("Player") || mode == Mode.Returning) return;
        if (PacManScript.isPower)
        {
            if (eatenPower == PacManScript.PowerActivation) return;
            eatenPower = PacManScript.PowerActivation;
            mode = Mode.Returning;
            isChasing = false;
            hasLeg = false;
            reversePending = false;
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.speed = normalSpeed * 1.6f;
                agent.stoppingDistance = 0f;
            }
            if (death != null && death.clip != null) death.Play();
            if (score != null) score.AddScore(Points);
            return;
        }
        var pacman = other.GetComponent<PacManScript>();
        if (pacman == null) return;
        PlayerPrefs.SetInt("CurrentScore", ScoreSystemScript.Score);
        PlayerPrefs.Save();
        pacman.LoseLife();
    }

    public static void ResetRound()
    {
        phase = 0;
        phaseTime = 0f;
        clockFrame = Time.frameCount;
        foreach (var ghost in FindObjectsOfType<GhostScript>())
        {
            if (!ghost.isActiveAndEnabled) continue;
            ghost.mode = Mode.Scatter;
            ghost.seenPhase = 0;
            ghost.direction = -1;
            ghost.eatenPower = -1;
            ghost.hasLeg = false;
            ghost.reversePending = false;
            ghost.nextRetry = 0f;
            ghost.isChasing = false;
            if (ghost.agent == null || !ghost.agent.isOnNavMesh) continue;
            ghost.agent.ResetPath();
            if (NavMesh.SamplePosition(ghost.home, out var spawn, ghost.tileSize * 3f, ghost.Filter))
                ghost.agent.Warp(spawn.position);
        }
    }
}
