﻿using DragonBonesMG.Animation;
using DragonBonesMG.JsonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace DragonBonesMG.Display
{
    public class DbMesh : DbDisplay
    {
        private readonly IDrawableDb _drawable;
        private Texture2D _texture;

        private readonly float[] _originalVertices;
        private readonly short[] _indices;
        private readonly float[] _uvs;
        private VertexPositionColorTexture[] _vertices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private BasicEffect _effect;

        private Matrix _cameraMatrix;

        internal DbMesh(DisplayData data, ITextureSupplier texturer, GraphicsDevice graphics)
            : base(data.Name)
        {
            _drawable = texturer.Get(data.Name);
            _originalVertices = data.Vertices;
            // reverse to go with MonoGames standard culling direction
            // TODO ideally this would happen at content build time along with any other computation
            _indices = data.Triangles.Reverse().ToArray();
            _uvs = data.Uvs;
            _vertices = new VertexPositionColorTexture[_originalVertices.Length / 2];
            // edges, userEdges?
            Initialize(graphics);
        }

        private void Initialize(GraphicsDevice graphicsDevice)
        {
            var s = new SpriteBatch(graphicsDevice);
            var vp = graphicsDevice.Viewport;
            _cameraMatrix = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);

            _texture = _drawable.RenderToTexture(s);

            // TODO these are all structs, so efficiency can be improved by working with pointers!

            var originalVerticesAsSpan = _originalVertices.AsSpan();
            var verticesAsSpan = _vertices.AsSpan();
            var uvsAsSpan = _uvs.AsSpan();

            for (int i = 0; i < originalVerticesAsSpan.Length; i += 2)
            {
                verticesAsSpan[i / 2] = new VertexPositionColorTexture(
                    new Vector3(originalVerticesAsSpan[i], originalVerticesAsSpan[i + 1], 0f),
                    Color.White,
                    new Vector2(uvsAsSpan[i], uvsAsSpan[i + 1]));
            }

            _indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
            _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);

            _effect = new BasicEffect(s.GraphicsDevice)
            {
                World = Matrix.Identity,
                View = Matrix.Identity,
                Texture = _texture,
                VertexColorEnabled = true,
                TextureEnabled = true,
            };
        }

        /// <summary>
        /// Update this mesh.
        /// </summary>
        /// <param name="state"></param>
        public void Update(MeshTimeline state)
        {
            if (state == null) return;

            var originalVerticesAsSpan = _originalVertices.AsSpan();
            var verticesAsSpan = _vertices.AsSpan();
            var stateVerticesAsSpan = state.Vertices.AsSpan();

            var offset = state.Vertices.Any() ? state.Offset : originalVerticesAsSpan.Length;

            for (int i = 0; i < offset; i += 2)
            {
                verticesAsSpan[i / 2].Position = new Vector3(originalVerticesAsSpan[i], originalVerticesAsSpan[i + 1], 0f);
            }

            for (int i = offset; i < originalVerticesAsSpan.Length; i += 2)
            {
                verticesAsSpan[i / 2].Position = new Vector3(
                    originalVerticesAsSpan[i] + stateVerticesAsSpan[i - offset],
                    originalVerticesAsSpan[i + 1] + stateVerticesAsSpan[i - offset + 1], 0);
            }
        }

        /// <summary>
        /// Draw this mesh.
        /// </summary>
        /// <param name="s">A spritebatch.</param>
        /// <param name="transform"> A transformation matrix.</param>
        /// <param name="colorTransform">A color</param>
        public override void Draw(SpriteBatch s, Matrix transform, Color colorTransform)
        {
            // TODO use the color, probably best to write a shader for this, so we can also handle negative scale/culling there

            bool scaleXPositive = transform.M11 >= 0;
            bool scaleYPositive = transform.M22 >= 0;

            bool reverseCull = !scaleXPositive || !scaleYPositive;

            RasterizerState rasterizerState = s.GraphicsDevice.RasterizerState;
            if (reverseCull) s.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullClockwiseFace };

            _effect.Projection = transform * _cameraMatrix;
            _indexBuffer.SetData(_indices);
            _vertexBuffer.SetData(_vertices);
            s.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            s.GraphicsDevice.Indices = _indexBuffer;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                s.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3);
            }
            s.GraphicsDevice.RasterizerState = rasterizerState;
        }
    }
}