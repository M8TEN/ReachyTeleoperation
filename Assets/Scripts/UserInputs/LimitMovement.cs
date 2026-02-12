using System;

public class LimitMovement
{
    public const int LEFT_SIDE = 0;
    public const int RIGHT_SIDE = 1;
    private readonly double[] LEFT_VOLUME_LIMITS =
    {
        0.2970021372738335,    //x-min
        0.5150536650457043,    //x-max
        0.04474292855345502,   //y-min
        0.4065434369531554,    //y-max
        -0.412436990943232444, //z-min
        -0.022780532069723743  //z-max
    };

    private readonly double[] RIGHT_VOLUME_LIMITS =
    {
        0.21701274404719764,  //x-min
        0.6186213994503541,   //x-max
        -0.5567124991918473,  //y-min
        -0.13960405374265367, //y-max
        -0.3996426656448221,  //z-min
        0.5467782688288836    //z-max
    };

    private double[] last_position = new double[3];
    private int side;
    private double[] limits;

    public LimitMovement(int arm_side)
    {
        side = arm_side;
        limits = (side == LEFT_SIDE) ? ref LEFT_VOLUME_LIMITS : ref RIGHT_VOLUME_LIMITS;
    }

    public Reachy.Sdk.Kinematics.Matrix4x4 LimitToVolume(Reachy.Sdk.Kinematics.Matrix4x4 pose)
    {
        if (side != LEFT_SIDE && side != RIGHT_SIDE)
        {
            pose.Data[3] = last_position[0];
            pose.Data[7] = last_position[1];
            pose.Data[11] = last_position[2];
            return pose;
        }
        //Matrix coords: X = 3, Y = 7, Z = 11
        double x = pose.Data[3];
        double y = pose.Data[7];
        double z = pose.Data[11];
        bool is_inside_volume = (
            x >= limits[0] && x <= limits[1] &&
            y >= limits[2] && y <= limits[3] &&
            z >= limits[4] && z <= limits[5]
        );

        if (is_inside_volume)
        {
            last_position[0] = pose.Data[3];
            last_position[1] = pose.Data[7];
            last_position[2] = pose.Data[11];
            return pose;
        }
        
        double[] direction =
        {
            x - last_position[0],
            y - last_position[1],
            z - last_position[2]
        };

        double[] start = {x, y, z};
        double smallest_t_value = Double.PositiveInfinity;
        if (direction[0] < 1e-6)
        {
            double current_t = (limits[0] - x) / direction[0];
            if (current_t >= -1e-6)
            {
                smallest_t_value = Math.Min(smallest_t_value, current_t);
            }
        }
        if (direction[0] > -1e-6)
        {
            double current_t = (limits[1] - x) / direction[0];
            if (current_t >= -1e-6)
            {
                smallest_t_value = Math.Min(smallest_t_value, current_t);
            }
        }
        if (direction[1] < 1e-6)
        {
            double current_t = (limits[2] - y) / direction[1];
            if (current_t >= -1e-6)
            {
                smallest_t_value = Math.Min(smallest_t_value, current_t);
            }
        }
        if (direction[1] > -1e-6)
        {
            double current_t = (limits[3] - y) / direction[1];
            if (current_t >= -1e-6)
            {
                smallest_t_value = Math.Min(smallest_t_value, current_t);
            }
        }
        if (direction[2] < 1e-6)
        {
            double current_t = (limits[4] - z) / direction[2];
            if (current_t >= -1e-6)
            {
                smallest_t_value = Math.Min(smallest_t_value, current_t);
            }
        }
        if (direction[2] > -1e-6)
        {
            double current_t = (limits[5] - z) / direction[2];
            if (current_t >= -1e-6)
            {
                smallest_t_value = Math.Min(smallest_t_value, current_t);
            }
        }

        if (Double.IsPositiveInfinity(smallest_t_value)) //No valid t found
        {
            pose.Data[3] = last_position[0];
            pose.Data[7] = last_position[1];
            pose.Data[11] = last_position[2];
            return pose;
        }

        smallest_t_value = Math.Max(0.0d, smallest_t_value);
        if (smallest_t_value > 1.0d)
        {
            last_position[0] = pose.Data[3];
            last_position[1] = pose.Data[7];
            last_position[2] = pose.Data[11];
            return pose;
        }

        double new_x = start[0] + direction[0] * smallest_t_value;
        double new_y = start[1] + direction[1] * smallest_t_value;
        double new_z = start[2] + direction[2] * smallest_t_value;

        last_position[0] = new_x;
        last_position[1] = new_y;
        last_position[2] = new_z;
        pose.Data[3] = new_x;
        pose.Data[7] = new_y;
        pose.Data[11] = new_z;
        return pose;
    }
}
