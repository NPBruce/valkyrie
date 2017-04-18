﻿// This code is originally from UnityStudio, adapted here to suit Valkyrie

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class Transform
    {
        public PPtr m_GameObject = new PPtr();
        public float[] m_LocalRotation;
        public float[] m_LocalPosition;
        public float[] m_LocalScale;
        public List<PPtr> m_Children = new List<PPtr>();
        public PPtr m_Father = new PPtr();//can be transform or type 224 (as seen in Minions)

        public Transform(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            if (sourceFile.platform == -2)
            {
                a_Stream.ReadUInt32();
                sourceFile.ReadPPtr();
                sourceFile.ReadPPtr();
            }

            m_GameObject = sourceFile.ReadPPtr();
            m_LocalRotation = new float[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
            m_LocalPosition = new float[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
            m_LocalScale = new float[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
            int m_ChildrenCount = a_Stream.ReadInt32();
            for (int j = 0; j < m_ChildrenCount; j++)
            {
                m_Children.Add(sourceFile.ReadPPtr());
            }
            m_Father = sourceFile.ReadPPtr();
        }
    }
}
