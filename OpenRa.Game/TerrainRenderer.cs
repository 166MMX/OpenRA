using System;
using System.Collections.Generic;
using System.Text;
using OpenRa.FileFormats;
using BluntDirectX.Direct3D;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenRa.Game
{
	class TerrainRenderer
	{
		FvfVertexBuffer<Vertex> vertexBuffer;
		IndexBuffer indexBuffer;
		Sheet terrainSheet;
		public TileSet tileSet;
		Region region;

		Renderer renderer;
		Map map;

		public TerrainRenderer(Renderer renderer, Map map, Viewport viewport)
		{
			this.renderer = renderer;
			region = Region.Create(viewport, DockStyle.Left, viewport.Width - 128, Draw);
			viewport.AddRegion(region);
			this.map = map;

			tileSet = new TileSet( map.TileSuffix );

			Dictionary<TileReference, Sprite> tileMapping =
				new Dictionary<TileReference, Sprite>();

			Size tileSize = new Size( 24, 24 );

			List<Vertex> vertices = new List<Vertex>();
			List<ushort> indices = new List<ushort>();

			for( int j = 0 ; j < map.Height ; j++ )
				for( int i = 0 ; i < map.Width ; i++ )
				{
					TileReference tileRef = map.MapTiles[ i + map.XOffset, j + map.YOffset ];
					Sprite tile;

					if( !tileMapping.TryGetValue( tileRef, out tile ) )
						tileMapping.Add( tileRef, tile = SheetBuilder.Add( tileSet.GetBytes( tileRef ), tileSize ) );

					terrainSheet = tile.sheet;

					Util.CreateQuad( vertices, indices, 24 * new float2( i, j ), tile, 0 );
				}

			vertexBuffer = new FvfVertexBuffer<Vertex>( renderer.Device, vertices.Count, Vertex.Format );
			vertexBuffer.SetData( vertices.ToArray() );

			indexBuffer = new IndexBuffer( renderer.Device, indices.Count );
			indexBuffer.SetData( indices.ToArray() );
		}

		void Draw()
		{
			int indicesPerRow = map.Width * 6;
			int verticesPerRow = map.Width * 4;

			int visibleRows = (int)(region.Size.Y / 24.0f + 2);

			int firstRow = (int)(region.Location.Y / 24.0f);
			int lastRow = firstRow + visibleRows;

			if (lastRow < 0 || firstRow > map.Height)
				return;

			if (firstRow < 0) firstRow = 0;
			if (lastRow > map.Height) lastRow = map.Height;

			renderer.DrawWithShader(ShaderQuality.Low, delegate
			{
				renderer.DrawBatch(vertexBuffer, indexBuffer, 
					new Range<int>(verticesPerRow * firstRow, verticesPerRow * lastRow), 
					new Range<int>(indicesPerRow * firstRow, indicesPerRow * lastRow), 
					terrainSheet.Texture);
			});
		}
	}
}
