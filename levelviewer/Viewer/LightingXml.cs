﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

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

    [XmlRoot("lighting")]
    public class LightingXml
    {
        private float _exposure;
        private List<SurfaceXml> _surfaces = new List<SurfaceXml>();
        private SkylightXml _skylight;

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
            specs.SunDirection = new Gk3Main.Math.Vector3(lighting.Skylight.SunDirection.X, lighting.Skylight.SunDirection.Y, lighting.Skylight.SunDirection.Z);

            return specs;
        }
    }
}
