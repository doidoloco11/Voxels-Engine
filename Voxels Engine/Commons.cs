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

        public float ExtraLayeredNoise(Vector2 pos, int layers, int octaves, float baseRoughness, float roughness,
            float persistance)
        {
            float x = pos.X;
            float y = pos.Y;
            for (int i = 0; i < layers; i++)
            {
                float xx = LayeredNoise(new Vector2(x, y), octaves, baseRoughness, roughness, persistance);
                float yy = LayeredNoise(new Vector2(y, x), octaves, baseRoughness, roughness, persistance);

                x = xx;
                y = yy;
            }

            return x;
        }
        
        public float ExtraLayeredNoise(Vector3 pos, int layers, int octaves, float baseRoughness, float roughness,
            float persistance)
        {
            float x = pos.X;
            float y = pos.Y;
            float z = pos.Z;
            for (int i = 0; i < layers; i++)
            {
                float xx = LayeredNoise(new Vector3(x, y, z), octaves, baseRoughness, roughness, persistance);
                float yy = LayeredNoise(new Vector3(y, x, z), octaves, baseRoughness, roughness, persistance);
                float zz = LayeredNoise(new Vector3(z, y, x), octaves, baseRoughness, roughness, persistance);

                x = xx;
                y = yy;
                z = zz;
            }

            return x;
        }
        
        public float ExtraRidgeLayeredNoise(Vector2 pos, int layers, int octaves, float baseRoughness, float roughness,
            float persistance)
        {
            float x = pos.X;
            float y = pos.Y;
            for (int i = 0; i < layers; i++)
            {
                float xx = RidgeLayeredNoise(new Vector2(x, y), octaves, baseRoughness, roughness, persistance);
                float yy = RidgeLayeredNoise(new Vector2(y, x), octaves, baseRoughness, roughness, persistance);

                x = xx;
                y = yy;
            }

            return x;
        }
        
        public float ExtraRidgeLayeredNoise(Vector3 pos, int layers, int octaves, float baseRoughness, float roughness,
            float persistance)
        {
            float x = pos.X;
            float y = pos.Y;
            float z = pos.Z;
            for (int i = 0; i < layers; i++)
            {
                float xx = RidgeLayeredNoise(new Vector3(x, y, z), octaves, baseRoughness, roughness, persistance);
                float yy = RidgeLayeredNoise(new Vector3(y, x, z), octaves, baseRoughness, roughness, persistance);
                float zz = RidgeLayeredNoise(new Vector3(z, y, x), octaves, baseRoughness, roughness, persistance);

                x = xx;
                y = yy;
                z = zz;
            }

            return x;
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
        
        public float RidgeLayeredNoise(Vector2 pos, int octaves, float baseRoughness, float roughness, float persistance)
        {
            float h = 0;
            float frequency = baseRoughness;
            float amplitude = 1;
            
            for (int i = 0; i < octaves; i++)
            {
                h += 1 - Math.Abs((float)Noise.Evaluate(pos.X * frequency, pos.Y * frequency) * amplitude);
                frequency *= roughness;
                amplitude *= persistance;
            }
            
            return h;
        }
        
        public float RidgeLayeredNoise(Vector3 pos, int octaves, float baseRoughness, float roughness, float persistance)
        {
            float h = 0;
            float frequency = baseRoughness;
            float amplitude = 1;
            
            for (int i = 0; i < octaves; i++)
            {
                h += 1 - Math.Abs((float)Noise.Evaluate(pos.X * frequency, pos.Y * frequency, pos.Z * frequency) * amplitude);
                frequency *= roughness;
                amplitude *= persistance;
            }
            
            return h;
        }
    }

    public static Vector3 Round(Vector3 x)
    {
        return new Vector3((float)Math.Round(x.X), (float)Math.Round(x.Y), (float)Math.Round(x.Z));
    }
}