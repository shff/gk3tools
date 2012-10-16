using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;

namespace Viewer
{
    public struct ColorXml
    {
        private float _red, _green, _blue;

        public ColorXml(float r, float g, float b)
        {
            _red = r;
            _green = g;
            _blue = b;
        }

        [XmlAttribute("red")]
        public float Red
        {
            get { return _red; }
            set { _red = value; }
        }

        [XmlAttribute("green")]
        public float Green
        {
            get { return _green; }
            set { _green = value; }
        }

        [XmlAttribute("blue")]
        public float Blue
        {
            get { return _blue; }
            set { _blue = value; }
        }
    }

    public struct VectorXml
    {
        private float _x, _y, _z;

        public VectorXml(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        [XmlAttribute("x")]
        public float X
        {
            get { return _x; }
            set { _x = value; }
        }

        [XmlAttribute("y")]
        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }

        [XmlAttribute("z")]
        public float Z
        {
            get { return _z; }
            set { _z = value; }
        }
    }

    [XmlRoot("surface")]
    public class SurfaceXml
    {
        private int _index;
        private int _width;
        private int _height;
        private bool _visible = true;

        public SurfaceXml()
        {
        }

        public SurfaceXml(int index, int width, int height)
        {
            _index = index;
            _width = width;
            _height = height;
        }

        [XmlAttribute("index")]
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        [XmlAttribute("width")]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [XmlAttribute("height")]
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        [XmlAttribute("visible")]
        [DefaultValue(true)]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
    }

    public class SkylightXml
    {
        private ColorXml _ambientColor;
        private VectorXml _sunDirection;
        private ColorXml _sunColor;

        public SkylightXml()
        {
        }

        public SkylightXml(ColorXml ambientColor)
        {
            _ambientColor = ambientColor;
        }

        [XmlElement("ambientColor")]
        public ColorXml AmbientColor
        {
            get { return _ambientColor; }
            set { _ambientColor = value; }
        }

        [XmlElement("sunDirection")]
        public VectorXml SunDirection
        {
            get { return _sunDirection; }
            set { _sunDirection = value; }
        }

        [XmlElement("sunColor")]
        public ColorXml SunColor
        {
            get { return _sunColor; }
            set { _sunColor = value; }
        }
    }

    public class OmniXml
    {
        private ColorXml _color;
        private VectorXml _position;
        private float _radius;

        [XmlAttribute("radius")]
        public float Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        [XmlElement("color")]
        public ColorXml Color
        {
            get { return _color; }
            set { _color = value; }
        }

        [XmlElement("position")]
        public VectorXml Position
        {
            get { return _position; }
            set { _position = value; }
        }
    }

    [XmlRoot("lighting")]
    public class LightingXml
    {
        private float _exposure;
        private List<SurfaceXml> _surfaces = new List<SurfaceXml>();
        private SkylightXml _skylight;
        private List<OmniXml> _omnis = new List<OmniXml>();

        [XmlAttribute("exposure")]
        public float Exposure
        {
            get { return _exposure; }
            set { _exposure = value; }
        }

        [XmlArray("surfaces")]
        [XmlArrayItem("surface")]
        public List<SurfaceXml> Surfaces
        {
            get { return _surfaces; }
        }

        [XmlElement("skylight")]
        public SkylightXml Skylight
        {
            get { return _skylight; }
            set { _skylight = value; }
        }

        [XmlArray("omnis")]
        [XmlArrayItem("omni")]
        public List<OmniXml> Omnis
        {
            get { return _omnis; }
        }

        public void Write(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LightingXml));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, this);
            writer.Close();
        }
        
        public static LightingXml Load(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LightingXml));
            TextReader reader = new StreamReader(filename);
            LightingXml lighting = (LightingXml)serializer.Deserialize(reader);
            reader.Close();

            return lighting;
        }

        public static Gk3Main.Game.LightmapSpecs GenerateSpecs(LightingXml lighting)
        {
            Gk3Main.Game.LightmapSpecs specs = new Gk3Main.Game.LightmapSpecs();

            Gk3Main.Math.Vector3 skyColor;
            skyColor.X = lighting.Skylight.AmbientColor.Red;
            skyColor.Y = lighting.Skylight.AmbientColor.Green;
            skyColor.Z = lighting.Skylight.AmbientColor.Blue;

            specs.SkyColor = skyColor;

            Gk3Main.Math.Vector3 sunColor;
            sunColor.X = lighting.Skylight.SunColor.Red;
            sunColor.Y = lighting.Skylight.SunColor.Green;
            sunColor.Z = lighting.Skylight.SunColor.Blue;
            specs.SunColor = sunColor;
            specs.SunDirection = new Gk3Main.Math.Vector3(lighting.Skylight.SunDirection.X, lighting.Skylight.SunDirection.Y, lighting.Skylight.SunDirection.Z);

            foreach (OmniXml omni in lighting.Omnis)
            {
                Gk3Main.Game.LightmapSpecs.OmniLight o = new Gk3Main.Game.LightmapSpecs.OmniLight();
                o.Radius = omni.Radius;
                o.Position = new Gk3Main.Math.Vector3(omni.Position.X, omni.Position.Y, omni.Position.Z);
                o.Color = new Gk3Main.Math.Vector3(omni.Color.Red, omni.Color.Green, omni.Color.Blue);
                specs.OmniLights.Add(o);
            }

            // TODO: this ignores the index and assumes they'll be ordered in the spec file!
            foreach (SurfaceXml surface in lighting.Surfaces)
            {
                Gk3Main.Game.LightmapSpecs.SurfaceLightmap lm = new Gk3Main.Game.LightmapSpecs.SurfaceLightmap();
                lm.Width = surface.Width;
                lm.Height = surface.Height;
                specs.Surfaces.Add(lm);
            }

            return specs;
        }
    }
}
