using Unity.VisualScripting;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject bubblePrefab;   // 발사할 버블
    public float launchSpeed = 6f;

    public NextBubbleSprite nextBubbleSprite;

    private const float MAX_ALLOWED_ANGLE = 75f;

    private float shootCooldownTimer = 0f;
    private const float SHOOT_COOLDOWN_DURATION = 0.5f;

    private int testStep = 0; // 테스트 각도를 추적하는 변수
    private float[] testAngles = { 0f, 90f, 180f, 270f }; // 테스트할 각도 리스트

    private Bubble currentBubble;
    private Bubble nextBubble;

    private Vector3 debugWorldPos;


    private void Start()
    {
        // 시작할 때 두 개 생성
        currentBubble = BubbleFactory.Instance.CreateRandomBubble(transform.position);
        nextBubble = BubbleFactory.Instance.CreateRandomBubble(new Vector3(10000, 10000, 0)); // 화면 밖
        UpdateNextBubbleSprite();
    }

    private void Update()
    {
        if (shootCooldownTimer > 0)
        {
            shootCooldownTimer -= Time.deltaTime;
        }

        Aim();

        // 쿨다운이 0 이하일 때만 발사 허용
        if (Input.GetMouseButtonDown(0) && shootCooldownTimer <= 0)
        {
            Shoot();
        }
    }

    private void OnDrawGizmos()
    {
        if (debugWorldPos != Vector3.zero)
        {
            // Gizmos.color = Color.yellow;
            //Gizmos.DrawSphere(debugWorldPos, 0.5f);
        }
    }

    private void TestFixedAim()
    {
        // 1초마다 다음 테스트 각도로 전환
        if (Time.frameCount % 60 == 0) // 60프레임마다
        {
            // testStep = (testStep + 1) % testAngles.Length;
        }

        testStep = 1;

        float currentTestAngle = testAngles[testStep];

        Quaternion rotation = Quaternion.Euler(0, 0, currentTestAngle);


        transform.rotation = rotation;



        // Ray 시각화 (길이 5f)
        // Debug.DrawRay(transform.position, finalDirectionVector * 5f, Color.red);
        // Debug.Log($"테스트 각도: {currentTestAngle}도, 벡터: {finalDirectionVector}");
    }

    // 마우스 위치를 기반으로 하여 조준
    private void Aim()
    {
        if (currentBubble == null)
        {
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 shooterPos2D = transform.position;
        Vector2 mousePos2D = worldMousePos;

        Vector2 pullDirection = (shooterPos2D - mousePos2D).normalized;

        float dotProduct = Vector2.Dot(pullDirection, Vector2.up);

        dotProduct = Mathf.Clamp(dotProduct, -1f, 1f);

        float thetaRad = Mathf.Acos(dotProduct);
        float thetaDeg = thetaRad * Mathf.Rad2Deg;


        // Debug.Log($"Theta Deg (벌어진 각도의 크기): {thetaDeg:F2}도");


        float clampedAngle = Mathf.Min(thetaDeg, MAX_ALLOWED_ANGLE);

        // 아크코사인으로 각도로 변환하면, 방향성을 상실한다.
        // 따라서 방향벡터로 다시 변환하기 위해서는 방향성을 구해야한다.
        
        // 마우스가 왼쪽에 있으면 (-1f)로 시계 방향, 오른쪽에 있으면 (+1f)로 반시계 방향 회전
        float directionSign = (mousePos2D.x < transform.position.x) ? -1f : 1f;

        // 0도는 +X축 방향
        // 90도 = UpVector
        float finalRotationZ = clampedAngle * directionSign;

        transform.rotation = Quaternion.Euler(0, 0, finalRotationZ);        

    }

    private void Shoot()
    {
        if (currentBubble == null)
        {
            return;
        }

        // 발사 방향 = 조준 회전값의 Up
        Vector2 shootDir = transform.up;

        currentBubble.Launch(shootDir, launchSpeed);
        currentBubble = null;

        // 쿨다운 타이머 시작
        shootCooldownTimer = SHOOT_COOLDOWN_DURATION;
    }

    public void CreateNextBubble()
    {
        // NextBubble이 CurrentBubble이 됨
        currentBubble = nextBubble;

        // 화면밖에 만들었으므로 다시 원래 위치로
        currentBubble.transform.position = transform.position;

        // 새 NextBubble 생성 (화면 밖)
        nextBubble = BubbleFactory.Instance.CreateRandomBubble(new Vector3(10000, 10000, 0));

        UpdateNextBubbleSprite();
    }

    private void UpdateNextBubbleSprite()
    {
        nextBubbleSprite.SetNextBubble(nextBubble.GetComponent<SpriteRenderer>().sprite);
    }
}
