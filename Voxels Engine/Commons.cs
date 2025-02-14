using OpenTK.Mathematics;

public static class Commons
{
    public class NoiseFunctions
    {
        private NoiseTest.OpenSimplexNoise Noise;
        private int seed;

        public NoiseFunctions()
        {
            seed = new Random().Next(-100000000, 100000000);
            Noise = new NoiseTest.OpenSimplexNoise(seed);
        }

        public float LayeredNoise(Vector2 pos, int octaves, float baseRoughness, float roughness, float persistance)
        {
            float h = 0;
            float frequency = baseRoughness;
            float amplitude = 1;
            
            for (int i = 0; i < octaves; i++)
            {
                h += (float)Noise.Evaluate(pos.X * frequency, pos.Y * frequency) * amplitude;
                frequency *= roughness;
                amplitude *= persistance;
            }
            
            return h;
        }
        
        public float LayeredNoise(Vector3 pos, int octaves, float baseRoughness, float roughness, float persistance)
        {
            float h = 0;
            float frequency = baseRoughness;
            float amplitude = 1;
            
            for (int i = 0; i < octaves; i++)
            {
                h += (float)Noise.Evaluate(pos.X * frequency, pos.Y * frequency, pos.Z * frequency) * amplitude;
                frequency *= roughness;
                amplitude *= persistance;
            }
            
            return h;
        }
    }
}