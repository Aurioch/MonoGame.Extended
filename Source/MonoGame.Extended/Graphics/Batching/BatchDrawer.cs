﻿using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Graphics.Batching
{
    internal sealed class BatchDrawer<TVertexType, TBatchItemData, TEffect> : IDisposable
        where TVertexType : struct, IVertexType where TBatchItemData : struct, IBatchItemData<TBatchItemData, TEffect> where TEffect : Effect
    {
        internal GraphicsDevice GraphicsDevice;
        internal readonly ushort MaximumVerticesCount;
        internal readonly ushort MaximumIndicesCount;

        internal DynamicVertexBuffer VertexBuffer;
        internal DynamicIndexBuffer IndexBuffer;
        internal TEffect Effect;

        internal BatchDrawer(GraphicsDevice graphicsDevice, ushort maximumVerticesCount = PrimitiveBatch<TVertexType, TBatchItemData, TEffect>.DefaultMaximumVerticesCount, ushort maximumIndicesCount = PrimitiveBatch<TVertexType, TBatchItemData, TEffect>.DefaultMaximumIndicesCount)
        {
            GraphicsDevice = graphicsDevice;
            MaximumVerticesCount = maximumVerticesCount;
            MaximumIndicesCount = maximumIndicesCount;

            VertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(TVertexType), maximumVerticesCount, BufferUsage.WriteOnly);
            IndexBuffer = new DynamicIndexBuffer(graphicsDevice, typeof(short), maximumIndicesCount, BufferUsage.WriteOnly);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (!isDisposing)
            {
                return;
            }

            GraphicsDevice = null;

            VertexBuffer?.Dispose();
            VertexBuffer = null;

            IndexBuffer?.Dispose();
            IndexBuffer = null;
        }

        internal void Select(TVertexType[] vertices, int startVertex, int vertexCount)
        {
            VertexBuffer.SetData(vertices, startVertex, vertexCount);
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
        }

        internal void Select(TVertexType[] vertices, int startVertex, int vertexCount, int[] indices, int startIndex, int indexCount)
        {
            VertexBuffer.SetData(vertices, startVertex, vertexCount);
            IndexBuffer.SetData(indices, startIndex, indexCount);
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
        }

        internal void Draw(ref TBatchItemData batchItemData, PrimitiveType primitiveType, int startVertex, int vertexCount)
        {
            var primitiveCount = primitiveType.GetPrimitiveCount(vertexCount);

            batchItemData.ApplyTo(Effect);

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(primitiveType, startVertex, primitiveCount);
            }
        }

        internal void Draw(ref TBatchItemData batchItemData, PrimitiveType primitiveType, int startVertex, int vertexCount, int startIndex, int indexCount)
        {
            var primitiveCount = primitiveType.GetPrimitiveCount(indexCount);

            batchItemData.ApplyTo(Effect);

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(primitiveType, startVertex, startIndex, primitiveCount);
            }
        }
    }
}
