using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{

    [Header("Components")]
    [SerializeField] GameObject ballPrefab;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CircleCollider2D ccTri;
    [SerializeField] CircleCollider2D ccEfe;
    [SerializeField] PointEffector2D effector;

    [Header("Editable Variables")]
    [SerializeField] [Range(1f, 25f)] float splitSpeedStrength = 10f;
    public bool splitDebug = false;


    private bool active = true;
    private float defualtMass = 0.08f;
    private float targetForce = -8;
    private float targetMass;
    private Vector3 targetScale;
    private bool collisionDetected;
    private Coroutine scaleActive;



    void Start()
    {
        defualtMass = rb.mass;
        targetMass = defualtMass;
        targetScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void OnEnable()
    {
        DefaultParameters();
    }

    void DefaultParameters()
    {
        active = true;
        splitDebug = false;
        rb.mass = defualtMass;
        transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        targetScale = transform.localScale;
        targetForce = -8;
        collisionDetected = false;
    }

    void Update()
    {
        //Click splitDebug in inspector to trigger ball split without condition check
        if (splitDebug) Split();
    }


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (active && collision.gameObject.CompareTag("ball"))
        {
            //Collision flag, prevents OnTriggerEnter2D to trigger twice
            BallScript otherBall = collision.gameObject.GetComponent<BallScript>();
            if (!otherBall.collisionDetected) collisionDetected = true;
           
            //Detect which ball will absorb the other
            if (collisionDetected && otherBall.active)
            {
                BallScript biggerBall;
                BallScript smallerBall;

                if (transform.localScale.x > otherBall.transform.localScale.x)
                {
                    biggerBall = this;
                    smallerBall = otherBall;

                }
                else
                {
                    biggerBall = otherBall;
                    smallerBall = this;
                }


                //Scale down smaller ball to zero, it won't be able to merge with other balls
                if (smallerBall.scaleActive != null) smallerBall.StopCoroutine(smallerBall.scaleActive);
                smallerBall.StartCoroutine(smallerBall.changeScale(1f, true));
                smallerBall.active = false;

                //Scale up bigger ball, it still can absorb smaller balls
                biggerBall.targetMass = biggerBall.transform.localScale.x + smallerBall.transform.localScale.x * 0.25f;
                if (biggerBall.transform.localScale.x < biggerBall.defualtMass * 50)
                {
                    //If ball was already absorbing another ball, stop scaling coroutine and start new one with adjusted targetScale
                    if (biggerBall.scaleActive != null) biggerBall.StopCoroutine(biggerBall.scaleActive);
                   
                    //targetScale and targetForce are values used in coroutine, thanks to them when ball absorbs multiple balls at once, its final values will be correct
                    biggerBall.targetScale = new Vector3(biggerBall.targetScale.x + smallerBall.transform.localScale.x, biggerBall.targetScale.y + smallerBall.transform.localScale.x, biggerBall.targetScale.z + smallerBall.transform.localScale.x);
                    biggerBall.targetForce = biggerBall.targetMass * -400f;
                    biggerBall.scaleActive = biggerBall.StartCoroutine(biggerBall.changeScale(1f, false));
                    biggerBall.ccEfe.radius = Mathf.Clamp(6 - biggerBall.targetScale.x * 2.7f, 1.07f, 6);
                }
                //If ball is big enough, it will split into 50 small balls
                else Split();


            }
        }

    }


    private void Split()
    {
        splitDebug = false;
        active = false;
        effector.enabled = false;
        if (scaleActive != null) StopCoroutine(scaleActive);
        StartCoroutine(changeScale(5f, true));
        for (int i = 0; i <= 50 && GameController.ballCount < 250; i++)
        {
            GameObject ball = GameController.SpawnFromPool("Ball", this.transform.position, Quaternion.identity); 
            BallScript ballScript = ball.GetComponent<BallScript>();
            ballScript.StartCoroutine(ballScript.TurnOffCollision());
            ballScript.transform.localScale = new Vector3(defualtMass, defualtMass, defualtMass);
            ballScript.rb.mass = defualtMass;
            ballScript.rb.velocity = new Vector2(Random.Range(-1, 1f), (Random.Range(-1, 1f))) * splitSpeedStrength;
            GameController.ballCount++;
        }
    }

    IEnumerator TurnOffCollision()
    {
        active = false;
        //Switches collision mask so that ball will not collide with other balls but walls will still stop it
        gameObject.layer = 9;
        ccTri.enabled = false;
        effector.enabled = false;
        yield return new WaitForSeconds(.5f);
        rb.isKinematic = false;
        ccTri.enabled = true;
        effector.enabled = true;
        gameObject.layer = 8;
        active = true;
    }

    //Changes force mode from pull to push
    public void Reverse()
    {
        effector.forceMagnitude *= -1;
        effector.enabled = true;
        active = false;
        rb.isKinematic = false;
        StopAllCoroutines();

    }

    //Smooth variable value change, thanks to this coroutine ball scaling looks better
    public IEnumerator changeScale(float duration, bool delete)
    {
        rb.velocity = Vector3.zero;
        //When balls merge rigidbody must be set to kinematic, otherwise strange movement can occur
        rb.isKinematic = true;
        ccTri.enabled = false;
        for (float time = 0; time < duration * 2; time += Time.deltaTime)
        {
            float progress = time / duration;
            if (!delete)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, progress);
                effector.forceMagnitude = Mathf.Lerp(effector.forceMagnitude, targetForce, progress);
                rb.mass = Mathf.Lerp(rb.mass, targetMass, progress);
            }

            else transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, progress);
            yield return null;
        }

        //Disables smaller ball and adds it to object pool
        if (delete)
        {
            GameController.ballCount--;
            GameController.poolDictionary["Ball"].Enqueue(this.gameObject);
            rb.isKinematic = false;
            this.gameObject.SetActive(false);


        }
        else
        {
            ccTri.enabled = true;
            rb.isKinematic = false;
        }
    }

}


