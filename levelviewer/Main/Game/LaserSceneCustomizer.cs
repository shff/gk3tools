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

        private bool _areLasersActive = false;
        private int[] _currentHeadTargets = new int[5];

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

            Gk3Main.Graphics.ModelResource[] lasers = new Gk3Main.Graphics.ModelResource[] {
                SceneManager.GetModelByName("CS2LASER_01", false),
                SceneManager.GetModelByName("CS2LASER_02", false),
                SceneManager.GetModelByName("CS2LASER_03", false),
                SceneManager.GetModelByName("CS2LASER_04", false),
                SceneManager.GetModelByName("CS2LASER_05", false)
            };

            // rotate and position each head
            for (int i = 0; i < 5; i++)
            {
                heads[i].TempTransform = Math.Matrix.RotateY(_headRotations[i,2]) 
                   * Math.Matrix.Translate(_headPositions[i][0], _headPositions[i][1], _headPositions[i][2]);
                lasers[i].TempTransform = Math.Matrix.RotateY(_headRotations[i, 2])
                    * Math.Matrix.Translate(_headPositions[i][0], _headPositions[i][1], _headPositions[i][2]);
            }

            // this is so the Sheep scripts and stuff know the heads are facing the center
            GameManager.SetIntegerGameVariable("CS2HEAD1", 2);
            GameManager.SetIntegerGameVariable("CS2HEAD2", 2);
            GameManager.SetIntegerGameVariable("CS2HEAD3", 2);
            GameManager.SetIntegerGameVariable("CS2HEAD4", 2);
            GameManager.SetIntegerGameVariable("CS2HEAD5", 2);

            _currentHeadTargets[0] = 2;
            _currentHeadTargets[1] = 2;
            _currentHeadTargets[2] = 2;
            _currentHeadTargets[3] = 2;
            _currentHeadTargets[4] = 2;
        }
        
        public void OnCustomFunction(string name)
        {
            if (name.StartsWith("turn", StringComparison.OrdinalIgnoreCase))
            {
                char direction = name[4];
                int head = name[5] - '1';
                int target;

                if (direction == 'l' || direction == 'L')
                {
                    target = (_currentHeadTargets[head] + 1) % 5;
                }
                else
                {
                    target = (_currentHeadTargets[head] + 4) % 5;
                }

                Gk3Main.Graphics.ModelResource laserModel = SceneManager.GetModelByName("CS2LASER_0" + (head + 1), false);
                Gk3Main.Graphics.ModelResource headModel = SceneManager.GetModelByName("CS2HEAD0" + (head + 1), false);
                laserModel.TempTransform = Math.Matrix.RotateY(_headRotations[head, target])
                    * Math.Matrix.Translate(_headPositions[head][0], _headPositions[head][1], _headPositions[head][2]);
                headModel.TempTransform = Math.Matrix.RotateY(_headRotations[head, target])
                     * Math.Matrix.Translate(_headPositions[head][0], _headPositions[head][1], _headPositions[head][2]);

                _currentHeadTargets[head] = target;
                GameManager.SetIntegerGameVariable("CS2HEAD" + (head + 1), target);

                if (isPuzzleComplete())
                {
                    GameManager.SetFlag("STAIRCASEOPEN"); 
                }
            }
            else if (name.Equals("ToggleLasers", StringComparison.OrdinalIgnoreCase))
            {
                _areLasersActive = !_areLasersActive;

                SceneManager.SetSceneModelVisibility("CS2LASER_01", _areLasersActive);
                SceneManager.SetSceneModelVisibility("CS2LASER_02", _areLasersActive);
                SceneManager.SetSceneModelVisibility("CS2LASER_03", _areLasersActive);
                SceneManager.SetSceneModelVisibility("CS2LASER_04", _areLasersActive);
                SceneManager.SetSceneModelVisibility("CS2LASER_05", _areLasersActive);
            }
        }

        private bool isPuzzleComplete()
        {
            int target = _currentHeadTargets[0];
            if (target == 1 || target == 3)
            {
                bool allHeadsMatch = true;
                for (int i = 1; i < 5; i++)
                {
                    if (_currentHeadTargets[i] != target)
                    {
                        allHeadsMatch = false;
                        break;
                    }
                }

                return allHeadsMatch;
            }

            return false;
        }
    }
}
