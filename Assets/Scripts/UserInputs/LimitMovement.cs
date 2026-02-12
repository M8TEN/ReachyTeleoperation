using System;

public class LimitMovement
{
    public const int LEFT_SIDE = 0;
    public const int RIGHT_SIDE = 1;
    private readonly double[] LEFT_VOLUME_LIMITS =
    {
        0.11811043229753332,  //x-min
        0.6335332352560936,   //x-max
        0.13514077968129315,  //y-min
        0.6592141561146554,   //y-max
        -0.34492036682391924, //z-min
        0.6003516143933125    //z-max
    };

    private readonly double[] RIGHT_VOLUME_LIMITS =
    {
        0.20231462989210047,  //x-min
        0.6397911697167149,   //x-max
        -0.6977623753471274,  //y-min
        -0.09710021007469337, //y-max
        -0.34623931460098595, //z-min
        0.5954658554344094    //z-max
    };

    private double[] last_position;
    private int side;
    private double[] limits;
    private bool first_valid_pose_reached = false;

    public LimitMovement(int arm_side)
    {
        side = arm_side;
        if (side == LEFT_SIDE)
        {
            limits  = new double[] {
                0.2970021372738335,    //x-min
                0.5150536650457043,    //x-max
                0.04474292855345502,   //y-min
                0.4065434369531554,    //y-max
                -0.412436990943232444, //z-min
                -0.022780532069723743  //z-max
            };
            last_position = new double[] {
                limits[1] - limits[0],
                limits[3] - limits[2],
                limits[5] - limits[4]
            };
        }
        else if (side == RIGHT_SIDE)
        {
            limits = new double[] {
                0.21701274404719764,  //x-min
                0.6186213994503541,   //x-max
                -0.5567124991918473,  //y-min
                -0.13960405374265367, //y-max
                -0.3996426656448221,  //z-min
                0.5467782688288836    //z-max
            };
            last_position = new double[] {
                limits[1] - limits[0],
                limits[3] - limits[2],
                limits[5] - limits[4]
            };
        }
    }
    public void ClampMovement(ref Reachy.Sdk.Kinematics.Matrix4x4 pose)
    {
        double x = pose.Data[3];
        double y = pose.Data[7];
        double z = pose.Data[11];
        bool is_inside_volume = (
            x >= limits[0] && x <= limits[1] &&
            y >= limits[2] && y <= limits[3] &&
            z >= limits[4] && z <= limits[5]
        );

        if (is_inside_volume && !first_valid_pose_reached)
        {
            first_valid_pose_reached = true;
        }
        pose.Data[3] = Math.Min(limits[1], Math.Max(limits[0], x));
        pose.Data[7] = Math.Min(limits[3], Math.Max(limits[2], y));
        pose.Data[11] = Math.Min(limits[5], Math.Max(limits[4], z));
    }
}
