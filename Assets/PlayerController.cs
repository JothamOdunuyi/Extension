using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        //PLAYER VAR
        [Header("Sensitivity")]
        [SerializeField] float x_sens;
        [SerializeField] float y_sens;

        [Header("Player Movement")]
        public float speed;
        [SerializeField] float sprint_multiplier;

        private float horz_movement;
        private float vert_movement;
        private float sprint_speed;
        public float defaultSpeed;

        //VAR
        [Header("Player Variables")]
        [SerializeField] float ground_radius;
        [SerializeField] LayerMask ground_mask;
        [SerializeField] Transform trans;
        [SerializeField] Transform cam_trans;
        [SerializeField] Transform ground_check;

        private Vector3 dir;
        private Rigidbody rb;
        private Quaternion cam;

        public bool cursor_locked = true;
        private float maxAngle = 90f;


        void Start()
        {
            defaultSpeed = speed;
            cam = cam_trans.localRotation;
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            //MOVEMENT
            horz_movement = Input.GetAxisRaw("Horizontal");
            vert_movement = Input.GetAxisRaw("Vertical");
            dir = new Vector3(horz_movement, 0, vert_movement).normalized;

            Update_Cursor();
        }

        private void FixedUpdate()
        {
            bool sprinting = Input.GetKey(KeyCode.LeftShift);

            bool isGrounded = Physics.CheckSphere(ground_check.position, ground_radius, ground_mask);
            bool isSprinting = sprinting && vert_movement > 0 && isGrounded;

            sprint_speed = speed;
            if (isSprinting)
            {
                sprint_speed *= sprint_multiplier;
            }

            Vector3 vel = transform.TransformDirection(dir) * sprint_speed * Time.deltaTime;
            vel.y = rb.velocity.y;
            rb.velocity = vel;
        }

        void UpdateY()
        {
            //VERTICAL CAM
            float cam_input = Input.GetAxis("Mouse Y") * y_sens * Time.deltaTime;
            Quaternion cam_movement = Quaternion.AngleAxis(cam_input, -Vector3.right);
            Quaternion cam_rotate = cam_trans.localRotation * cam_movement;

            if (Quaternion.Angle(cam, cam_rotate) < maxAngle)
            {
                cam_trans.localRotation = cam_rotate;
            }
        }

        void UpdateX()
        {
            //HORIZONTAL CAMERA
            float cam_input = Input.GetAxis("Mouse X") * x_sens * Time.deltaTime;
            Quaternion cam_movement = Quaternion.AngleAxis(cam_input, Vector3.up);
            Quaternion cam_rotate = trans.localRotation * cam_movement;
            trans.localRotation = cam_rotate;
        }

        void Update_Cursor()
        {
            //LOCK CURSOR
            if (cursor_locked)
            {
                //CURSOR HIDDEN
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                UpdateY();
                UpdateX();

                //PRESS ESC UNLOCK CURSOR
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursor_locked = false;
                }
            }
            else
            {
                //CURSOR UNHIDES
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                //PRESS ESC AGAIN LOCKS CURSOR
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursor_locked = true;
                }
            }
        }

      /*  void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ground_check.position, ground_radius);
        }*/
    }

}