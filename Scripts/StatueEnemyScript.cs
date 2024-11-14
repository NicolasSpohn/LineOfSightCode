using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using Vector3Helper;

public class StatueEnemyScript : MonoBehaviour
{
    //Parameters
    //public float viewRadius;
    public float moveSpeed;
    public float attackRadius;
    public LayerMask sightLayerMask;
    public LayerMask interactionLayerMask;
    public float sightPadding;
    public float interactionReach;
    public float doorOpenTime;

    //Components
    GameStateManagerScript stateManager;
    NavMeshAgent agent;
    new Transform transform;
    private new Renderer renderer;
    Transform rendererTransform;
    Transform camTransform;
    new AudioSource audio;
    float audioVolume;

    Transform player;
    Camera camCamera;
    public BoxCollider renderBounds;

    //Other Data
    public enum EnemyState { Moving, Visible, Halted, Paused }
    public EnemyState enemyState;
    bool canMove => enemyState == EnemyState.Moving;
    

    float doorTimeLeft = 0;
    DoorScript openingDoor;


    private void Start()
    {
        stateManager = GameStateManagerScript.instance;
        agent = GetComponent<NavMeshAgent>();
        transform = base.transform;
        renderer = GetComponentInChildren<Renderer>();
        rendererTransform = renderBounds.transform;
        player = stateManager.player.GetComponent<Transform>();
        camTransform = stateManager.player.camera;
        camCamera = camTransform.GetComponentInChildren<Camera>();
        audio = GetComponent<AudioSource>();
        audioVolume = audio.volume;
    }


    void Update()
    {

        if((int)enemyState < 2) DetermineMovementCapability();
        
        if (!agent.isStopped)
        {
            agent.destination = player.position;
            /*
            if (Vector3.Distance(player.position, transform.position) <= attackRadius) agent.destination = player.position;
            else agent.destination = sneakUpTarget.position;
             */
        }

        if(canMove) ForwardInteraction();

        if (doorTimeLeft > 0 && enemyState == EnemyState.Halted)
        {
            doorTimeLeft -= Time.deltaTime;
            if(doorTimeLeft <= 0.4)
            {
                openingDoor?.Interact(this);
                openingDoor = null;
            }
            if(doorTimeLeft <= 0)
            {
                ChangeMovementCapability(EnemyState.Visible);
            }
        }

    }

    void DetermineMovementCapability()
    {
        if (!IsPlayerLooking()) ChangeMovementCapability(EnemyState.Moving);
        else
        {
            if (IsSightBlockedV3()) ChangeMovementCapability(EnemyState.Moving);
            else ChangeMovementCapability(EnemyState.Visible);
        }
        

    }
    void ChangeMovementCapability(EnemyState value)
    {
        enemyState = value;
        agent.isStopped = value != EnemyState.Moving;
        if (value != EnemyState.Moving)
        {
            agent.velocity = Vector3.zero;
            agent.destination = agent.nextPosition;
        }
        audio.volume = value == EnemyState.Moving ? audioVolume : 0;
    }


    //Frustrum Check Code from https://youtu.be/_e57zSZSOS8
    bool IsPlayerLooking() => GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camCamera), renderer.bounds);    

    bool IsSightBlockedV3()
    {
        //Bounds rendererBounds = renderer.bounds;
        Position centerPos = rendererTransform.TransformPoint(renderBounds.center);

        Position forwardRightPos = (Vector3)centerPos + (rendererTransform.forward * (renderBounds.size.z/2 + sightPadding))    + (rendererTransform.right * (renderBounds.size.x/2 + sightPadding));
        Position forwardLeftPos =  (Vector3)centerPos + (rendererTransform.forward * (renderBounds.size.z/2 + sightPadding))    - (rendererTransform.right * (renderBounds.size.x/2 + sightPadding));
        Position backRightPos =    (Vector3)centerPos - (rendererTransform.forward * (renderBounds.size.z/2 + sightPadding))    + (rendererTransform.right * (renderBounds.size.x/2 + sightPadding));
        Position backLeftPos =     (Vector3)centerPos - (rendererTransform.forward * (renderBounds.size.z / 2 + sightPadding))  - (rendererTransform.right * (renderBounds.size.x/2 + sightPadding));


        bool originHit = SeeRay(transform.position);
        bool midHit = SeeRay(centerPos);
        bool forwardRightHit = SeeRay(forwardRightPos);
        bool backRightHit = SeeRay(backRightPos);
        bool forwardLeftHit = SeeRay(forwardLeftPos);
        bool backLeftHit = SeeRay(backLeftPos);


        return !(originHit || midHit || forwardLeftHit || forwardRightHit || backLeftHit || backRightHit);
    }

    bool SeeRay(Position end)
    {
        Vector3 withinCam = camCamera.WorldToViewportPoint(end);
        if (!(withinCam.z > 0 && withinCam.x < 1 && withinCam.x > 0 && withinCam.y < 1 && withinCam.y > 0)) return false;

        RaycastHit hit;
        Physics.Linecast(camCamera.transform.position, end, out hit, sightLayerMask, QueryTriggerInteraction.Ignore);
        bool result = hit.collider == null || hit.point == end;

        Debug.DrawLine(camCamera.transform.position, end, result ? Color.white : Color.red);
        return result;

    }
    [SerializeField] Vector3 dForLeftPos;
    [SerializeField] Vector3 dForLeftHitPos;

    void ForwardInteraction()
    {
        RaycastHit result;
        if (!Physics.Raycast(transform.position, transform.forward, out result, interactionReach, interactionLayerMask, QueryTriggerInteraction.Collide)) return;
        if (result.transform == player.transform) KillPlayer();
        else
        {
            openingDoor = result.transform.GetComponent<DoorScript>();
            if (openingDoor && openingDoor.isClosed) BeginDoorOpening();
        }
    }

    void KillPlayer()
    {
        ChangeMovementCapability(EnemyState.Halted);
        GameStateManagerScript.instance.player.BeginDeath();
    }

    void BeginDoorOpening()
    {
        ChangeMovementCapability(EnemyState.Halted);
        doorTimeLeft = doorOpenTime + 0.4f;

    }

    EnemyState beforePauseState;
    public void SetPause(bool value)
    {
        if (value) beforePauseState = enemyState;
        ChangeMovementCapability(value ? EnemyState.Paused : beforePauseState);
    }















    
    /* Unused
LayerMask sightLayerMask_Old;
float statueRadius_Old;
Transform sneakUpTarget_Old;
void SetSneakTargetPosition()
{
    Vector3 toPlayerDirection = (player.position - transform.position).normalized;

    //1 if same direction, -1 if opposite, 0 if perpendicular
    float DotProd = Vector3.Dot(-toPlayerDirection, player.forward.normalized);

    float playerStatueDistance = Vector3.Distance(player.position, transform.position);
    float distanceFromPlayer = playerStatueDistance * DotProd;

    Vector3 direction = -player.right * Mathf.Sign(Vector3.Dot(player.right, toPlayerDirection));
    //Vector3 backPadding = -player.forward * Mathf.Abs(Vector3.Dot(player.right, toPlayerDirection));

    sneakUpTarget_Old.position = player.position + direction * distanceFromPlayer;

    //Debug.DrawRay(player.position, -toPlayerDirection);
}
bool IsSightBlockedV1()
{
    float midDistance = Vector3.Distance(transform.position, player.position);
    Vector3 midDirection = (transform.position - player.position).normalized;
    RaycastHit hit;
    Physics.Raycast(player.position, midDirection, out hit, midDistance, sightLayerMask_Old);

    bool midHit = (hit.transform == transform);

    float angle = Mathf.Rad2Deg*Mathf.Atan(statueRadius_Old/midDistance);

    Vector3 leftDirection = Quaternion.AngleAxis(-angle, Vector3.up) * midDirection;
    Vector3 rightDirection = Quaternion.AngleAxis(angle, Vector3.up) * midDirection; ;

    float sideDistance = Mathf.Sqrt(Mathf.Pow(midDistance, 2) + Mathf.Pow(statueRadius_Old, 2));

    Physics.Raycast(player.position, leftDirection, out hit, sideDistance, sightLayerMask_Old);
    bool leftHit = (hit.transform == transform);
    Physics.Raycast(player.position, rightDirection, out hit, sideDistance, sightLayerMask_Old);
    bool rightHit = (hit.transform == transform);

    Debug.DrawRay(player.position, midDirection * midDistance, midHit? Color.white:Color.red);
    Debug.DrawRay(player.position, leftDirection * sideDistance, leftHit ? Color.white:Color.red);
    Debug.DrawRay(player.position, rightDirection * sideDistance, rightHit ? Color.white:Color.red);


    return !(midHit || rightHit || leftHit);
}
bool IsSightBlockedV2()
{
    Position playerPos = player.transform.position;
    Position enemyPosCenter = transform.position;

    Direction enemyToPlayerDirection = new Direction(playerPos - enemyPosCenter).normalized;
    Direction enemyToPlayerPerpendicular = enemyToPlayerDirection.Rotate(90, Direction.up);

    bool midHit = SeeRay(enemyPosCenter);
    bool leftHit = SeeRay(enemyPosCenter - (enemyToPlayerPerpendicular * statueRadius_Old));
    bool rightHit = SeeRay(enemyPosCenter + (enemyToPlayerPerpendicular * statueRadius_Old));

    return !(midHit || rightHit || leftHit);
}
 */

}
