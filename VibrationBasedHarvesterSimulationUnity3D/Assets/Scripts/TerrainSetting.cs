using System.IO;
using UnityEngine;

public class TerrainSetting
{
    private static TerrainSetting terrainSetting;

    private Terrain terrain;
    private int hmWidth;
    private int hmHeight;
    private int m_Resolution;
    private bool m_FlipVertically = false;
    private string filepath;


    public static TerrainSetting getTerrainSetting()
    {
        if (terrainSetting == null)
            terrainSetting = new TerrainSetting();
        return terrainSetting;
    }

    public Terrain getTerrain() { return this.terrain; }
    public int gethmWidth() { return this.hmWidth; }
    public int gethmHeight() { return this.hmHeight; }
    public int gethmResolution() { return this.m_Resolution; }
    public bool getmFlipVertically() { return this.m_FlipVertically; }
    public string getFilePath() { return this.filepath; }

    public void setTerrain(Terrain terrain)
    {
        this.terrain = terrain;
    }

    public void sethmWidth(int width)
    {
        this.hmWidth = width;
    }

    public void sethmHeight(int height)
    {
        this.hmHeight = height;
    }

    public void setmResolution(int resolution)
    {
        this.m_Resolution = resolution;
    }

    public void setmFlipVertically(bool flipVertically)
    {
        this.m_FlipVertically = flipVertically;
    }

    public void setFilePath(string filepath)
    {
        this.filepath = filepath;
    }

    public void setHeightmap()
    {
        // read data
        byte[] data;
        using (BinaryReader br = new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read)))
        {
            data = br.ReadBytes(m_Resolution * m_Resolution);
            br.Close();
        }
        float[,] heights = new float[hmWidth, hmHeight];

        float normalize = 1.0F / (1 << 8);
        for (int y = 0; y < hmHeight; y++)
        {
            for (int x = 0; x < hmHeight; x++)
            {
                int index = Mathf.Clamp(x, 0, m_Resolution - 1) + Mathf.Clamp(y, 0, m_Resolution - 1) * m_Resolution;
                byte compressedHeight = data[index];
                float height = compressedHeight * normalize;
                int destY = m_FlipVertically ? hmHeight - 1 - y : y;
                heights[destY, x] = height;
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);
        terrain.Flush();
    }
}

