using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class LaserSceneCustomizer : ISceneCustomizer
    {
        private static float[][] _headPositions = new float[][]{
		     new float[] {77.026f, 51.944f, 124.426f},
		     new float[] {127.718f, 51.944f, -19.289f},
		     new float[] {5.607f, 51.944f, -110.376f},
		     new float[] {-118.223f, 51.944f, -22.235f},
		     new float[] {-72.012f, 51.944f, 121.967f},
        };

        private static float[,] _headRotations = new float[5,5];

        static LaserSceneCustomizer()
        {
            // calculate the center point
            Math.Vector3 center = Math.Vector3.Zero;
            for (int i = 0; i < 5; i++)
            {
                center.X += _headPositions[i][0];
                center.Y += _headPositions[i][1];
                center.Z += _headPositions[i][2];
            }

            center /= 5.0f;

            // now calculate the different rotations of the heads
            for (int currentHead = 0; currentHead < 5; currentHead++)
            {
                for (int currentRotation = 0; currentRotation < 5; currentRotation++)
                {
                    int targetHead;
                    Math.Vector3 target = Math.Vector3.Zero;
                    Math.Vector3 headPosition;

                    headPosition.X = _headPositions[currentHead][0];
                    headPosition.Y = _headPositions[currentHead][1];
                    headPosition.Z = _headPositions[currentHead][2];

                    if (currentRotation == 0)
                    {
                        targetHead = (currentHead + 1) % 5;
                        target.X = _headPositions[targetHead][0];
                        target.Y = _headPositions[targetHead][1];
                        target.Z = _headPositions[targetHead][2];
                    }
                    else if (currentRotation == 1)
                    {
                        targetHead = (currentHead + 2) % 5;
                        target.X = _headPositions[targetHead][0];
                        target.Y = _headPositions[targetHead][1];
                        target.Z = _headPositions[targetHead][2];
                    }
                    else if (currentRotation == 2)
                    {
                        // this is the center, so it's special
                        target = center;
                    }
                    else if (currentRotation == 3)
                    {
                        targetHead = (currentHead + 3) % 5;
                        target.X = _headPositions[targetHead][0];
                        target.Y = _headPositions[targetHead][1];
                        target.Z = _headPositions[targetHead][2];
                    }
                    else if (currentRotation == 4)
                    {
                        targetHead = (currentHead + 4) % 5;
                        target.X = _headPositions[targetHead][0];
                        target.Y = _headPositions[targetHead][1];
                        target.Z = _headPositions[targetHead][2];
                    }

                    Math.Vector3 toTarget = target - headPosition;
                    _headRotations[currentHead, currentRotation] = (float)System.Math.Atan2(toTarget.Z, toTarget.X) - Math.Constants.Pi * 0.5f;
                }
            }
        }

        public void OnLoad()
        {
            // position the head models
            Gk3Main.Graphics.ModelResource[] heads = new Gk3Main.Graphics.ModelResource[] {
                SceneManager.GetModelByName("CS2HEAD01", false),
                SceneManager.GetModelByName("CS2HEAD02", false),
                SceneManager.GetModelByName("CS2HEAD03", false),
                SceneManager.GetModelByName("CS2HEAD04", false),
                SceneManager.GetModelByName("CS2HEAD05", false)
            };

            // rotate and position each head
            for (int i = 0; i < 5; i++)
            {
                heads[i].TempTransform = Math.Matrix.RotateY(_headRotations[i,2]) 
                   * Math.Matrix.Translate(_headPositions[i][0], _headPositions[i][1], _headPositions[i][2]);
            }
        }
        
        public void OnCustomFunction(string name)
        {
            if (name.StartsWith("turn", StringComparison.OrdinalIgnoreCase))
            {
                
            }
        }
    }
}
