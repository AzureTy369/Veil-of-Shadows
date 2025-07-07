using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] //Tạo một đối tượng playerData mới bằng cách nhấp chuột phải trong Project Menu rồi chọn Create/Player/Player Data và kéo vào player
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength; //Lực hướng xuống (trọng lực) cần thiết cho jumpHeight và jumpTimeToApex mong muốn.
    [HideInInspector] public float gravityScale;    //Cường độ trọng lực của người chơi dưới dạng hệ số nhân của trọng lực (được đặt trong ProjectSettings/Physics2D).
                                                    //Cũng là giá trị được đặt cho rigidbody2D.gravityScale của người chơi.
    [Space(5)]
    public float fallGravityMult;                   //Hệ số nhân áp dụng cho gravityScale của người chơi khi rơi.
    public float maxFallSpeed;                      //Tốc độ rơi tối đa (vận tốc giới hạn) của người chơi khi rơi.
    [Space(5)]
    public float fastFallGravityMult;               //Hệ số nhân lớn hơn áp dụng cho gravityScale của người chơi khi họ đang rơi và nhấn input đi xuống.
                                                    //Thấy trong các game như Celeste, cho phép người chơi rơi cực nhanh nếu họ muốn.
    public float maxFastFallSpeed;                  //Tốc độ rơi tối đa (vận tốc giới hạn) của người chơi khi thực hiện rơi nhanh hơn.
    
    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed;                       //Tốc độ mục tiêu mà chúng ta muốn người chơi đạt được.
    public float runAcceleration;                   //Tốc độ mà người chơi tăng tốc lên đến tốc độ tối đa, có thể đặt bằng runMaxSpeed để tăng tốc tức thì hoặc 0 để không có tăng tốc
    [HideInInspector] public float runAccelAmount;  //Lực thực tế (nhân với speedDiff) được áp dụng lên người chơi.
    public float runDecceleration;                  //Tốc độ mà người chơi giảm tốc từ tốc độ hiện tại, có thể đặt bằng runMaxSpeed để giảm tốc tức thì hoặc 0 để không có giảm tốc
    [HideInInspector] public float runDeccelAmount; //Lực thực tế (nhân với speedDiff) được áp dụng lên người chơi.
    [Space(5)]
    [Range(0f, 1)] public float accelInAir;         //Hệ số nhân áp dụng cho tốc độ tăng tốc khi ở trên không.
    [Range(0f, 1)] public float deccelInAir;        //Hệ số nhân áp dụng cho tốc độ giảm tốc khi ở trên không.
    [Space(5)]
    public bool doConserveMomentum = true;          //Bảo toàn đà của người chơi khi họ đang di chuyển nhanh hơn tốc độ tối đa.

    [Space(20)]

    [Header("Jump")]
    public float jumpHeight;                        //Độ cao nhảy của người chơi
    public float jumpTimeToApex;                    //Thời gian giữa việc áp dụng lực nhảy và đạt đến độ cao nhảy mong muốn. Các giá trị này cũng điều khiển trọng lực và lực nhảy của người chơi.
    [HideInInspector] public float jumpForce;       //Lực thực tế (hướng lên) được áp dụng lên người chơi khi họ nhảy.

    [Header("Both Jumps")]
    public float jumpCutGravityMult;                //Hệ số nhân để tăng trọng lực nếu người chơi thả nút nhảy trong khi vẫn đang nhảy
    [Range(0f, 1)] public float jumpHangGravityMult; //Giảm trọng lực khi gần đỉnh điểm (độ cao tối đa mong muốn) của cú nhảy
    public float jumpHangTimeThreshold;             //Tốc độ (gần bằng 0) mà người chơi sẽ trải nghiệm thêm "lơ lửng khi nhảy". Velocity.y của người chơi gần bằng 0 nhất ở đỉnh của cú nhảy (tương tự như gradient của đồ thị parabol hoặc hàm bậc hai)
    [Space(0.5f)]
    public float jumpHangAccelerationMult;          //Hệ số nhân gia tốc khi ở trạng thái "lơ lửng khi nhảy"
    public float jumpHangMaxSpeedMult;              //Hệ số nhân tốc độ tối đa khi ở trạng thái "lơ lửng khi nhảy"

    [Header("Wall Jump")]
    public Vector2 wallJumpForce;                   //Lực thực tế (lần này do chúng ta đặt) được áp dụng lên người chơi khi nhảy tường.
    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp;   //Giảm ảnh hưởng của chuyển động người chơi khi đang nhảy tường.
    [Range(0f, 1.5f)] public float wallJumpTime;    //Thời gian mà chuyển động của người chơi bị làm chậm sau khi nhảy tường.
    public bool doTurnOnWallJump;                   //Người chơi sẽ xoay để đối mặt với hướng nhảy tường

    [Space(20)]

    [Header("Slide")]
    public float slideSpeed;                        //Tốc độ mục tiêu khi trượt xuống tường
    public float slideAccel;                        //Gia tốc khi trượt xuống tường

    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime;   //Thời gian ân hạn sau khi rơi khỏi bục, nơi bạn vẫn có thể nhảy
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime; //Thời gian đệm sau khi nhấn nhảy, khi đó một cú nhảy sẽ được tự động thực hiện ngay khi các yêu cầu (ví dụ: chạm đất) được đáp ứng.

    [Space(20)]

    [Header("Dash")]
    public int dashAmount;                          //Số lần lướt người chơi có thể thực hiện
    public float dashSpeed;                         //Tốc độ của lướt
    public float dashSleepTime;                     //Thời gian mà trò chơi đóng băng khi chúng ta nhấn lướt nhưng trước khi chúng ta đọc input hướng và áp dụng lực
    [Space(5)]
    public float dashAttackTime;                    //Thời gian của giai đoạn "tấn công" của lướt khi người chơi di chuyển với tốc độ dashSpeed
    [Space(5)]
    public float dashEndTime;                       //Thời gian sau khi bạn kết thúc giai đoạn kéo ban đầu, làm mượt quá trình chuyển tiếp trở lại trạng thái nghỉ (hoặc bất kỳ trạng thái tiêu chuẩn nào)
    public Vector2 dashEndSpeed;                    //Làm chậm người chơi, khiến lướt cảm thấy phản hồi hơn (được sử dụng trong Celeste)
    [Range(0f, 1f)] public float dashEndRunLerp;    //Làm chậm ảnh hưởng của chuyển động người chơi khi đang lướt
    [Space(5)]
    public float dashRefillTime;                    //Thời gian trước khi lướt được nạp lại khi ở trên mặt đất
    [Space(5)]
    [Range(0.01f, 0.5f)] public float dashInputBufferTime; //Thời gian đệm cho đầu vào lướt
    

    //Unity Callback, được gọi khi inspector cập nhật
    private void OnValidate()
    {
        //Tính toán lực hấp dẫn bằng công thức (gravity = 2 * jumpHeight / timeToJumpApex^2)
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
        
        //Tính toán tỷ lệ trọng lực của rigidbody (tức là: cường độ trọng lực tương đối so với giá trị trọng lực của unity, xem project settings/Physics2D)
        gravityScale = gravityStrength / Physics2D.gravity.y;

        //Tính toán lực tăng và giảm tốc chạy bằng công thức: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        //Tính toán lực nhảy bằng công thức (initialJumpVelocity = gravity * timeToJumpApex)
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}
