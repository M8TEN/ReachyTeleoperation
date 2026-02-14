using System;

public class LimitMovement
{
    public const int LEFT_SIDE = 0;
    public const int RIGHT_SIDE = 1;
    private readonly double[] RIGHT_VOLUME_LIMITS =
    {
        0.1d,   //x-min
        0.65d,  //x-max
        -0.65d, //y-min
        -0.1d,  //y-max
        -0.35d, //z-min
        0.65d   //z-max
    };

    private readonly double[] LEFT_VOLUME_LIMITS =
    {
        0.1d,   //x-min
        0.65d,  //x-max
        0.1d,   //y-min
        0.65d,  //y-max
        -0.35d, //z-min
        0.65d   //z-max
    };
    private int side;
    private double[] limits;
    private bool first_valid_pose_reached = false;

    public LimitMovement(int arm_side)
    {
        side = arm_side;
        limits = (side == LEFT_SIDE) ? LEFT_VOLUME_LIMITS : RIGHT_VOLUME_LIMITS;   
    }
    public void ClampMovement(ref Reachy.Sdk.Kinematics.Matrix4x4 pose)
    {
        if (side != LEFT_SIDE && side != RIGHT_SIDE) return;

        double x = pose.Data[3];
        double y = pose.Data[7];
        double z = pose.Data[11];
        bool is_inside_volume = (
            x >= limits[0] && x <= limits[1] &&
            y >= limits[2] && y <= limits[3] &&
            z >= limits[4] && z <= limits[5]
        );
        
        pose.Data[3] = Math.Min(limits[1], Math.Max(limits[0], x));
        pose.Data[7] = Math.Min(limits[3], Math.Max(limits[2], y));
        pose.Data[11] = Math.Min(limits[5], Math.Max(limits[4], z));
    }
}
