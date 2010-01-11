using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class ActorFace
    {
        private string _actorCode;
        private FaceDefinition _faceDefinition;
        private Graphics.TextureResource _baseFace;
        private Graphics.RenderTarget _renderTarget;
        private Graphics.TextureResource _generatedFace;
        private Graphics.TextureResource[] _mouths;
        private Graphics.TextureResource[] _smiles;
        private Graphics.TextureResource _currentMouth;
        private bool _faceIsDirty;

        public ActorFace(string actorCode)
        {
            _actorCode = actorCode;
            _faceDefinition = FaceDefinitions.GetFaceDefinition(actorCode);

            _baseFace = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_FACE.BMP");

            _mouths = new Graphics.TextureResource[8];
            _mouths[0] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH00.BMP");
            _mouths[1] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH01.BMP");
            _mouths[2] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH02.BMP");
            _mouths[3] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH03.BMP");
            _mouths[4] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH04.BMP");
            _mouths[5] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH05.BMP");
            _mouths[6] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH06.BMP");
            _mouths[7] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_MOUTH07.BMP");

            _smiles = new Graphics.TextureResource[2];
            _smiles[0] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_SMILE_01.BMP");
            _smiles[1] = (Graphics.TextureResource)Resource.ResourceManager.Load(actorCode + "_SMILE_02.BMP");

            if (Graphics.RendererManager.CurrentRenderer.RenderToTextureSupported)
            {
                _renderTarget = Graphics.RendererManager.CurrentRenderer.CreateRenderTarget(_baseFace.Width, _baseFace.Height);
                _generatedFace = _renderTarget.Texture;
            }
            else
                _generatedFace = Graphics.RendererManager.CurrentRenderer.CreateUpdatableTexture(actorCode + "_FACE", _baseFace.Width, _baseFace.Height);

            updateTexture(false);
        }

        public void SetMouth(string mouth)
        {
            const int indexOfNumber = 5;

            int mouthNum;
            Utils.TryParseInt(mouth, indexOfNumber, 2, out mouthNum);

            _currentMouth = _mouths[mouthNum];

            _faceIsDirty = true;
        }

        public void RebuildTexture()
        {
            updateTexture(true);

            _faceIsDirty = false;
        }

        public Graphics.TextureResource Texture
        {
            get 
            {
                return _generatedFace;
            }
        }

        public bool FaceIsDirty { get { return _faceIsDirty; } }

        private void updateTexture(bool modified)
        {
            if (Graphics.RendererManager.CurrentRenderer.RenderToTextureSupported)
            {
                // set the render target
                Graphics.RendererManager.CurrentRenderer.SetRenderTarget(_renderTarget);

                // TODO: render all the faces onto the texture

                // undo the render target and get the texture
                Graphics.RendererManager.CurrentRenderer.SetRenderTarget(null);
            }
            else
            {
                // guess we have to do this the hard, ugly way :(
                byte[] facePixels = (byte[])_baseFace.Pixels.Clone();

                if (modified)
                {
                    // blit the mouth
                    blit((int)_faceDefinition.MouthOffset.X, (int)_faceDefinition.MouthOffset.Y,
                        _currentMouth.Pixels, _currentMouth.Width, _currentMouth.Height,
                        facePixels, _baseFace.Width, _baseFace.Height);

                    // TODO: blit the rest
                }

                // generate a new texture
                Graphics.UpdatableTexture t = (Graphics.UpdatableTexture)_generatedFace;
                t.Update(facePixels);
            }
        }

        private void blit(int destX, int destY, byte[] source, int sourceWidth, int sourceHeight, 
            byte[] destination, int destinationWidth, int destinationHeight)
        {
            for (int y = 0; y < sourceHeight; y++)
            {
                int destinationIndex = ((destY + y) * destinationWidth + destX) * 4;
                Array.Copy(source, y * sourceWidth * 4, destination,
                    destinationIndex, sourceWidth * 4);
            }
        }
    }

    public class Actor : IDisposable
    {
        private string _noun;
        private string _modelName;
        private Math.Vector3 _position;
        private float _facingAngle;
        private Graphics.ModelResource _model;
        private bool _isEgo;
        private ActorFace _face;

        public Actor(string modelName, string noun, bool isEgo)
        {
            string actorName = Utils.GetFilenameWithoutExtension(modelName);
            _face = new ActorFace(actorName);
            _noun = noun;
            _modelName = modelName;
            _isEgo = isEgo;

            _model = (Graphics.ModelResource)Resource.ResourceManager.Load(Utils.MakeEndsWith(modelName, ".MOD"));


            if (_model.Meshes != null)
            {
                // find the face section
                string faceSectionName = actorName + "_FACE";
                for (int i = 0; i < _model.Meshes.Length; i++)
                {
                    for (int j = 0; j < _model.Meshes[i].sections.Length; j++)
                    {
                        if (_model.Meshes[i].sections[j].texture.Equals(faceSectionName, StringComparison.OrdinalIgnoreCase))
                        {
                            _model.Meshes[i].sections[j].textureResource = _face.Texture;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_model != null)
            {
                Resource.ResourceManager.Unload(_model);
                _model = null;
            }
        }

        public void Render(Graphics.Camera camera)
        {
            if (_model != null)
            {
                _model.RenderAt(_position, _facingAngle, camera);
            }
        }

        public void SetMouth(string mouth)
        {
            _face.SetMouth(mouth);
            _face.RebuildTexture();
        }

        public bool CollideRay(Math.Vector3 origin, Math.Vector3 direction, float length, out float distance)
        {
            if (_model != null && _model.Loaded)
                return _model.CollideRay(_position, origin, direction, length, out distance);

            distance = float.MaxValue;
            return false;
        }

        public string ModelName
        {
            get { return _modelName; }
        }

        public string Noun
        {
            get { return _noun; }
        }

        public Math.Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float FacingAngle
        {
            get { return _facingAngle; }
            set { _facingAngle = value; }
        }

        public bool IsEgo
        {
            get { return _isEgo; }
        }

        public Graphics.ModelResource Model
        {
            get { return _model; }
        }
    }
}
