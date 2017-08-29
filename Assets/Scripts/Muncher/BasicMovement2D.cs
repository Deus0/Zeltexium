using UnityEngine;
using System.Collections;

namespace Zeltex.AI
{
    public class BasicMovement2D : MonoBehaviour
    {
        Rigidbody2D MyRigid;
        public GameObject MyBulletPrefab;
        [Header("Movement")]
        public float MovementSpeed = 1.5f;
        public float SlowDownRate = 0.9f; 
        //public float SlowSpeed = 3f;
        //public float SlowThreshold = 0.5f;
        public Vector2 MaxVelocity = new Vector2(0.2f, 0.2f);
        [Header("Jumping")]
        public float JumpSpeed = 4f;
        public string JumpButton = "Fire3";
        public float MaxJump = 150;
        public bool IsAirMovement;
        public float GroundCheckDistance = 32;
        float JumpBegin;
        bool IsJumping;
        public LayerMask GroundLayer;

        void OnDrawGizmos()
        {
            float PosX = transform.position.x;
            float PosY = transform.position.y;
            float PosZ = transform.position.z;
            Gizmos.color = Color.green;
            if (IsJumping)
                Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(PosX, JumpBegin, PosZ), new Vector3(PosX, GetMaxJump(), PosZ));
            Gizmos.color = Color.green;
            if (IsOnGround())
                Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(PosX, PosY, PosZ), new Vector3(PosX, PosY- GroundCheckDistance * transform.lossyScale.y, PosZ));
        }
        float GetMaxJump()
        {
            return JumpBegin + MaxJump * transform.lossyScale.y;
        }
        // Use this for initialization
        void Start ()
        {
            MyRigid = gameObject.GetComponent<Rigidbody2D>();
        }
	    void AnimateInDirection(bool IsRight)
        {
            if (IsRight)
            {
               transform.localScale = new Vector3(0.25f, transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-0.25f, transform.localScale.y, transform.localScale.z);
            }
        }
        public bool IsOnGround()
        {
            float CheckDistance = GroundCheckDistance * transform.lossyScale.y;
            if (Physics2D.Raycast(transform.position, -transform.up, CheckDistance, GroundLayer))
            {
                return true;
            }
            return false;
        }
        float LastFired;
        // Update is called once per frame
        void Update ()
        {
            if (Time.time - LastFired >= 0.1f && Input.GetButton("Fire1"))
            {
                LastFired = Time.time;
                GameObject MyBullet = (GameObject) Instantiate(MyBulletPrefab, transform.position, Quaternion.identity);
                MyBullet.transform.SetParent(transform.parent, false);
                if (transform.localScale.x > 0)
                {
                    MyBullet.transform.position = transform.position + (new Vector3(transform.lossyScale.x*120, 0, 0));
                    MyBullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(20,0));
                }
                else
                {
                    MyBullet.transform.position = transform.position - (new Vector3(-transform.lossyScale.x * 120, 0, 0));
                    MyBullet.GetComponent<Rigidbody2D>().AddForce(-new Vector2(20, 0));
                    //MyBullet.transform.LookAt(MyBullet.transform.position - transform.right);
                    MyBullet.transform.localScale = new Vector3(-MyBullet.transform.localScale.x, 
                        MyBullet.transform.localScale.y, 
                        MyBullet.transform.localScale.z);
                }
                Destroy(MyBullet, 5f);
            }
            float MyAxis = Input.GetAxis("Horizontal");
            bool Onground = IsOnGround();
            if (Onground || IsAirMovement)
            { 
                if (MyAxis > 0)
                {
                    //Debug.Log("Moving right.");
                    if (MyRigid.velocity.x < MaxVelocity.x * transform.lossyScale.x)
                        MyRigid.AddForce(new Vector2(100*MovementSpeed*Time.deltaTime, 0));
                    AnimateInDirection(true);
                }
                else if (MyAxis < 0)
                {
                    //Debug.Log("Moving left.");
                    if (MyRigid.velocity.x > -MaxVelocity.x * (-transform.lossyScale.x))
                        MyRigid.AddForce(new Vector2(-100 * MovementSpeed * Time.deltaTime, 0));
                    AnimateInDirection(false);
                }
                else
                {   // slow down 
                    MyRigid.velocity = new Vector2(MyRigid.velocity.x * SlowDownRate, MyRigid.velocity.y);
                }
            }
            if (!IsJumping && (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown(JumpButton)) && Onground)
            {
                JumpBegin = transform.position.y;
                IsJumping = true;
                Debug.Log("Jumping");
            }
            if (IsJumping)
            {
                if (MyRigid.velocity.y < MaxVelocity.y * transform.lossyScale.y)
                    MyRigid.AddForce(new Vector2(0, 100 * JumpSpeed * Time.deltaTime));
                if (Input.GetButtonUp(JumpButton) || Input.GetKeyUp(KeyCode.Space) || transform.position.y >= GetMaxJump())
                {
                    MyRigid.velocity = new Vector2(MyRigid.velocity.x, MyRigid.velocity.y*0.5f);
                    IsJumping = false;
                }
            }
	    }
        void ClampVel()
        {
            // clamp x velocity
            MyRigid.velocity = new Vector2(
                         Mathf.Clamp(
                         MyRigid.velocity.x, 
                         -MaxVelocity.x * transform.lossyScale.x, 
                         MaxVelocity.x * transform.lossyScale.x),
                     Mathf.Clamp(
                         MyRigid.velocity.y,
                         -MaxVelocity.y * transform.lossyScale.y,
                         MaxVelocity.y * transform.lossyScale.y));
        }
    }
}